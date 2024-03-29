namespace azs_util.Containers;

[Command(Name = "delete", Description = "deletes containers from the storage account.")]
public class DeleteContainers: BaseCommand
{

    [Option(ShortName = "a", LongName = "all", Description = "delete all containers in the storage account.")]
    public bool All { get; set; }

    [Option(ShortName = "n", LongName = "name", Description = "delete the container with this name.")]
    public string ContainerName { get; set; }

    public async Task OnExecute(IConsole console)
    {
        var sw = new Stopwatch();
        sw.Start();
        if (string.IsNullOrEmpty(ConnectionString))
        {
            ConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            if (string.IsNullOrEmpty(ConnectionString)) throw new ArgumentException(nameof(ConnectionString));
        }
        if (!All && string.IsNullOrEmpty(ContainerName)) throw new ArgumentException("need the container name");

        var bc = new BlobServiceClient(ConnectionString);
        var token = string.Empty;
        int count = 0;


        do
        {
            var results = bc.GetBlobContainersAsync(BlobContainerTraits.Metadata)
                .AsPages(token);
            await foreach (var cp in results)
            {
                foreach (var item in cp.Values)
                {
                    if (All)
                    {
                        await bc.DeleteBlobContainerAsync(item.Name);
                        count++;
                    }
                    else
                    {
                        if (item.Name.Equals(ContainerName))
                            await bc.DeleteBlobContainerAsync(item.Name);
                        count++;
                    }
                }
            }
        } while (token != string.Empty);
        sw.Stop();
        console.WriteLine($"deleted {count} containers in {sw.Elapsed.TotalSeconds} seconds.");
    }

    public DeleteContainers(IConsole iConsole) : base(iConsole)
    {
    }
}
