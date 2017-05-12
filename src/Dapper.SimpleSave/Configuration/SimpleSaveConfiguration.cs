using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Fasterflect;

namespace Dapper.SimpleSave.Configuration
{
    public static class SimpleSaveConfiguration
    {
        public static ISimpleConfiguration Configuration { get; set; } = new BasicConfiguration();

        internal static IDictionary<string, IEntityConfiguration<object>> EntityConfigurations => new ConcurrentDictionary<string, IEntityConfiguration<object>>();

        public static IEntityConfiguration<TEntity> GetEntityConfig<TEntity>() where TEntity : class, new()
        {
            return EntityConfigurations.ContainsKey(nameof(TEntity)) ? (IEntityConfiguration<TEntity>)EntityConfigurations[nameof(TEntity)] : new EntityConfiguration<TEntity>();
        }

        public static IConfiguration GetEntityConfig(Type entityType)
        {
            var method = typeof(SimpleSaveConfiguration).GetMethods().Single(mi => mi.Name == "GetEntityConfig" && !mi.Parameters().Any()).MakeGenericMethod(entityType);
            return ((dynamic) method.Invoke(null, null)).Configuration;
        }

        public static void Update<TEntity>(this IEntityConfiguration<TEntity> entity) where TEntity : class, new()
        {
            if (EntityConfigurations.ContainsKey(nameof(TEntity)))
            {
                EntityConfigurations[nameof(TEntity)] = (IEntityConfiguration<object>)entity;
                return;
            }

            EntityConfigurations.Add(nameof(TEntity), (IEntityConfiguration<object>)entity);
        }
    }
}