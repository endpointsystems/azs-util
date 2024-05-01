namespace azs_util;
internal class Program
{
    private static async Task Main(string[] args)
    {
        var app = new CommandLineApplication
        {
            Name = "azs-util",
            Description = "an Azure storage account utility for blob and table storage"
        };
        app.HelpOption(inherited: true);
        await CommandLineApplication.ExecuteAsync<AzsUtil>(args);
    }
}
