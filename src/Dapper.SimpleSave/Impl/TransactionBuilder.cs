﻿using System.Collections.Generic;
using Dapper.SimpleSave.Metadata;

namespace Dapper.SimpleSave.Impl
{
    public class TransactionBuilder
    {

        private readonly DtoMetadataCache _dtoMetadataCache;

        public TransactionBuilder(DtoMetadataCache dtoMetadataCache)
        {
            _dtoMetadataCache = dtoMetadataCache;
        }

        public IList<IScript> BuildUpdateScripts<T>(T oldObject, T newObject, bool softDelete = false)
        {
            var differ = new Differ(_dtoMetadataCache);
            var differences = differ.Diff(oldObject, newObject, softDelete);

            if (FireDifferenceProcessedEvents(differences))
            {
                differences = differ.Diff(oldObject, newObject, softDelete);
            }

            var operationBuilder = new OperationBuilder();
            var operations = operationBuilder.Build(differences);

            var commandBuilder = new CommandBuilder();
            var commands = commandBuilder.Coalesce(operations);

            var scriptBuilder = new ScriptBuilder(_dtoMetadataCache);
            var scripts = scriptBuilder.Build(commands);
            return scripts;
        }

        private bool FireDifferenceProcessedEvents(IList<Difference> differences)
        {
            bool furtherChangesMade = false;

            foreach (var difference in differences)
            {
                var args = new DifferenceEventArgs(difference);
                SimpleSaveExtensions.OnDifferenceProcessed(args);
                if (args.HaveFurtherChangesBeenMade)
                {
                    furtherChangesMade = true;
                }
            }

            return furtherChangesMade;
        }
    }
}
