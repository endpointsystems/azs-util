using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading.Tasks;
using Azure.Data.Tables;
using McMaster.Extensions.CommandLineUtils;

namespace azs_util.Table;

[Command(Name = "cpc", FullName = "copyColumn", Description = "copy one column into a new column")]
public class CopyColumn: BaseCommand
{
    [Required]
    [Option(ShortName = "s",LongName = "source", Description = "the source column name")]
    public string SourceColumn { get; set; }

    [Required]
    [Option(ShortName = "d",LongName = "dest", Description = "the destination column name")]
    public string DestColumn { get; set; }
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
            var newBatch = new List<TableTransactionAction>();

            await foreach (var page in items.AsPages(token))
            {
                foreach (var entity in page.Values)
                {
                    count++;
                    var newEntity = new TableEntity(entity.PartitionKey, entity.RowKey);
                    foreach (var pair in entity)
                    {
                        newEntity.Add(pair.Key,pair.Value);
                    }
                    //add the new column
                    newEntity.Add(DestColumn,entity[SourceColumn]);
                    newBatch.Add(new TableTransactionAction(TableTransactionActionType.UpdateMerge,newEntity));

                    if (newBatch.Count != 100) continue;
                    await table.SubmitTransactionAsync(newBatch);
                    newBatch.Clear();
                }
                token = page.ContinuationToken;
            }
            await table.SubmitTransactionAsync(newBatch);
            newBatch.Clear();
        } while (token != null);
        sw.Stop();
        write($"copied {count} values from {SourceColumn} to {DestColumn} in {sw.Elapsed.TotalSeconds} seconds");
    }

    public CopyColumn(IConsole iConsole) : base(iConsole) { }
}
