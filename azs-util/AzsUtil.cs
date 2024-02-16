namespace azs_util;

[Command(Name = "azs-util", Description = "an Azure storage account utility for blob and table storage")]
[Subcommand(typeof(Table.Table), typeof(Containers.Containers))]
public class AzsUtil
{
    private const string help = "-?|-h|--help";

    public void OnExecute(CommandLineApplication app)
    {
        app.ShowHelp();
    }
}
