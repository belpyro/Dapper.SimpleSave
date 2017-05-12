using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Dapper.SimpleSave.Configuration
{
    public class ConfigurationExtensions<TEntity> where TEntity : class, new()
    {
        private IConfiguration<TEntity> _configuration;

        public ConfigurationExtensions<TEntity> ToTable(string name = null)
        {
            _configuration = new SampleConfiguration<TEntity> { TableName = string.IsNullOrEmpty(name) ? typeof(TEntity).Name : name };
            return this;
        }

        public ConfigurationExtensions<TEntity> WithKey(Expression<Func<TEntity, object>> expr)
        {
            _configuration.Key = GetMemberExpression(expr).Member.Name;
            return this;
        }

        public ConfigurationExtensions<TEntity> WithOneToOne<TRelation>(Expression<Func<TEntity, object>> expr, string columnName)
        {
            var memberExpr = GetMemberExpression(expr);

            var relation = new MemberRelation()
            {
                ColumnName = columnName,
                To = typeof(TRelation),
                Relation = RelationType.OneToOne,
                PropertyName = memberExpr.Member.Name
            };

            _configuration.Relations.Add(relation);
            return this;
        }

        public ConfigurationExtensions<TEntity> WithOneToMany<TRelation>(Expression<Func<TEntity, object>> expr, string columnName)
        {
            var memberExpr = GetMemberExpression(expr);

            var relation = new MemberRelation()
            {
                ColumnName = columnName,
                To = typeof(TRelation),
                Relation = RelationType.OneToMany,
                PropertyName = memberExpr.Member.Name
            };

            _configuration.Relations.Add(relation);
            return this;
        }

        public ConfigurationExtensions<TEntity> WithManyToOne<TRelation>(Expression<Func<TEntity, object>> expr, string columnName)
        {
            var memberExpr = GetMemberExpression(expr);

            var relation = new MemberRelation()
            {
                ColumnName = columnName,
                To = typeof(TRelation),
                Relation = RelationType.ManyToOne,
                PropertyName = memberExpr.Member.Name
            };

            _configuration.Relations.Add(relation);
            return this;
        }

        public ConfigurationExtensions<TEntity> WithManyToMany<TRelation>(Expression<Func<TEntity, object>> expr, string columnName)
        {
            var memberExpr = GetMemberExpression(expr);

            var relation = new MemberRelation()
            {
                ColumnName = columnName,
                To = typeof(TRelation),
                Relation = RelationType.ManyToMany,
                PropertyName = memberExpr.Member.Name
            };

            _configuration.Relations.Add(relation);
            return this;
        }

        public ConfigurationExtensions<TEntity> WithSoftDelete(Expression<Func<TEntity, object>> expr, bool deletedIfTrue = true)
        {
            var memberExpr = GetMemberExpression(expr);

            return this;
        }

        public ConfigurationExtensions<TEntity> WithIgnore(params Expression<Func<TEntity, object>>[] expr)
        {
            return this;
        }

        public ConfigurationExtensions<TEntity> WithReadOnly(params Expression<Func<TEntity, object>>[] expr)
        {
            return this;
        }

        public ConfigurationExtensions<TEntity> Column(Expression<Func<TEntity, object>> expr, string name)
        {
            return this;
        }


        private static MemberExpression GetMemberExpression(LambdaExpression expr)
        {
            if (expr.Body is MemberExpression)
            {
                return expr.Body as MemberExpression;
            }

            return ((UnaryExpression)expr.Body).Operand as MemberExpression;
        }
    }

    public interface IConfiguration<T>
    {
        string TableName { get; set; }

        string Key { get; set; }

        Type EntityType { get; }

        IList<MemberRelation> Relations { get; }
    }

    public class MemberRelation
    {
        public RelationType Relation { get; set; }

        public Type To { get; set; }

        public string ColumnName { get; set; }

        public string PropertyName { get; set; }
    }

    public enum RelationType
    {
        OneToOne,
        OneToMany,
        ManyToOne,
        ManyToMany
    }

    public class SampleConfiguration<T> : IConfiguration<T>
    {
        public string TableName { get; set; }

        public string Key { get; set; }

        public Type EntityType { get; } = typeof(T);

        public IList<MemberRelation> Relations { get; } = new List<MemberRelation>();
    }
}
