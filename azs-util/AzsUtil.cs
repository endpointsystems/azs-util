namespace azs_util;

[Command(Name = "azs-util", Description = "an Azure storage account utility for blob and table storage.\r\nEndpoint Systems\r\nhttps://endpointsystems.com\r\nFind this tool on GitHub at https://github.com/endpointsystems/azs-util")]
[Subcommand(typeof(Table.Table), typeof(Containers.Containers))]
public class AzsUtil
{
    private const string help = "-?|-h|--help";

    public void OnExecute(CommandLineApplication app)
    {
        app.ShowHelp();
    }
}
