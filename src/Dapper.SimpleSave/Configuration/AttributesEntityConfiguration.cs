using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dapper.SimpleSave.Attributes;
using Dapper.SimpleSave.Impl;
using Dapper.SimpleSave.Metadata;
using Fasterflect;

namespace Dapper.SimpleSave.Configuration
{
    public class AttributesEntityConfiguration<TEntity> : EntityConfiguration<TEntity> where TEntity : class, new()
    {
        private readonly Type _entityType;
        private readonly IDictionary<Type, Func<MemberInfo, IEntityConfiguration<TEntity>, IEntityConfiguration<TEntity>>> _actions = new ConcurrentDictionary<Type, Func<MemberInfo, IEntityConfiguration<TEntity>, IEntityConfiguration<TEntity>>>();

        public AttributesEntityConfiguration()
        {
            _entityType = typeof(TEntity);
            InitActions();
            _entityType.Validate();
        }

        private void InitActions()
        {
            _actions.Add(typeof(TableAttribute), (info, entitity) => entitity.ToTable(info.Attribute<TableAttribute>().SchemaQualifiedTableName));
            _actions.Add(typeof(ColumnAttribute), (info, configuration) => configuration.Column(CreateExpression(info), info.Attribute<ColumnAttribute>().Name));
            _actions.Add(typeof(PrimaryKeyAttribute), (info, configuration) => configuration.WithKey(CreateExpression(info)));
            _actions.Add(typeof(SoftDeleteColumnAttribute), (info, configuration) => configuration.WithSoftDelete(CreateExpression(info), info.Attribute<SoftDeleteColumnAttribute>().TrueIndicatesDeleted));
            _actions.Add(typeof(SimpleSaveIgnoreAttribute), (info, configuration) => configuration.WithReadOnly(CreateExpression(info)));
            _actions.Add(typeof(ReferenceDataAttribute), (info, configuration) => configuration.AsReferenceData(info.Attribute<ReferenceDataAttribute>().HasUpdateableForeignKeys));
            _actions.Add(typeof(OneToManyAttribute), (info, configuration) => configuration.WithOneToMany(info.Attribute<ForeignKeyReferenceAttribute>().ReferencedDto, CreateExpression(info), info.Attribute<ColumnAttribute>().Name));
            _actions.Add(typeof(ManyToOneAttribute), (info, configuration) => configuration.WithManyToOne(info.Attribute<ForeignKeyReferenceAttribute>().ReferencedDto, CreateExpression(info), info.Attribute<ColumnAttribute>().Name));
            _actions.Add(typeof(OneToOneAttribute), (info, configuration) => configuration.WithOneToOne(info.Attribute<ForeignKeyReferenceAttribute>().ReferencedDto, CreateExpression(info), info.Attribute<ColumnAttribute>().Name));
            _actions.Add(typeof(ManyToManyAttribute), (info, configuration) => configuration.WithManyToMany(info.Attribute<ForeignKeyReferenceAttribute>().ReferencedDto, CreateExpression(info), info.Attribute<ColumnAttribute>().Name));
        }

        public override IEntityConfiguration<TEntity> AsDefault()
        {
            //initialize entity
            ProceedAttributes(_entityType);

            //collect properties
            var props = _entityType.Properties(Flags.InstancePublic).ToList();

            props.ForEach(ProceedAttributes);

            return this;
        }

        private void ProceedAttributes(MemberInfo member)
        {
            IList<Attribute> entityAttrs = member.Attributes();

            foreach (var entityAttr in entityAttrs)
            {
                if (_actions.ContainsKey(entityAttr.GetType()))
                {
                    _actions[entityAttr.GetType()](member, this);
                }
            }
        }

        private Expression<Func<TEntity, object>> CreateExpression(MemberInfo member)
        {
            var prop = member as PropertyInfo;

            if (prop == null)
            {
                throw new Exception($"Parameter {nameof(member)} must be {nameof(PropertyInfo)} type");
            }

            var parameter = Expression.Parameter(_entityType, "x");
            Expression constant = Expression.Property(parameter, prop);

            if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                constant = Expression.Convert(constant, typeof(object));
            }

            return Expression.Lambda<Func<TEntity, object>>(constant, new[] { parameter });
        }
    }
}