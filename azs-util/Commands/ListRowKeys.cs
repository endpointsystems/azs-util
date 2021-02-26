using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ats_util.Commands;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Azure.Cosmos.Table;

namespace azs_util.Commands
{
    [Command(Name = "list-rk", FullName = "list-row-keys", Description = "list RowKey items for a given PartitionKey")]
    public class ListRowKeys : BaseCommand
    {
        [Option(LongName = "partition-key", ShortName = "p", Description = "The PartitionKey to query.")]
        public string PartitionKey { get; set; }

        public async Task OnExecute(IConsole console)
        {
            CheckConnection();
            var list = new List<string>();
            var sw = Stopwatch.StartNew();
            try
            {
                var csa = CloudStorageAccount.Parse(ConnectionString);
                var client = csa.CreateCloudTableClient();
                var table = client.GetTableReference(TableName);
                var q = new TableQuery<TableEntity> {FilterString = $"PartitionKey eq '{PartitionKey}'"};
                int count = 0;

                TableContinuationToken token = null;
                do
                {
                    var items = await table.ExecuteQuerySegmentedAsync(q, token);
                    token = items.ContinuationToken;
                    foreach (var item in items)
                    {
                        count++;
                        list.Add(item.RowKey);
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
    }
}
