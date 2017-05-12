﻿using System;
using System.Collections.Generic;
using Castle.Core.Internal;
using Dapper.SimpleSave.Attributes;
using Dapper.SimpleSave.Metadata;

namespace Dapper.SimpleSave.Impl
{
    public class CommandBuilder
    {
        public IEnumerable<BaseCommand> Coalesce(IEnumerable<BaseOperation> operations)
        {
            var results = new List<BaseCommand>();
            var updates = new List<UpdateOperation>();

            var updateColumns = CreateUpdateColumnSet();
            DtoMetadata updateMetadata = null;
            string updateTableName = null;
            object updatePk = null;

            foreach (var operation in operations)
            {
                if (operation is UpdateOperation)
                {
                    var update = operation as UpdateOperation;
                    var newMetadata = GetNewUpdateMetadata(update);
                    var newTableName = GetNewUpdateTableName(update);
                    var newPrimaryKeyValue = GetNewUpdatePrimaryKeyValue(update);
                    //  If the table name, or row PK changes, we need to apply any UpdateOperations we already have as a new command
                    //  then start tracking UpdateOperations afresh, starting with this one.
                    if (updateTableName != newTableName
                        || updateMetadata != newMetadata
                        || !PrimaryKeyComparer.SuppliedPrimaryKeyValuesMatch(
                                newMetadata,
                                updatePk,
                                newPrimaryKeyValue)
                        || updateColumns.Contains(update.ColumnName))
                    {
                        ValidateUpdateOperation(update);
                        if (null != updateTableName)
                        {
                            ApplyUpdatesSoFarAsNewCommand(results, updates, ref updateTableName, ref updatePk);
                        }

                        updateColumns = CreateUpdateColumnSet();
                        updateMetadata = newMetadata;
                        updateTableName = newTableName;
                        updatePk = newPrimaryKeyValue;
                    }

                    //  TODO: replace UPDATE on same column

                    updateColumns.Add(update.ColumnName);
                    updates.Add(update);
                }
                else
                {
                    ApplyUpdatesSoFarAsNewCommand(results, updates, ref updateTableName, ref updatePk);

                    updateColumns = CreateUpdateColumnSet();
                    updateMetadata = null;
                    updateTableName = null;
                    updatePk = null;

                    ValidateInsertOrDeleteOperation((BaseInsertDeleteOperation) operation);

                    if (operation is InsertOperation)
                    {
                        results.Add(new InsertCommand(operation as InsertOperation));
                    }
                    else if (operation is DeleteOperation)
                    {
                        results.Add(new DeleteCommand(operation as DeleteOperation));
                    }
                }
            }

            if (updates.Count > 0)
            {
                ApplyUpdatesSoFarAsNewCommand(results, updates, ref updateTableName, ref updatePk);
            }

            return results;
        }

        private ISet<string> CreateUpdateColumnSet()
        {
            return SimpleSaveExtensions.IsRdbmsCaseSensitive
                ? new HashSet<string>()
                : new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
        }

        private DtoMetadata GetNewUpdateMetadata(UpdateOperation update)
        {
            return ReverseUpdateHelper.IsReverseUpdateWithChildReferencingParent(update)
                ? update.ValueMetadata
                : update.OwnerMetadata;
        }

        private string GetNewUpdateTableName(UpdateOperation update)
        {
            return ReverseUpdateHelper.IsReverseUpdateWithChildReferencingParent(update)
                ? update.ValueMetadata.TableName
                : update.TableName;
        }

        private object GetNewUpdatePrimaryKeyValue(UpdateOperation update)
        {
            return ReverseUpdateHelper.IsReverseUpdateWithChildReferencingParent(update)
                ? update.ValueMetadata.GetPrimaryKeyValueAsObject(update.Value)
                : update.OwnerPrimaryKeyAsObject;
        }

        private void ValidateInsertOrDeleteOperation(BaseInsertDeleteOperation insert)
        {
            var tableMetadata = insert.ValueMetadata;
            if (tableMetadata == null)
            {
                throw new ArgumentException(
                    "Cannot INSERT or DELETE without metadata for table because we don't know which table we're acting on. "
                    + "BaseInsertDeleteOperation.ValueMetadata must not be null.",
                    "insert");
            }

            // We only need to complain if somebody tries to insert into (or delete from) a top level table at this point.
            // Insertions into child tables will be transformed into inserts into link tables for many to many
            // reference data tables later. Deletes for many to many relationships will likewise act against the
            // link table.
            if (insert.OwnerMetadata == null && tableMetadata.IsReferenceData)
            {
                throw new InvalidOperationException(string.Format(
                    "You cannot INSERT into or DELETE from a reference data table, even if that table contains updateable foreign keys. "
                    + "Invalid table: {0}.",
                    tableMetadata.TableName));
            }
        }

        private void ValidateUpdateOperation(UpdateOperation update)
        {
            var tableMetadata = update.OwnerMetadata;
            if (null != tableMetadata)
            {
                if (tableMetadata.IsReferenceData)
                {
                    var columnMetadata = update.OwnerPropertyMetadata;
                    if (!tableMetadata.HasUpdateableForeignKeys)
                    {
                        throw new InvalidOperationException(string.Format(
                            "You cannot perform UPDATEs on a reference data table with no updateable foreign keys. "
                            + "Attempt was made to UPDATE table {0}, column {1}.",
                            tableMetadata.TableName,
                            columnMetadata == null ? "(unknown column)" : columnMetadata.ColumnName));
                    }

                    if (null != columnMetadata && !columnMetadata.HasAttribute<ForeignKeyReferenceAttribute>())
                    {
                        throw new InvalidOperationException(string.Format(
                            "You cannot UPDATE non-foreign key columns on a reference data table with updateable foreign keys. "
                            + "Attempt was made to UPDATE table {0}, column {1}.",
                            tableMetadata.TableName,
                            columnMetadata.ColumnName));
                    }
                }
            }
        }

        private void ApplyUpdatesSoFarAsNewCommand(
            IList<BaseCommand> results,
            IList<UpdateOperation> updates,
            ref string updateTableName,
            ref object updatePk)
        {
            if (updates.Count == 0)
            {
                return;
            }
            var command = new UpdateCommand();
            updates.ForEach(update => command.AddOperation(update));
            results.Add(command);
            updates.Clear();
            updateTableName = null;
            updatePk = null;
        }
    }
}
