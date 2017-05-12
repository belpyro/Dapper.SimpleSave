namespace Dapper.SimpleSave.Attributes
{
    public sealed class ReferenceData
    {
        public ReferenceData(bool hasUpdateableForeignKeys = false)
        {
            HasUpdateableForeignKeys = hasUpdateableForeignKeys;
        }

        public bool HasUpdateableForeignKeys { get; private set; }
    }
}