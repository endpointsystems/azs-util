namespace azs_util.Table;

[Command(Name = "list-rk", FullName = "list-row-keys", Description = "list RowKey items for a given PartitionKey")]
public class ListRowKeys : BaseCommand
{
    [Option(LongName = "partition-key", ShortName = "p", Description = "The PartitionKey to query.")]
    public string PartitionKey { get; set; }

    [Option(ShortName = "t",LongName = "table", Description = "Table name")]
    public string TableName { get; set; }

    public async Task OnExecute(IConsole console)
    {
        var list = new List<string>();
        var sw = Stopwatch.StartNew();
        try
        {
            var client = new TableServiceClient(ConnectionString);
            var table  = client.GetTableClient(TableName);
            var items = table.QueryAsync<TableEntity>($"PartitionKey eq '{PartitionKey}'");
            var count = 0;

            var token = string.Empty;
            do
            {
                await foreach (var page in items.AsPages(token))
                {
                    foreach (var pageValue in page.Values)
                    {
                        count++;
                        list.Add(pageValue.RowKey);
                    }
                    token = page.ContinuationToken;
                }
            } while (token != null);

            sw.Stop();
            foreach (var item in list)
            {
                console.WriteLine(item);
            }

            console.WriteLine();
            console.WriteLine(
                $"Found {list.Count} items in table {TableName} for PartitionKey {PartitionKey} in {sw.Elapsed.TotalSeconds:N} seconds ({sw.ElapsedMilliseconds}ms)");
        }
        catch (Exception e)
        {
            console.WriteLine(e);
            throw;
        }
    }

    public ListRowKeys(IConsole iConsole) : base(iConsole) { }
}
