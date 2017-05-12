﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Dapper.SimpleSave.Attributes;
using Dapper.SimpleSave.Metadata;
using Newtonsoft.Json;

namespace Dapper.SimpleSave.Impl {
    public class ScriptBuilder
    {
        private readonly DtoMetadataCache _dtoMetadataCache;

        public ScriptBuilder(DtoMetadataCache dtoMetadataCache)
        {
            _dtoMetadataCache = dtoMetadataCache;
        }

        public IList<IScript> Build(IEnumerable<BaseCommand> commands)
        {
            var scripts = new List<IScript>();
            BuildInternal(commands, scripts);
            return scripts;
        }

        public void BuildInternal(IEnumerable<BaseCommand> commands, IList<IScript> scripts)
        {
            int paramIndex = 0;
            Script script = null;
            foreach (var command in commands)
            {
                if (script == null)
                {
                    script = new Script();
                    scripts.Add(script);
                }

                if (command is UpdateCommand)
                {
                    AppendUpdateCommand(script, command as UpdateCommand, ref paramIndex);
                }
                else if (command is InsertCommand)
                {
                    var isPkAssignedByRdbms = AppendInsertCommand(script, command as InsertCommand, ref paramIndex);

                    if (isPkAssignedByRdbms)    //  Don't need to terminate batch with user assigned PK
                    {
                        SimpleSaveExtensions.Logger.LogBuilt(script);
                        script = null;
                    }
                }
                else if (command is DeleteCommand)
                {
                    AppendDeleteCommand(script, command as DeleteCommand, ref paramIndex);
                }
            }

            if (script != null)
            {
                SimpleSaveExtensions.Logger.LogBuilt(script);
            }
        }

        private static void AppendUpdateCommand(Script script, UpdateCommand command, ref int paramIndex)
        {
            if (ReverseUpdateHelper.IsReverseUpdateWithChildReferencingParent(command.Operations.First()))
            {
                AppendReverseUpdateCommandForChildTableReferencingParent(script, command, ref paramIndex);
            }
            else
            {
                AppendStandardUpdateCommand(script, command, ref paramIndex);
            }
        }

