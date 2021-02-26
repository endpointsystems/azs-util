using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ats_util.Commands;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Azure.Cosmos.Table;
// ReSharper disable ExpressionIsAlwaysNull
// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable LoopVariableIsNeverChangedInsideLoop

namespace azs_util.Commands
{

    [Command(Name="list-pk",FullName = "list-partition-keys",Description = "list all PartitionKeys in a table")]
    public class ListPartitionKeys: BaseCommand
    {
        public async Task OnExecute(IConsole console)
        {
            long rows = 0;
            var hash = new HashSet<string>();
            if (string.IsNullOrEmpty(ConnectionString))
            {
                ConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
                if (string.IsNullOrEmpty(ConnectionString)) throw new ArgumentException(nameof(ConnectionString));
            }

            var sw = Stopwatch.StartNew();

            try
            {
                var csa = CloudStorageAccount.Parse(ConnectionString);
                var client = csa.CreateCloudTableClient();
                var table  = client.GetTableReference(TableName);

                TableContinuationToken token = null;
                do
                {
                    var items = await table.ExecuteQuerySegmentedAsync(
                        new TableQuery<DynamicTableEntity>(), token);
                    token = items.ContinuationToken;
                    foreach (var item in items)
                    {
                        rows++;
                        if (!hash.Contains(item.PartitionKey)) hash.Add(item.PartitionKey);
                    }
                } while (token != null);

                sw.Stop();
                foreach (var h in hash)
                {
                    console.WriteLine(h);
                }

                console.WriteLine(
                    $"Listed {hash.Count} partition keys from {rows} rows in table {TableName} in {sw.Elapsed.TotalSeconds:N} seconds ({sw.ElapsedMilliseconds}ms)");

            }
            catch (Exception e)
            {
                console.WriteLine(e);
                throw;
            }
        }
    }
}
