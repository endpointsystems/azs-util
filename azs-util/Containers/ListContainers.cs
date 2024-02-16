namespace azs_util.Containers;

[Command(Name = "list", Description = "List blob storage container names.")]
public class ListContainers: BaseCommand
{

    [Option(ShortName = "d", LongName = "deleted", Description = "show deleted containers (marked with asterisk)")]
    public bool ListDeleted { get; set; }

    [Option(ShortName = "p", LongName = "public", Description = "mark public containers (display options)")]
    public bool ListPublic { get; set; }

    public async Task OnExecute(IConsole console)
    {
        var sw = new Stopwatch();
        sw.Start();
        if (string.IsNullOrEmpty(ConnectionString))
        {
            ConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            if (string.IsNullOrEmpty(ConnectionString)) throw new ArgumentException(nameof(ConnectionString));
        }


        var bc = new BlobServiceClient(ConnectionString);
        var token = string.Empty;

        int deleted = 0;
        int isPublic = 0;
        int count = 0;

        do
        {
            var results = bc.GetBlobContainersAsync(BlobContainerTraits.Metadata)
                .AsPages(token);
            await foreach (var cp in results)
            {
                foreach (var item in cp.Values)
                {
                    var sb = new StringBuilder(item.Name);
                    if (ListDeleted && item.IsDeleted.HasValue && item.IsDeleted.Value)
                    {
                        sb.Append("*");
                        deleted++;
                    }

                    if (ListPublic)
                    {
                        if (item.Properties.PublicAccess != PublicAccessType.Blob)
                            sb.Append($" (public {item.Properties.PublicAccess.ToString()})");
                        console.WriteLine(sb.ToString());
                        isPublic++;
                    }
                    else
                    {
                        console.WriteLine(sb.ToString());
                        count++;
                    }
                }
            }
        } while (token != string.Empty);
        sw.Stop();

        var ssb = new StringBuilder();
        ssb.Append($"operation counted {isPublic + count + deleted} containers");
        if (ListDeleted) ssb.Append($" ({deleted} deleted");
        if (ListDeleted && ListPublic) ssb.Append($", {isPublic} public)");
        else ssb.Append(")");
        ssb.Append($" in {sw.Elapsed.TotalSeconds} seconds.");
        console.WriteLine(ssb.ToString());
    }

    public ListContainers(IConsole iConsole) : base(iConsole) { }
}
