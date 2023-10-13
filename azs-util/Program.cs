using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace azs_util;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("---------------------");
        Console.WriteLine("azs-util: an Azure storage account utility for blob and table storage");
        Console.WriteLine("by Endpoint Systems");
        Console.WriteLine("https://endpointsystems.com");
        Console.WriteLine("Find this tool on GitHub at https://github.com/endpointsystems/azs-util");
        Console.WriteLine("---------------------");
        var app = new CommandLineApplication()
        {
            Name = "azs-util",
            Description = "an Azure storage account utility for blob and table storage"
        };
        app.HelpOption(inherited: true);
        await CommandLineApplication.ExecuteAsync<AzsUtil>(args);
    }
}
