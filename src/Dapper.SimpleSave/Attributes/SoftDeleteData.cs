namespace Dapper.SimpleSave.Attributes
{
    public class SoftDeleteData
    {
        public SoftDeleteData(bool trueIndicatesDeleted = true)
        {
            TrueIndicatesDeleted = trueIndicatesDeleted;
        }

        public bool TrueIndicatesDeleted { get; }
    }
}