using System.Reflection;
using BeaverX.Data.SqlSugar.DependencyInjection;
using BeaverX.Domain.Entities;
using BeaverX.Domain.Users;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace BeaverX.Data.SqlSugar.Internal;

internal static class SqlSugarClientFactory
{
    private static readonly NullabilityInfoContext NullabilityContext = new();

    public static ISqlSugarClient Create(IOptions<BeaverXSqlSugarOptions> optionsAccessor, ICurrentUser currentUser)
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
        ConfigureClrNullableColumns(config);

        var db = new SqlSugarClient(config);
        ConfigureBeaverXFilters(db, currentUser, options.NormalizeEntityBeforeWrite);
        options.ConfigureClient?.Invoke(db);
        return db;
    }

    /// <summary>
    /// 按 CLR 可空性自动设置列可空，避免 Domain 实体依赖 SqlSugar 特性。
    /// 覆盖审计基类中的 DateTime? / long? 以及业务实体中的 string? 等字段。
    /// </summary>
    private static void ConfigureClrNullableColumns(ConnectionConfig config)
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
        Action<object?>? normalizeEntityBeforeWrite)
    {
        db.QueryFilter.AddTableFilter<ISoftDelete>(it => it.IsDeleted == false);

        db.Aop.DataExecuting = (_, entityInfo) =>
        {
            var userId = currentUser.Id;

            switch (entityInfo.OperationType)
            {
                case DataFilterType.InsertByObject:
                    ApplyInsertAudit(entityInfo.EntityValue, userId);
                    break;
                case DataFilterType.UpdateByObject:
                    ApplyUpdateAudit(entityInfo.EntityValue, userId);
                    break;
            }

            normalizeEntityBeforeWrite?.Invoke(entityInfo.EntityValue);
        };
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
