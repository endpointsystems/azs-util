namespace azs_util;

public abstract class BaseCommand
{

    protected readonly IConsole console;

    [Option(ShortName = "cs", LongName = "connection-string", Description = "Storage account connection string. Environment variable: AZURE_STORAGE_CONNECTION_STRING.")]
    public string ConnectionString { get; set; }

    protected void CheckConnection()
    {
        if (string.IsNullOrEmpty(ConnectionString))
            ConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
    }

    protected BaseCommand(IConsole iConsole)
    {
        console = iConsole;
        CheckConnection();
    }

    protected void write(string msg)
    {
        console.WriteLine(msg);
    }

}
