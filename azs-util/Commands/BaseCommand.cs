using System;
using McMaster.Extensions.CommandLineUtils;

namespace ats_util.Commands
{
    public abstract class BaseCommand
    {

        [Option(ShortName = "cs", LongName = "connection-string", Description = "Storage account connection string. Environment variable: AZURE_STORAGE_CONNECTION_STRING.")]
        public string ConnectionString { get; set; } =
            Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");

        [Option(ShortName = "t",LongName = "table", Description = "Table name")]
        public string TableName { get; set; }
    }
}
