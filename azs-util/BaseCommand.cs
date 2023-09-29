using System;
using McMaster.Extensions.CommandLineUtils;

namespace azs_util;

public abstract class BaseCommand
{

    protected readonly IConsole console;

    [Option(ShortName = "cs", LongName = "connection-string", Description = "Storage account connection string. Environment variable: AZURE_STORAGE_CONNECTION_STRING.")]
    public string ConnectionString { get; set; }

    [Option(ShortName = "ds", LongName = "dest-connection-string", Description = "Storage account destination connection string. Environment variable: AZURE_STORAGE_CONNECTION_DEST.")]
    public string DestConnectionString { get; set; } =
        Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_DEST");

    [Option(ShortName = "t",LongName = "table", Description = "Table name")]
    public string TableName { get; set; }

    protected void CheckConnection()
    {
        if (string.IsNullOrEmpty(ConnectionString))
            ConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
        if (string.IsNullOrEmpty(ConnectionString)) ArgumentNullException.ThrowIfNull(ConnectionString);
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
