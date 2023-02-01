using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading.Tasks;
using Azure.Data.Tables;
using McMaster.Extensions.CommandLineUtils;

namespace azs_util.Table;

[Command(Name = "dc", FullName = "deleteColumn", Description = "delete a column in your table")]
public class DeleteColumn: BaseCommand
{
    [Required]
    [Option(ShortName = "c",LongName = "column", Description = "the column name")]
    public string ColumnName { get; set; }
    
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
                        if (pair.Key == ColumnName) continue;
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
        write($"{TableName}: deleted column {ColumnName} with {count} values in {sw.Elapsed.TotalSeconds} seconds");
        write("Please note: if you still see this column in Azure Storage Explorer full of null values, it's due to ASE caching the schema. ");
    }

    public DeleteColumn(IConsole iConsole) : base(iConsole) { }
}
