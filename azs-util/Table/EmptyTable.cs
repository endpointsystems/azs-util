// ReSharper disable ExpressionIsAlwaysNull
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace azs_util.Table;

[Command(Name = "et", FullName = "emptyTable", Description = "Empty a table >= Timestamp (simple date) or use your own custom OData where clause")]
public class EmptyTable : BaseCommand
{
    [Option(ShortName = "d",LongName = "date")]
    public string Date { get; init; }

    [Option(ShortName = "w",LongName = "where",Description = "OData where clause for query")]
    public string Where { get; init; }

    [Required]
    [Option(ShortName = "t",LongName = "table", Description = "Table name")]
    public string TableName { get; init; }

    public async Task OnExecuteAsync()
    {
        var client = new TableServiceClient(ConnectionString);
        var table  = client.GetTableClient(TableName);
        var count = 0;
        var sw = Stopwatch.StartNew();
        var token = string.Empty;
        string where = string.Empty;
        if (!string.IsNullOrEmpty(Where)) where = Where;
        else if (!string.IsNullOrEmpty(Date)) where = $"Timestamp ge datetime'{DateTime.Parse(Date):yyyy-MM-ddTHH:mm:ss.fffffffZ}'";
        var items = table.QueryAsync<TableEntity>(where);

        do
        {
            var batch = new List<TableTransactionAction>();
            string lastPart = string.Empty;
            await foreach (var page in items.AsPages(token))
            {
                foreach (var entity in page.Values)
                {
                    count++;
                    var de = new TableEntity(entity.PartitionKey, entity.RowKey);
                    if (lastPart.Equals(string.Empty))
                    {
                        lastPart = entity.PartitionKey;
                        write($"lastPart is empty, now set to {lastPart}");
                        batch.Add(new TableTransactionAction(TableTransactionActionType.Delete, de));
                        continue;
                    }
                    if (!lastPart.Equals(de.PartitionKey))
                    {
                        lastPart = de.PartitionKey;
                        write($"{de.PartitionKey} != {lastPart}, flushing {batch.Count}");
                        await table.SubmitTransactionAsync(batch);
                        batch.Clear();
                        write($"adding {de.PartitionKey}");
                        batch.Add(new TableTransactionAction(TableTransactionActionType.Delete, de));
                        continue;
                    }
                    batch.Add(new TableTransactionAction(TableTransactionActionType.Delete, de));
                    if (batch.Count < 100) continue;
                    write($"submitting full batch");
                    await table.SubmitTransactionAsync(batch);
                    batch.Clear();
                }

                token = page.ContinuationToken;
            }
            if (batch.Count > 0) await table.SubmitTransactionAsync(batch);
            batch.Clear();
        } while (!string.IsNullOrEmpty(token));

        sw.Stop();
        write($"{TableName}: emptied table {TableName} of {count} records in {sw.Elapsed.TotalSeconds} seconds");

    }

    public EmptyTable(IConsole iConsole) : base(iConsole)
    {
    }
}