        private static void AppendReverseUpdateCommandForChildTableReferencingParent(
            Script script,
            UpdateCommand command,
            ref int paramIndex)
        {
            var operation = command.Operations.FirstOrDefault();
            var fkPropertyMetadata = operation.ValueMetadata.GetForeignKeyColumnFor(
                operation.OwnerMetadata.DtoType);
            script.Buffer.Append(string.Format(@"UPDATE {0}
SET [{1}] = ",
                operation.ValueMetadata.TableName,
                fkPropertyMetadata.ColumnName));

            var valueFunc = new Func<object>(() => command.PrimaryKeyAsObject);
            var wireUpAction = new Action(() =>
            {
                object newPropertyValue = valueFunc();
                if (CanAssignToProperty(newPropertyValue, fkPropertyMetadata))
                {
                    //  This will wire the correct FK column value into the child object.
                    fkPropertyMetadata.Prop.SetValue(operation.Value, newPropertyValue);
                }
            });
            FormatWithParameter(
                script,
                "{0}",
                ref paramIndex,
                wireUpAction,
                valueFunc);

            script.Buffer.Append(string.Format(@"
WHERE [{0}] = ", operation.ValueMetadata.PrimaryKey.ColumnName));

            FormatWithParameter(script, @"{0};
",
                ref paramIndex,
                null,   //  Don't need to do any wire up for this since we already have the PK on the child.
                operation.Value is Func<object>
                    ? operation.Value
                    : new Func<object>(() => operation.ValueMetadata.GetPrimaryKeyValueAsObject(operation.Value)));
        }

        private static void AppendStandardUpdateCommand(
            Script script,
            UpdateCommand command,
            ref int paramIndex)
        {
            script.Buffer.Append(string.Format(@"UPDATE {0}
SET ", command.TableName));

            int count = 0;
            var columnsSeenBefore = CreateColumnsSeenBefore();
            foreach (var operation in command.Operations)
            {
                if (count > 0)
                {
                    script.Buffer.Append(@",
    ");
                }

                CheckAndThrowIfThisColumnHasBeenSeenBefore(
                    columnsSeenBefore, command.TableName, operation.ColumnName, command);

                script.Buffer.Append(string.Format(@"[{0}] = ", operation.ColumnName));
                bool useKey = false;
                string fkTargetColumn = null;

                if (null != operation.ValueMetadata)
                {
                    if ((null != operation.OwnerPropertyMetadata
                     && (operation.OwnerPropertyMetadata.HasAttribute<OneToOneAttribute>()
                         || operation.OwnerPropertyMetadata.HasAttribute<ManyToOneAttribute>()
                         || operation.OwnerPropertyMetadata.HasAttribute<ReferenceDataAttribute>())))
                    {
                        var manyToOne = operation.OwnerPropertyMetadata.GetAttribute<ManyToOneAttribute>();
                        if (manyToOne != null && manyToOne.ForeignKeyTargetColumnName != null)
                        {
                            fkTargetColumn = manyToOne.ForeignKeyTargetColumnName;
                        }
                        else
                        {
                            useKey = true;
                        }
                    }
                    else if (operation.ValueMetadata.HasAttribute<ReferenceDataAttribute>())
                    {
                        useKey = true;
                    }
                }

                if (useKey)
                {
                    var valueFunc = operation.Value == null
                        ? null
                        : (operation.Value is Func<object>
                            ? (Func<object>) operation.Value
                            : () => operation.ValueMetadata.GetPrimaryKeyValueAsObject(operation.Value));

                    var wireUpAction = valueFunc == null
                        ? null
                        : new Action(() =>
                        {
                            var newPropertyValue = valueFunc();
                            if (CanAssignToProperty(newPropertyValue, operation.ColumnPropertyMetadata))
                            {
                                //  Here we're setting the foreign key pointing at the child object on the owner.
                                operation.ColumnPropertyMetadata.Prop.SetValue(operation.Owner, newPropertyValue);
                            }
                        });

                    FormatWithParameter(
                        script,
                        "{0}",
                        ref paramIndex,
                        wireUpAction,
                        valueFunc);
                }
                else if (fkTargetColumn != null)
                {

                    var property = operation.ValueMetadata[fkTargetColumn];
                    if (property == null)
                    {
                        throw new ArgumentException(string.Format(
                            "Cannot UPDATE foreign key relationship for non existent target column '{0}'"
                            + " specified from column '{1}' on type '{2}'.",
                            fkTargetColumn,
                            operation.OwnerPropertyMetadata.ColumnName,
                            operation.ValueMetadata.DtoType));
                    }

                    var valueFunc = operation.Value == null
                        ? null
                        : (operation.Value is Func<object>
                            ? (Func<object>) operation.Value
                            : () => property.GetValue(operation.Value));

                    var wireUpAction = valueFunc == null
                        ? null
                        : new Action(() =>
                        {
                            var newPropertyValue = valueFunc();
                            if (CanAssignToProperty(newPropertyValue, operation.ColumnPropertyMetadata))
                            {
                                //  Here we're setting the foreign key pointing at the child's fkTargetColumn object on the owner.
                                operation.ColumnPropertyMetadata.Prop.SetValue(operation.Owner, newPropertyValue);
                            }
                        });

                    FormatWithParameter(
                        script,
                        "{0}",
                        ref paramIndex,
                        wireUpAction,
                        valueFunc);
                }
                else
                {
                    FormatWithParameter(
                        script,
                        "{0}",
                        ref paramIndex,
                        null,   //  Standard column value so no wire-up required
                        operation.Value);
                }
                ++count;
            }

            script.Buffer.Append(string.Format(@"
WHERE [{0}] = ", command.PrimaryKeyColumn));
            FormatWithParameter(script, @"{0};
",
                ref paramIndex,
                null,   //  No need to wire this up to anything else.
                new Func<object>(() => command.PrimaryKeyAsObject));
        }

        private static void AppendDeleteCommand(Script script, DeleteCommand command, ref int paramIndex)
        {
            var operation = command.Operation;
            if (operation.ValueMetadata != null)
            {
                if (null != operation.OwnerPropertyMetadata
                    && operation.OwnerPropertyMetadata.HasAttribute<ManyToManyAttribute>())
                {
                    //  Remove record in link table; don't touch either entity table

                    script.Buffer.Append(string.Format(
                        @"DELETE FROM {0}
WHERE [{1}] = ", 
                        operation.OwnerPropertyMetadata.GetAttribute<ManyToManyAttribute>().SchemaQualifiedLinkTableName,
                        operation.OwnerPrimaryKeyColumn));

                    FormatWithParameter(
                        script,
                        "{0} AND ",
                        ref paramIndex,
                        null,   //  No wire up required for DELETE from link table.
                        new Func<object>(() => operation.OwnerPrimaryKeyAsObject));

                    script.Buffer.Append(string.Format("[{0}] = ", operation.ValueMetadata.PrimaryKey.Prop.Name));

                    FormatWithParameter(script, @"{0};
",
                        ref paramIndex,
                        null,   //  No wire up required for DELETE from link table.
                        new Func<object>(() => operation.ValueMetadata.GetPrimaryKeyValueAsObject(operation.Value)));
                }
                else if (operation.OwnerPropertyMetadata == null
                    || ((operation.OwnerPropertyMetadata.HasAttribute<OneToManyAttribute>()
                        || operation.OwnerPropertyMetadata.HasAttribute<OneToOneAttribute>())
                    && !operation.ValueMetadata.HasAttribute<ReferenceDataAttribute>()
                    && !operation.OwnerPropertyMetadata.HasAttribute<ReferenceDataAttribute>()))
                {
                    //  DELETE the value from the other table

                    script.Buffer.Append(string.Format(
                        @"DELETE FROM {0}
WHERE [{1}] = ",
                        operation.ValueMetadata.TableName,
                        operation.ValueMetadata.PrimaryKey.Prop.Name));
                    FormatWithParameter(script, @"{0};
",
                        ref paramIndex,
                        null,   //  Methinks no wire-up required here either, although with 1:1 relationships where the FK is on the
                                //  child there's always the possibility we might need to set the column value on the parent to null.
                                //  Possible TODO
                        new Func<object>(() => operation.ValueMetadata.GetPrimaryKeyValueAsObject(operation.Value)));
                }
            }
            else
            {
                throw new ArgumentException(
                    string.Format(
                        "Invalid DELETE command: {0}",
                        JsonConvert.SerializeObject(command)),
                    "command");
            }
        }

        private bool AppendInsertCommand(Script script, InsertCommand command, ref int paramIndex)
        {
            var isPkAssignedByRdbms = true;
            var needsUpdateContingency = false;
            PropertyMetadata guidPKColumn = null;
            var operation = command.Operation;
            if (operation.ValueMetadata != null) {
                if (null != operation.OwnerPropertyMetadata
                    && operation.OwnerPropertyMetadata.HasAttribute<ManyToManyAttribute>())
                {
                    InsertRecordInLinkTable(script, ref paramIndex, operation);
                    script.Buffer.Append(@"
SELECT SCOPE_IDENTITY();
");
                }
                else if (operation.OwnerPropertyMetadata == null
                    || ((operation.OwnerPropertyMetadata.HasAttribute<OneToManyAttribute>()
                        || operation.OwnerPropertyMetadata.HasAttribute<OneToOneAttribute>())//IsOneToOneRelationshipWithFkOnParent(operation)) //  Because 1:1 with FK on child is like 1:N, and we already handle 1:1 with FK on parent anyway
                        && !operation.ValueMetadata.HasAttribute<ReferenceDataAttribute>()
                        && !operation.OwnerPropertyMetadata.HasAttribute<ReferenceDataAttribute>())) 
                {
                    //  INSERT the value into the table defined by ValueMetadata
                    
                    var colBuff = new StringBuilder();
                    var valBuff = new StringBuilder();
                    var values = new ArrayList();
                    var index = 0;

                    var updateCommand = new UpdateCommand();
                    var hasPrimaryKeyValueAlready = false;
                    string pkColumnName = null;

                    var columnsSeenBefore = CreateColumnsSeenBefore();
                    foreach (var property in operation.ValueMetadata.WriteableProperties)
                    {
                        if (property.IsPrimaryKey)
                        {
                            isPkAssignedByRdbms = !property.GetAttribute<PrimaryKeyAttribute>().IsUserAssigned;
                            pkColumnName = property.ColumnName;

                            var pkValue = operation.ValueMetadata.GetPrimaryKeyValueAsObject(operation.Value);
                            if (pkValue != null)
                            {
                                hasPrimaryKeyValueAlready = true;
                            }

                            //updateCommand.AddOperation(new UpdateOperation()
                            //{
                            //    ColumnName = property.ColumnName,
                            //    TableName = operation.ValueMetadata.TableName,
                            //    Value = pkValue,
                            //    ValueMetadata = _dtoMetadataCache.GetMetadataFor(property.Prop.PropertyType),
                            //    Owner = operation.Owner,
                            //    OwnerMetadata = operation.OwnerMetadata,
                            //    OwnerPrimaryKeyColumn = operation.ValueMetadata.PrimaryKey.ColumnName,
                            //    OwnerPropertyMetadata = property
                            //});

                            if (isPkAssignedByRdbms)
                            {
                                var type = property.Prop.PropertyType;
                                if (type != typeof (int)
                                    && type != typeof (int?)
                                    && type != typeof (long)
                                    && type != typeof (long?))
                                {
                                    if (property.Prop.PropertyType == typeof (Guid?)
                                        || property.Prop.PropertyType == typeof (Guid))
                                    {
                                        guidPKColumn = property;
                                    }
                                    else
                                    {
                                        throw new ArgumentException(string.Format(
                                            "Unsupported primary key type {0} on entity {1}. Primary keys must be nullable ints, longs or GUIDs.",
                                            type.FullName,
                                            operation.ValueMetadata.DtoType.FullName));
                                    }
                                }

                                continue;
                            }
                        }

                        var getter = property.Prop.GetGetMethod();

                        if (getter == null
                            || property.HasAttribute<ManyToManyAttribute>()
                            || property.HasAttribute<OneToManyAttribute>()
                            || ! property.IsSaveable)
                        {
                            continue;
                        }

                        //updateCommand.AddOperation(new UpdateOperation()
                        //{
                        //    ColumnName = property.ColumnName,
                        //    TableName = operation.ValueMetadata.TableName,
                        //    Value = operation.Value,
                        //    ValueMetadata = operation.ValueMetadata,
                        //    Owner = operation.Owner,
                        //    OwnerMetadata = operation.OwnerMetadata,
                        //    OwnerPrimaryKeyColumn = operation.OwnerPrimaryKeyColumn,
                        //    OwnerPropertyMetadata = operation.OwnerPropertyMetadata
                        //});

                        CheckAndThrowIfThisColumnHasBeenSeenBefore(
                            columnsSeenBefore, operation.ValueMetadata.TableName, property.ColumnName, command);

                        AppendPropertyToInsertStatement(
                            script, colBuff, valBuff, property, ref index, operation, values, getter, updateCommand);
                    }

                    needsUpdateContingency = !isPkAssignedByRdbms || hasPrimaryKeyValueAlready;

                    if (needsUpdateContingency)
                    {
                        script.Buffer.Append(
                            string.Format(@"IF EXISTS (SELECT * FROM {0} WHERE [{1}] = ",
                            operation.ValueMetadata.TableName,
                            pkColumnName));

                        //  N.B. The PK value doesn't need referencing with a function here because
                        //  we already have a primary key value.
                        FormatWithParameter(
                            script,
                            "{0}",
                            ref paramIndex,
                            null, //  No wire-up required for the IF EXISTS value
                            operation.ValueMetadata.GetPrimaryKeyValueAsObject(operation.Value));

                        script.Buffer.Append(@")
BEGIN
");
                        AppendUpdateCommand(script, updateCommand, ref paramIndex);

                        if (guidPKColumn == null)
                        {
                            script.Buffer.Append(string.Format(@"
SELECT {0};
", operation.ValueMetadata.GetPrimaryKeyValueAsObject(operation.Value)));
                        }
                        else
                        {
                            script.Buffer.Append(string.Format(@"
SELECT '{0}';
", operation.ValueMetadata.GetPrimaryKeyValueAsObject(operation.Value)));
                        }

                        script.Buffer.Append(@"
END
ELSE
BEGIN
");
                    }

                    script.Buffer.Append(string.Format(
                    @"INSERT INTO {0} (
    {1}
)",
                        operation.ValueMetadata.TableName,
                        colBuff));

                    if (guidPKColumn != null && isPkAssignedByRdbms)
                    {
                        script.Buffer.Append(string.Format(@"
OUTPUT inserted.[{0}]
",
                            guidPKColumn.ColumnName));
                    }

                    script.Buffer.Append(@" VALUES (
    ");

                    FormatWithParameter(script, valBuff.ToString(), ref paramIndex, null, values.ToArray());
                    script.Buffer.Append(@"
);
");
                    script.InsertedValue = operation.Value;
                    script.InsertedValueMetadata = operation.ValueMetadata;

                    if (guidPKColumn == null && isPkAssignedByRdbms)
                    {
                        script.Buffer.Append(@"
SELECT SCOPE_IDENTITY();
");
                    }

                    if (needsUpdateContingency)
                    {
                        script.Buffer.Append(@"
END
");
                    }
                }
            }
            else
            {
                throw new ArgumentException(
                    string.Format(
                        "Invalid INSERT command: {0}",
                        JsonConvert.SerializeObject(command)),
                    "command");
            }
            return isPkAssignedByRdbms;
        }

        private static void InsertRecordInLinkTable(Script script, ref int paramIndex, BaseInsertDeleteOperation operation)
        {
            var linkTableName = operation.OwnerPropertyMetadata.GetAttribute<ManyToManyAttribute>().SchemaQualifiedLinkTableName;
            var parentPrimaryKeyColumn = operation.OwnerPrimaryKeyColumn;
            var childPrimaryKeyColumn = operation.ValueMetadata.PrimaryKey.Prop.Name;

            var parentPrimaryKeyGetter = new Func<object>(
                    () => operation.OwnerPrimaryKeyAsObject);

            var childPrimaryKeyGetter = new Func<object>(
                    () => operation.ValueMetadata.GetPrimaryKeyValueAsObject(operation.Value));

            script.Buffer.Append(string.Format("IF NOT EXISTS (SELECT * FROM {0} WHERE [{1}] = ", linkTableName, parentPrimaryKeyColumn));
            FormatWithParameter(script, "{0} AND ", ref paramIndex, null, parentPrimaryKeyGetter);
            script.Buffer.Append(string.Format("[{0}] = ", childPrimaryKeyColumn));
            FormatWithParameter(script, @"{0})
BEGIN
", ref paramIndex, null, childPrimaryKeyGetter);

            script.Buffer.Append(string.Format(
                @"    INSERT INTO {0} (
        [{1}], [{2}]
    ) VALUES (
        ",
                linkTableName,
                parentPrimaryKeyColumn,
                childPrimaryKeyColumn));

            FormatWithParameter(script, @"{0}, {1}
    );
END
",
                ref paramIndex,
                null,   //  No wire-up required for link table
                parentPrimaryKeyGetter,
                childPrimaryKeyGetter);
        }

        //  TODO: I'm wondering whether to reinstate this check purely for the purpose of validation
        //  and similarly to add one for the situation where the FK column is on the child table.
        private static bool IsOneToOneRelationshipWithFkOnParent(BaseInsertDeleteOperation operation)
        {
            if (operation.OwnerPropertyMetadata.HasAttribute<OneToOneAttribute>()
                    && operation.OwnerPropertyMetadata.HasAttribute<ForeignKeyReferenceAttribute>())
            {
                if (operation.OwnerPropertyMetadata.GetAttribute<ForeignKeyReferenceAttribute>().ReferencedDto
                    == operation.ValueMetadata.DtoType)
                {
                    return true;
                }

                throw new ArgumentException(string.Format(
                    "Invalid one to one relationship defined between parent {0} and child {1} "
                    + "on property {2} of parent. "
                    + "Parent has one to one relationship with [ForeignKeyReference] attribute "
                    + "but type specified in [ForeignKeyReference] does not match child type. "
                    + "Instead it is {3}. Change the type referenced by [ForeignKeyReference] "
                    + "or change the type of the property to match that specified in "
                    + "[ForeignKeyReference].",
                    operation.OwnerMetadata.DtoType.FullName,
                    operation.ValueMetadata.DtoType.FullName,
                    operation.OwnerPropertyMetadata.Prop.Name,
                    operation.OwnerPropertyMetadata.GetAttribute<ForeignKeyReferenceAttribute>().ReferencedDto.FullName),
                "operation");
            }
            return false;
        }

        private void AppendPropertyToInsertStatement(
            Script script,
            StringBuilder colBuff,
            StringBuilder valBuff,
            PropertyMetadata property,
            ref int index,
            BaseInsertDeleteOperation operation,
            ArrayList values,
            MethodInfo getter,
            UpdateCommand updateCommand)
        {
            object columnValueForUpdate = null;

            if (property.HasAttribute<ForeignKeyReferenceAttribute>()
                && null != operation.OwnerMetadata
                && _dtoMetadataCache.GetValidatedMetadataFor(
                    property.GetAttribute<ForeignKeyReferenceAttribute>().ReferencedDto).TableName ==
                operation.OwnerMetadata.TableName)
            {
                columnValueForUpdate = new Func<object>(() => operation.OwnerPrimaryKeyAsObject);
                values.Add(columnValueForUpdate);
            }
            else if (property.HasAttribute<ManyToOneAttribute>() && property.GetAttribute<ManyToOneAttribute>().ForeignKeyTargetColumnName != null)
            {
                var propValue = property.GetValue(operation.Value);
                var propTypeMetadata = _dtoMetadataCache.GetValidatedMetadataFor(property.Prop.PropertyType);
                if (null != propValue && null != propTypeMetadata)
                {
                    var targetName = property.GetAttribute<ManyToOneAttribute>().ForeignKeyTargetColumnName;
                    var fkTargetProperty = propTypeMetadata[targetName];
                    if (fkTargetProperty == null)
                    {
                        throw new ArgumentException(string.Format(
                            "Cannot INSERT foreign key value for non existent target column '{0}'"
                            + " specified from column '{1}'.",
                            targetName,
                            property.ColumnName));
                    }

                    columnValueForUpdate = new Func<object>(() => fkTargetProperty.GetValue(propValue));
                    values.Add(columnValueForUpdate);
                }
                else
                {
                    columnValueForUpdate = new Func<object>(() => null);
                    values.Add(columnValueForUpdate);
                }
            }
            else if (property.HasAttribute<ManyToOneAttribute>() || property.HasAttribute<OneToOneAttribute>())
            {
                if (property.HasAttribute<OneToOneAttribute>() && !property.HasAttribute<ForeignKeyReferenceAttribute>())
                {
                    //  One to one relationship where child table references parent rather than the other way around.
                    //  This will be saved along with the child object.
                    return;
                }

                object propValue = property.GetValue(operation.Value);
                DtoMetadata propMetadata = _dtoMetadataCache.GetValidatedMetadataFor(property.Prop.PropertyType);
                columnValueForUpdate = new Func<object>(
                        () =>
                            propValue == null || propMetadata == null
                                ? null
                                : propMetadata.GetPrimaryKeyValueAsObject(propValue));
                values.Add(columnValueForUpdate);
            }
            else
            {
                columnValueForUpdate = getter.Invoke(operation.Value, new object[0]);
                values.Add(columnValueForUpdate);
            }

            if (columnValueForUpdate is Func<object>)
            {
                script.WireUpActions.Add(() =>
                {
                    object newPropertyValue = ((Func<object>) columnValueForUpdate)();
                    if (CanAssignToProperty(newPropertyValue, property))
                    {
                        property.Prop.SetValue(operation.Value, newPropertyValue);
                    }
                });
            }

            updateCommand.AddOperation(new UpdateOperation()
            {
                ColumnPropertyMetadata = property,
                TableName = operation.ValueMetadata.TableName,
                Value = columnValueForUpdate,
                ValueMetadata = property.IsString || property.IsNumericType || property.IsEnum || ! property.IsReferenceType
                    ? null
                    : _dtoMetadataCache.GetMetadataFor(property.Prop.PropertyType),
                Owner = operation.Value,
                OwnerMetadata = operation.ValueMetadata,
                OwnerPrimaryKeyColumn = operation.ValueMetadata.PrimaryKey.ColumnName,
                OwnerPropertyMetadata = property
            });

            if (colBuff.Length > 0) {
                colBuff.Append(@", ");
                valBuff.Append(@", ");
            }

            colBuff.Append("[" + property.ColumnName + "]");

            valBuff.Append("{");
            valBuff.Append(index);
            valBuff.Append("}");

            ++index;
        }

        private static bool CanAssignToProperty(object newPropertyValue, PropertyMetadata property)
        {
            var canAssign = false;
            var propertyType = property.Prop.PropertyType;
            if (newPropertyValue == null)
            {
                if (!propertyType.IsValueType
                    ||
                    (propertyType.IsValueType && propertyType.IsGenericType &&
                     propertyType.GetGenericTypeDefinition() == typeof (Nullable<>)))
                {
                    canAssign = true;
                }
            }
            else if (propertyType.IsAssignableFrom(newPropertyValue.GetType()))
            {
                canAssign = true;
            }
            return canAssign;
        }

        private static void FormatWithParameter(
            Script script,
            string formatString,
            ref int paramIndex,
            Action wireUpAction,
            params object [] paramValues)
        {
            var paramNames = new ArrayList();
            for(int index = 0, count = paramValues.Length; index < count; ++index)
            {
                object paramValue = paramValues[index];
                string paramName = "p" + paramIndex;
                ValidateParameterValue(index, paramName, paramValue);
                script.Parameters[paramName] = paramValue;
                paramNames.Add("@" + paramName);
                ++paramIndex;
            }

            script.Buffer.Append(string.Format(formatString, paramNames.ToArray()));

            if (wireUpAction != null)
            {
                script.WireUpActions.Add(wireUpAction);
            }
        }

        private static void ValidateParameterValue(
            int index,
            string paramName,
            object paramValue)
        {
            if (paramValue == null || paramValue is string)
            {
                return;
            }

            var type = paramValue.GetType();
            if (type.IsValueType
                || (type.IsConstructedGenericType
                    && type.GetGenericTypeDefinition() == typeof(Func<>))) {
                return;
            }

            throw new ArgumentException(
                string.Format(
                    "Reference types other than string are not permitted as parameter values for generated SQL. "
                    + "Invalid value at index {0} for parameter @{1}: {2}",
                    index,
                    paramName,
                    paramValue),
                "paramValue");
        }

        private static ISet<string> CreateColumnsSeenBefore()
        {
            return SimpleSaveExtensions.IsRdbmsCaseSensitive
                ? new HashSet<string>()
                : new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
        }

        private static void CheckAndThrowIfThisColumnHasBeenSeenBefore(
            ISet<string> columnsSeenBefore,
            string tableName,
            string columnName,
            BaseCommand command)
        {
            if (columnsSeenBefore.Contains(columnName))
            {
                throw new InvalidOperationException(string.Format(
                    "Something has gone terribly wrong. We are trying to UPDATE table '{0}' by "
                    + "setting the value of column '{1}' more than once. Command details: {2}",
                    tableName,
                    columnName,
                    JsonConvert.SerializeObject(command, Formatting.Indented)));
            }

            columnsSeenBefore.Add(columnName);
        }
    }
}
