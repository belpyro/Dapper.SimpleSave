using System;
using System.Linq.Expressions;

namespace Dapper.SimpleSave.Configuration
{
    public interface IEntityConfiguration<TEntity>
    {
        IEntityConfiguration<TEntity> ToTable(string name = null);

        IEntityConfiguration<TEntity> WithKey(Expression<Func<TEntity, object>> expr);

        IEntityConfiguration<TEntity> WithOneToOne<TRelation>(Expression<Func<TEntity, object>> expr, string columnName);

        void AddRelationship<TRelation>(Expression<Func<TEntity, object>> expr, string columnName, RelationType type);

        IEntityConfiguration<TEntity> WithOneToMany<TRelation>(Expression<Func<TEntity, object>> expr, string columnName);

        IEntityConfiguration<TEntity> WithManyToOne<TRelation>(Expression<Func<TEntity, object>> expr, string columnName);

        IEntityConfiguration<TEntity> WithManyToMany<TRelation>(Expression<Func<TEntity, object>> expr, string columnName);

        IEntityConfiguration<TEntity> WithOneToOne(Type relationType, Expression<Func<TEntity, object>> expr, string columnName);

        IEntityConfiguration<TEntity> WithOneToMany(Type relationType, Expression<Func<TEntity, object>> expr, string columnName);

        IEntityConfiguration<TEntity> WithManyToOne(Type relationType, Expression<Func<TEntity, object>> expr, string columnName);

        IEntityConfiguration<TEntity> WithManyToMany(Type relationType, Expression<Func<TEntity, object>> expr, string columnName);

        IEntityConfiguration<TEntity> WithSoftDelete(Expression<Func<TEntity, object>> expr, bool deletedIfTrue = true);

        IEntityConfiguration<TEntity> Exclude(params Expression<Func<TEntity, object>>[] expr);

        IEntityConfiguration<TEntity> AsReferenceData(bool hasUpdatableFK = false);

        IEntityConfiguration<TEntity> Test<TResult, TResult2>(TEntity a1);

        IEntityConfiguration<TEntity> WithReadOnly(params Expression<Func<TEntity, object>>[] expr);

        IEntityConfiguration<TEntity> Column(Expression<Func<TEntity, object>> expr, string name);

        IEntityConfiguration<TEntity> AsDefault();

        IConfiguration Configuration { get; }
    }
}