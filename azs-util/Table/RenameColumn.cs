namespace azs_util.Table;

[Command(Name = "rc", FullName = "renameColumn", Description = "rename a column in your table")]
public class RenameColumn: BaseCommand
{
    [Required]
    [Option(ShortName = "o",LongName = "oldName", Description = "the old column name")]
    public string OldName { get; set; }

    [Required]
    [Option(ShortName = "n", LongName = "newName", Description = "the new column name")]
    public string NewName { get; set; }

    [Option(ShortName = "t",LongName = "table", Description = "Table name")]
    public string TableName { get; set; }

    public async Task OnExecuteAsync()
    {
        var client = new TableServiceClient(ConnectionString);
        var table  = client.GetTableClient(TableName);
        var items = table.QueryAsync<TableEntity>();
        var count = 0;

        var sw = Stopwatch.StartNew();
        var token = string.Empty;
        do
        {
            var oldBatch = new List<TableTransactionAction>();
            var newBatch = new List<TableTransactionAction>();

            await foreach (var page in items.AsPages(token))
            {
                foreach (var entity in page.Values)
                {
                    count++;
                    var newEntity = new TableEntity(entity.PartitionKey, entity.RowKey);
                    foreach (var pair in entity)
                    {
                        if (pair.Key == OldName)
                        {
                            newEntity.Add(NewName, pair.Value);
                            continue;
                        }
                        newEntity.Add(pair.Key,pair.Value);
                    }
                    oldBatch.Add(new TableTransactionAction(TableTransactionActionType.Delete,entity));
                    newBatch.Add(new TableTransactionAction(TableTransactionActionType.Add,newEntity));

                    if (newBatch.Count != 100) continue;
                    await table.SubmitTransactionAsync(oldBatch);
                    await table.SubmitTransactionAsync(newBatch);
                    newBatch.Clear();
                }
                token = page.ContinuationToken;
            }
            await table.SubmitTransactionAsync(oldBatch);
            await table.SubmitTransactionAsync(newBatch);
            newBatch.Clear();
        } while (token != null);
        sw.Stop();
        write($"{TableName}: renamed column {OldName} to {NewName} with {count} values in {sw.Elapsed.TotalSeconds} seconds");
        write("Please note: if you still see the old column name in Azure Storage Explorer full of null values, it's due to ASE caching the schema. ");
    }

    public RenameColumn(IConsole iConsole) : base(iConsole) { }
}
