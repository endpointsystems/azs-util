namespace azs_util.Table;

[Command(Name = "dc", FullName = "deleteColumn", Description = "delete a column in your table")]
public class DeleteColumn: BaseCommand
{
    [Required]
    [Option(ShortName = "c",LongName = "column", Description = "the column name")]
    public string ColumnName { get; set; }
    [Required]
    [Option(ShortName = "t",LongName = "table", Description = "Table name")]
    public string TableName { get; set; }

    /// <summary>
    /// Deletes a column in the specified table.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task OnExecuteAsync()
    {
        var client = new TableServiceClient(ConnectionString);
        var table  = client.GetTableClient(TableName);
        var items = table.QueryAsync<TableEntity>();
        var count = 0;

        var sw = Stopwatch.StartNew();
        var token = string.Empty;
        var oldBatch = new List<TableTransactionAction>();
        var newBatch = new List<TableTransactionAction>();
        var currentPartitionKey = string.Empty;

        do
        {
            await foreach (var page in items.AsPages(token))
            {
                foreach (var entity in page.Values)
                {
                    // Check if partition key changed
                    if (!string.IsNullOrEmpty(currentPartitionKey) &&
                        currentPartitionKey != entity.PartitionKey)
                    {
                        // Submit current batches before starting new partition
                        if (oldBatch.Count > 0)
                        {
                            await table.SubmitTransactionAsync(oldBatch);
                            await table.SubmitTransactionAsync(newBatch);
                            oldBatch.Clear();
                            newBatch.Clear();
                        }
                    }

                    currentPartitionKey = entity.PartitionKey;
                    count++;

                    var newEntity = new TableEntity(entity.PartitionKey, entity.RowKey);
                    foreach (var pair in entity)
                    {
                        if (pair.Key == ColumnName) continue;
                        newEntity.Add(pair.Key,pair.Value);
                    }

                    oldBatch.Add(new TableTransactionAction(TableTransactionActionType.Delete,entity));
                    newBatch.Add(new TableTransactionAction(TableTransactionActionType.Add,newEntity));

                    // Submit when batch reaches 100 (Azure limit)
                    if (newBatch.Count >= 100)
                    {
                        await table.SubmitTransactionAsync(oldBatch);
                        await table.SubmitTransactionAsync(newBatch);
                        oldBatch.Clear();
                        newBatch.Clear();
                    }
                }
                token = page.ContinuationToken;
            }

            // Submit any remaining items
            if (oldBatch.Count > 0)
            {
                await table.SubmitTransactionAsync(oldBatch);
                await table.SubmitTransactionAsync(newBatch);
                oldBatch.Clear();
                newBatch.Clear();
            }
        } while (token != null);

        sw.Stop();
        write($"{TableName}: deleted column {ColumnName} with {count} values in {sw.Elapsed.TotalSeconds} seconds");
        write("Please note: if you still see this column in Azure Storage Explorer full of null values, it's due to ASE caching the schema. ");
    }

    public DeleteColumn(IConsole iConsole) : base(iConsole) { }
}
