namespace Dapper.SimpleSave.Configuration
{
    public static class SimpleSaveConfiguration
    {
       public static ISimpleConfiguration Configuration { get; set; } = new BasicConfiguration();
    }
}