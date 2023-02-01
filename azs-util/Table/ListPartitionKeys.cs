using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Azure.Data.Tables;
using McMaster.Extensions.CommandLineUtils;

// ReSharper disable ExpressionIsAlwaysNull
// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable LoopVariableIsNeverChangedInsideLoop

namespace azs_util.Table;

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
            var client = new TableServiceClient(ConnectionString);
            var table  = client.GetTableClient(TableName);

            var token = string.Empty;
            do
            {
                var items = table.QueryAsync<TableEntity>();

                await foreach(var page in items.AsPages(token))
                {
                    foreach (var pageValue in page.Values)
                    {
                        rows++;
                        if (!hash.Contains(pageValue.PartitionKey)) hash.Add(pageValue.PartitionKey);
                    }

                    token = page.ContinuationToken;
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

    public ListPartitionKeys(IConsole iConsole) : base(iConsole) { }
}
