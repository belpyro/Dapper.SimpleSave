using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dapper.SimpleSave.Attributes;
using Dapper.SimpleSave.Metadata;
using Fasterflect;

namespace Dapper.SimpleSave.Configuration
{
    public class EntityConfiguration<TEntity> : IEntityConfiguration<TEntity> where TEntity : class, new()
    {
        private IConfiguration _configuration;

        public EntityConfiguration()
        {
            ToTable();
        }

        public IEntityConfiguration<TEntity> ToTable(string name = null)
        {
            _configuration = new BaseConfig<TEntity> { TableName = string.IsNullOrEmpty(name) ? typeof(TEntity).Name : name };
            return this;
        }

        public IEntityConfiguration<TEntity> WithKey(Expression<Func<TEntity, object>> expr)
        {
            _configuration.Key =
                new PropertyMetadataExt(GetMemberExpression(expr).Member as PropertyInfo) { IsPrimaryKey = true };

            return this;
        }

        public IEntityConfiguration<TEntity> WithOneToOne<TRelation>(Expression<Func<TEntity, object>> expr, string columnName)
        {
            WithOneToOne(typeof(TRelation), expr, columnName);
            return this;
        }

        public IEntityConfiguration<TEntity> WithOneToOne(Type relationType, Expression<Func<TEntity, object>> expr, string columnName)
        {
            AddRelationship(relationType, expr, columnName, RelationType.OneToOne);
            return this;
        }

        public void AddRelationship<TRelation>(Expression<Func<TEntity, object>> expr, string columnName, RelationType type)
        {
            AddRelationship(typeof(TRelation), expr, columnName, type);
        }

        public void AddRelationship(Type relationshipType, Expression<Func<TEntity, object>> expr, string columnName, RelationType type)
        {
            var metadata = GetPropMetadata(GetMemberExpression(expr));
            metadata.RelationshipType = type;
            metadata.Relationship = new MemberRelation()
            {
                ColumnName = columnName,
                To = relationshipType,
                PropertyName = metadata.Prop.Name
            };
        }

        public IEntityConfiguration<TEntity> WithOneToMany<TRelation>(Expression<Func<TEntity, object>> expr, string columnName)
        {
            WithOneToMany(typeof(TRelation), expr, columnName);
            return this;
        }

        public IEntityConfiguration<TEntity> WithOneToMany(Type relationType, Expression<Func<TEntity, object>> expr, string columnName)
        {
            AddRelationship(relationType, expr, columnName, RelationType.OneToMany);
            return this;
        }

        public IEntityConfiguration<TEntity> WithManyToOne<TRelation>(Expression<Func<TEntity, object>> expr, string columnName)
        {
            WithManyToOne(typeof(TRelation), expr, columnName);
            return this;
        }

        public IEntityConfiguration<TEntity> WithManyToOne(Type relationType, Expression<Func<TEntity, object>> expr, string columnName)
        {
            AddRelationship(relationType, expr, columnName, RelationType.ManyToOne);
            return this;
        }

        public IEntityConfiguration<TEntity> WithManyToMany<TRelation>(Expression<Func<TEntity, object>> expr, string columnName)
        {
            WithManyToMany(typeof(TRelation), expr, columnName);
            return this;
        }

        public IEntityConfiguration<TEntity> WithManyToMany(Type relationType, Expression<Func<TEntity, object>> expr, string columnName)
        {
            AddRelationship(relationType, expr, columnName, RelationType.ManyToMany);
            return this;
        }

        public IEntityConfiguration<TEntity> WithSoftDelete(Expression<Func<TEntity, object>> expr, bool deletedIfTrue = true)
        {
            var metadata = GetPropMetadata(GetMemberExpression(expr));

            if (!metadata.IsBool) throw new ArgumentException($"SoftDelete property {nameof(metadata.Prop.Name)} must be boolean only!");

            metadata.SoftDelete = new SoftDeleteData(deletedIfTrue);

            return this;
        }

        public IEntityConfiguration<TEntity> Exclude(params Expression<Func<TEntity, object>>[] expr)
        {
            foreach (var expression in expr)
            {
                var metadata = GetPropMetadata(GetMemberExpression(expression));
                metadata.IsIgnored = true;
            }
            return this;
        }

        public IEntityConfiguration<TEntity> AsReferenceData(bool hasUpdatableFK = false)
        {
            _configuration.ReferenceData = new ReferenceData(hasUpdatableFK);
            return this;
        }

        public IEntityConfiguration<TEntity> Test<TResult, TResult2>(TEntity a1)
        {
            throw new NotImplementedException();
        }

        public IEntityConfiguration<TEntity> WithReadOnly(params Expression<Func<TEntity, object>>[] expr)
        {
            foreach (var expression in expr)
            {
                var metadata = GetPropMetadata(GetMemberExpression(expression));
                metadata.IsReadOnly = true;
            }

            return this;
        }

        public IEntityConfiguration<TEntity> Column(Expression<Func<TEntity, object>> expr, string name)
        {
            var metadata = GetPropMetadata(GetMemberExpression(expr));
            metadata.ColumnName = name;
            return this;
        }

        public virtual IEntityConfiguration<TEntity> AsDefault()
        {
            var defaultKey = SimpleSaveConfiguration.Configuration.DefaultKeyName;

            var props = _configuration.DtoType.Properties(Flags.InstancePublic)
                .Where(x => !_configuration.Properties.ContainsKey(x.Name))
                .ToList();

            props.ForEach(p =>
            {
                var metadata = new PropertyMetadataExt(p);

                if (p.Name.Equals(defaultKey, StringComparison.OrdinalIgnoreCase) && _configuration.Key == null)
                {
                    _configuration.Key = new PropertyMetadataExt(p) { IsPrimaryKey = true };
                    return;
                }

                _configuration.Properties.Add(p.Name, metadata);
            });

            if (_configuration.Key == null) throw new Exception($"Primary key for {nameof(TEntity)} not found. Default key name is {defaultKey}");

            return this;
        }

        protected IPropertyMetadataExt GetPropMetadata(MemberExpression memberExpr)
        {
            IPropertyMetadataExt metadata;

            if (_configuration.Key != null && _configuration.Key.Name.Equals(memberExpr.Member.Name))
            {
                return _configuration.Key;
            }

            if (_configuration.Properties.ContainsKey(memberExpr.Member.Name))
            {
                metadata = _configuration.Properties[memberExpr.Member.Name];
            }
            else
            {
                metadata = new PropertyMetadataExt((PropertyInfo)memberExpr.Member);
                _configuration.Properties.Add(memberExpr.Member.Name, metadata);
            }
            return metadata;
        }

        private static MemberExpression GetMemberExpression(LambdaExpression expr)
        {
            MemberExpression expression = expr.Body is MemberExpression ? (MemberExpression)expr.Body : ((UnaryExpression)expr.Body).Operand as MemberExpression;
            var property = expression.Member as PropertyInfo;

            if (!property.IsReadable())
            {
                throw new Exception($"Property {property.Name} must be public");
            }

            return expression;
        }

        public IConfiguration Configuration => _configuration;
    }
}
