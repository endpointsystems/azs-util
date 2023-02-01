using System;
using McMaster.Extensions.CommandLineUtils;

namespace azs_util;

public abstract class BaseCommand
{

    protected readonly IConsole console;

    [Option(ShortName = "cs", LongName = "connection-string", Description = "Storage account connection string. Environment variable: AZURE_STORAGE_CONNECTION_STRING.")]
    public string ConnectionString { get; set; } =
        Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");

    [Option(ShortName = "t",LongName = "table", Description = "Table name")]
    public string TableName { get; set; }

    protected void CheckConnection()
    {
        if (!string.IsNullOrEmpty(ConnectionString)) return;
        ConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
        if (string.IsNullOrEmpty(ConnectionString)) throw new ArgumentException(nameof(ConnectionString));
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
