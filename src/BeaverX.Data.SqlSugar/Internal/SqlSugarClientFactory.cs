using System.Reflection;
using BeaverX.Data.SqlSugar.DependencyInjection;
using BeaverX.Domain.Entities;
using BeaverX.Domain.IdGeneration;
using BeaverX.Domain.Users;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace BeaverX.Data.SqlSugar.Internal;

internal static class SqlSugarClientFactory
{
    private static readonly NullabilityInfoContext NullabilityContext = new();

    public static ISqlSugarClient Create(
        IOptions<BeaverXSqlSugarOptions> optionsAccessor,
        ICurrentUser currentUser,
        IIdGenerator<long> longIdGenerator,
        IIdGenerator<Guid> guidIdGenerator)
    {
        var options = optionsAccessor.Value;
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            throw new InvalidOperationException("BeaverX SqlSugar 未配置 ConnectionString。请在 AddBeaverXSqlSugar 中设置。");
        }

        var config = new ConnectionConfig
        {
            ConnectionString = options.ConnectionString,
            DbType = options.DbType,
            IsAutoCloseConnection = options.IsAutoCloseConnection,
            InitKeyType = InitKeyType.Attribute,
            MoreSettings = new ConnMoreSettings
            {
                IsAutoDeleteQueryFilter = true,
                IsAutoUpdateQueryFilter = true,
                PgSqlIsAutoToLower = false,
                PgSqlIsAutoToLowerSchema = false,
                PgSqlIsAutoToLowerCodeFirst = false,
            }
        };

        options.ConfigureConnection?.Invoke(config);
        ConfigureEntityConventions(config);

        var db = new SqlSugarClient(config);
        ConfigureBeaverXFilters(db, currentUser, longIdGenerator, guidIdGenerator, options.NormalizeEntityBeforeWrite);
        options.ConfigureClient?.Invoke(db);
        return db;
    }

    /// <summary>
    /// 实体列约定：
    /// 1. 属性名 Id（不区分大小写）默认识别为主键，且非数据库自增（由IIdGenerator赋值）；
    /// 2. 按 CLR 可空性自动设置列可空，避免 Domain 实体依赖 SqlSugar 特性。
    /// </summary>
    private static void ConfigureEntityConventions(ConnectionConfig config)
    {
        config.ConfigureExternalServices ??= new ConfigureExternalServices();
        var previous = config.ConfigureExternalServices.EntityService;

        config.ConfigureExternalServices.EntityService = (property, column) =>
        {
            previous?.Invoke(property, column);

            if (column.IsIgnore)
            {
                return;
            }

            if (string.Equals(property.Name, "Id", StringComparison.OrdinalIgnoreCase))
            {
                // SqlSugar API 拼写为 IsPrimarykey
                column.IsPrimarykey = true;
                // BeaverX 由 IIdGenerator 发号；若标成 Identity，Insert 不会写入 Id，实体侧常表现为 0
                column.IsIdentity = false;
            }

            if (IsClrNullable(property))
            {
                column.IsNullable = true;
            }
        };
    }

    private static bool IsClrNullable(PropertyInfo property)
    {
        var type = property.PropertyType;
        if (Nullable.GetUnderlyingType(type) != null)
        {
            return true;
        }

        if (type.IsValueType)
        {
            return false;
        }

        var nullability = NullabilityContext.Create(property);
        return nullability.ReadState == NullabilityState.Nullable
            || nullability.WriteState == NullabilityState.Nullable;
    }

    private static void ConfigureBeaverXFilters(
        SqlSugarClient db,
        ICurrentUser currentUser,
        IIdGenerator<long> longIdGenerator,
        IIdGenerator<Guid> guidIdGenerator,
        Action<object?>? normalizeEntityBeforeWrite)
    {
        db.QueryFilter.AddTableFilter<ISoftDelete>(it => it.IsDeleted == false);

        db.Aop.DataExecuting = (_, entityInfo) =>
        {
            var userId = currentUser.Id;

            switch (entityInfo.OperationType)
            {
                case DataFilterType.InsertByObject:
                    ApplyInsertId(entityInfo.EntityValue, longIdGenerator, guidIdGenerator);
                    ApplyInsertAudit(entityInfo.EntityValue, userId);
                    break;
                case DataFilterType.UpdateByObject:
                    ApplyUpdateAudit(entityInfo.EntityValue, userId);
                    break;
            }

            normalizeEntityBeforeWrite?.Invoke(entityInfo.EntityValue);
        };
    }

    private static void ApplyInsertId(
        object? entity,
        IIdGenerator<long> longIdGenerator,
        IIdGenerator<Guid> guidIdGenerator)
    {
        switch (entity)
        {
            case IEntity<long> longEntity when EntityIdHelper.IsDefault(longEntity.Id):
                longEntity.Id = longIdGenerator.Generate();
                break;
            case IEntity<Guid> guidEntity when EntityIdHelper.IsDefault(guidEntity.Id):
                guidEntity.Id = guidIdGenerator.Generate();
                break;
        }
    }

    private static void ApplyInsertAudit(object? entity, long? userId)
    {
        if (entity is IHasCreationTime<long> creation)
        {
            if (creation.CreationTime == default)
            {
                creation.CreationTime = DateTime.Now;
            }

            if (userId.HasValue && creation.CreatorId is null)
            {
                creation.CreatorId = userId.Value;
            }
        }
    }

    private static void ApplyUpdateAudit(object? entity, long? userId)
    {
        if (entity is IHasModificationTime<long> modification)
        {
            modification.LastModificationTime = DateTime.Now;
            if (userId.HasValue)
            {
                modification.LastModifierId = userId.Value;
            }
        }
    }

    internal static void ApplySoftDelete(object entity, long? userId)
    {
        if (entity is not ISoftDelete softDelete)
        {
            return;
        }

        softDelete.IsDeleted = true;

        if (entity is IHasDeletionTime<long> deletion)
        {
            deletion.DeletionTime = DateTime.Now;
            if (userId.HasValue)
            {
                deletion.DeleterId = userId.Value;
            }
        }
    }
}
