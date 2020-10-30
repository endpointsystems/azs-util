using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Azure.Cosmos.Table;
// ReSharper disable ExpressionIsAlwaysNull

namespace ats_util.Commands
{
    [Command(Name = "rename-pk",FullName = "rename-partition-key",Description = "Rename a PartitionKey to something else.")]
    public class RenamePartitionKey: BaseCommand
    {
        [Option(ShortName = "old", LongName = "old-partition", Description = "The old partition key")]
        public string OldPartitionKey { get; set; }

        [Option(ShortName = "new", LongName = "new-partition", Description = "The new partition key")]
        public string NewPartitionKey { get; set; }

        [Option(ShortName = "b", LongName = "batch", Description = "Batch size")]
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public int BatchSize { get; set; } = 100;

        public async Task OnExecute(IConsole console)
        {
            if (string.IsNullOrEmpty(ConnectionString))
            {
                ConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
                if (string.IsNullOrEmpty(ConnectionString)) throw new ArgumentException(nameof(ConnectionString));
            }
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                var csa = CloudStorageAccount.Parse(ConnectionString);
                var client = csa.CreateCloudTableClient();
                var table  = client.GetTableReference(TableName);
                var q = new TableQuery<TableEntity> {FilterString = $"PartitionKey eq '{OldPartitionKey}'"};
                int count = 0;

                TableContinuationToken token = null;
                do
                {
                    var items = await table.ExecuteQuerySegmentedAsync(q, token);

                    var oldBatch = new TableBatchOperation();
                    var newBatch = new TableBatchOperation();
                    var context = new OperationContext();
                    foreach (var entity in items)
                    {
                        var dte = new DynamicTableEntity(NewPartitionKey, entity.RowKey)
                        {
                            Properties = entity.WriteEntity(context)
                        };
                        oldBatch.Add(TableOperation.Delete(entity));
                        newBatch.Add(TableOperation.InsertOrMerge(dte));
                        count++;
                        if (oldBatch.Count != BatchSize) continue;
                        await table.ExecuteBatchAsync(oldBatch);
                        await table.ExecuteBatchAsync(newBatch);
                        oldBatch.Clear();
                        newBatch.Clear();
                    }

                    if (oldBatch.Count <= 0) continue;
                    await table.ExecuteBatchAsync(oldBatch);
                    await table.ExecuteBatchAsync(newBatch);
                    oldBatch.Clear();
                    newBatch.Clear();

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
                } while (token != null);

                sw.Stop();

                console.WriteLine(
                    $"Renamed Partition Key for {count} records from '{OldPartitionKey}' to {NewPartitionKey}' in {sw.Elapsed.TotalSeconds} seconds.");


            }
            catch (Exception e)
            {
                console.WriteLine(e);
                throw;
            }
        }
    }
}
