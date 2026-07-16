using BeaverX.Data.SqlSugar.DependencyInjection;
using BeaverX.Domain.Entities;
using BeaverX.Domain.Users;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace BeaverX.Data.SqlSugar.Internal;

internal static class SqlSugarClientFactory
{
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
                IsAutoUpdateQueryFilter = true
            }
        };

        options.ConfigureConnection?.Invoke(config);

        var db = new SqlSugarClient(config);
        ConfigureBeaverXFilters(db, currentUser, options.NormalizeEntityBeforeWrite);
        options.ConfigureClient?.Invoke(db);
        return db;
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
