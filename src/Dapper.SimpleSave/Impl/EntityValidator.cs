using System;
using System.Linq;
using Dapper.SimpleSave.Attributes;
using Fasterflect;

namespace Dapper.SimpleSave.Impl
{
    public static class EntityValidator
    {
        public static void Validate(this Type entityType)
        {
            if (!entityType.HasAttribute<TableAttribute>())
            {
                throw new ArgumentException(
                    $"Type {entityType.FullName} is not marked with a [Table] attribute. You must mark it with " +
                    "[Table(\"[schema].[tableName]\")] to use it with Dapper.SimpleSave.",
                    $"type");
            }

            if (entityType.IsEnum) return;

            var primaryKey = entityType.PropertiesWith(Flags.InstancePublic, typeof(PrimaryKeyAttribute))
                .SingleOrDefault();

            if (primaryKey == null)
            {
                throw new ArgumentException(
                    $"Type {entityType.FullName} does not have a property marked with a [PrimaryKey] attribute. You " +
                    "must mark a property of type int?, long? or Guid? with a [PrimaryKey] attribute.",
                    $"type");
            }

            var pkType = primaryKey.PropertyType;
            if (pkType != typeof(int?) && pkType != typeof(long?) && pkType != typeof(Guid?))
            {
                throw new ArgumentException(
                    $"Primary key properties must be of type int?, long? or Guid? but {primaryKey.Name} on {entityType.FullName} " +
                    $"is of type {pkType.FullName}. Either change the type of the property or use a different " +
                    "property as the primary key.",
                    $"type");
            }

            if (!primaryKey.CanWrite)
            {
                throw new ArgumentException(
                    $"Primary key properties must be read-write but {primaryKey.Name} on {entityType.FullName} is read-only. " +
                    "Make the property read-write or choose a different primary key property.");
            }
        }
    }
}