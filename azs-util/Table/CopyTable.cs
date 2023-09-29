using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Azure.Data.Tables;
using McMaster.Extensions.CommandLineUtils;

namespace azs_util.Table;

[Command(Name = "cpt", FullName = "copyTable", Description = "copy a table from one storage account to another")]
public class CopyTable : BaseCommand
{

    private List<TableTransactionAction> batch = new();
    private int masterCount = 0;
    public async Task OnExecute()
    {
        if (string.IsNullOrEmpty(DestConnectionString))
            DestConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_DEST");
        if (string.IsNullOrEmpty(DestConnectionString)) ArgumentNullException.ThrowIfNull(DestConnectionString);
        if (string.IsNullOrEmpty(TableName)) ArgumentNullException.ThrowIfNull(TableName);

        var src = new TableServiceClient(ConnectionString);
        var dest = new TableServiceClient(DestConnectionString);

        var srcTable  = src.GetTableClient(TableName);
        var destTable = dest.GetTableClient(TableName);
        write($"creating table {TableName} in {dest.AccountName} if it doesn't already exist");
        await destTable.CreateIfNotExistsAsync();

        var items = srcTable.QueryAsync<TableEntity>();

        var count = 0;

        var sw = Stopwatch.StartNew();
        var token = string.Empty;

        do
        {
            await foreach (var page in items.AsPages(token))
            {
                foreach (var value in page.Values)
                {
                    await batchLogic(value, destTable);
                }
            }
        } while (!string.IsNullOrEmpty(token));

        // we're done, so submit whatever we might have lev
        if (batch.Count > 0)
        {
            write($"loop is clean so processing remaining {batch.Count} records");
            await destTable.SubmitTransactionAsync(batch);
            batch.Clear();
        }
        sw.Stop();
        write($"copied {masterCount} values from {src.AccountName} to {dest.AccountName} in {sw.Elapsed.TotalSeconds} seconds");


    }

    private string lastPart = string.Empty; // last partition value
    private async Task batchLogic(TableEntity entity, TableClient destTable)
    {
        if (string.IsNullOrEmpty(lastPart))
        {
            lastPart = entity.PartitionKey;
            batch.Add(new TableTransactionAction(TableTransactionActionType.UpsertReplace, entity));
            masterCount++;
        }
        else
        {
            if (lastPart == entity.PartitionKey)
            {
                batch.Add(new TableTransactionAction(TableTransactionActionType.UpsertReplace, entity));
                masterCount++;
            }
            else
            {
                lastPart = entity.PartitionKey;
                await destTable.SubmitTransactionAsync(batch);
                batch.Clear();
                batch.Add(new TableTransactionAction(TableTransactionActionType.UpsertReplace, entity));
                masterCount++;
            }
        }

        if (batch.Count == 100)
        {
            await destTable.SubmitTransactionAsync(batch);
            batch.Clear();
        }
    }


    public CopyTable(IConsole iConsole) : base(iConsole)
    {
    }
}
