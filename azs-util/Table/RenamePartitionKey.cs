using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Azure.Data.Tables;
using McMaster.Extensions.CommandLineUtils;

// ReSharper disable ExpressionIsAlwaysNull

namespace azs_util.Table
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
            CheckConnection();
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                var client = new TableServiceClient(ConnectionString);
                var table  = client.GetTableClient(TableName);
                var items = table.QueryAsync<TableEntity> ($"PartitionKey eq '{OldPartitionKey}'");
                int count = 0;

                string token = null;
                do
                {
                    var oldBatch = new List<TableTransactionAction>();
                    var newBatch = new List<TableTransactionAction>();

                    await foreach (var page in items.AsPages(token))
                    {
                        foreach (var pageValue in page.Values)
                        {
                            oldBatch.Add(new TableTransactionAction(TableTransactionActionType.Delete,pageValue));
                            var newEntity = new TableEntity(NewPartitionKey, pageValue.RowKey);

                            foreach (var key in pageValue.Keys)
                            {
                                if (key == "PartitionKey" || key == "RowKey") continue;
                                newEntity.Add(key,pageValue[key]);
                            }
                            newBatch.Add(new TableTransactionAction(TableTransactionActionType.Add,newEntity));
                            count++;
                            if (oldBatch.Count != BatchSize) continue;
                            await table.SubmitTransactionAsync(oldBatch);
                            await table.SubmitTransactionAsync(newBatch);
                            oldBatch.Clear();
                            newBatch.Clear();
                        }
                        //catch the leftover batch (anything less than the batch size)
                        if (oldBatch.Count > 0) await table.SubmitTransactionAsync(oldBatch);
                        if (newBatch.Count > 0) await table.SubmitTransactionAsync(newBatch);
                        oldBatch.Clear();
                        newBatch.Clear();
                        token = page.ContinuationToken;
                    }
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

        public RenamePartitionKey(IConsole iConsole) : base(iConsole) { }
    }
}
