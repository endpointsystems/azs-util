namespace azs_util.Table;

[Command(Name = "cpt", FullName = "copyTable", Description = "copy a table from one storage account to another")]
public class CopyTable : BaseCommand
{

    private List<TableTransactionAction> batch = new();
    private int masterCount = 0;

    [Option(ShortName = "ds", LongName = "dest-connection-string", Description = "Storage account destination connection string. Environment variable: AZURE_STORAGE_CONNECTION_DEST.")]
    public string DestConnectionString { get; set; } =
        Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_DEST");

    [Option(ShortName = "t",LongName = "table", Description = "Table name")]
    public string TableName { get; set; }
    [Option(ShortName = "dt",LongName = "destTable", Description = "Destination table name")]
    public string DestTable { get; set; }
    public async Task OnExecute()
    {
        if (string.IsNullOrEmpty(ConnectionString))
            ConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION");
        if (string.IsNullOrEmpty(DestConnectionString))
            DestConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_DEST");

        if (string.IsNullOrEmpty(DestConnectionString) || string.IsNullOrEmpty(DestConnectionString))
        {
            write("either set the AZURE_STORAGE_CONNECTION and AZURE_STORAGE_CONNECTION_DEST environment variables");
            write("or pass them in using the -cs and -ds options.");
            write("usage: azs-util table cpt -t <source table name> -dt <dest table name>");
            return;
        }

        if (string.IsNullOrEmpty(TableName) || string.IsNullOrEmpty(DestTable))
        {
            write("missing source or destination table names.");
            write("usage: azs-util table cpt -t <source table name> -dt <dest table name>");
            return;
        }

        try
        {
            var src = new TableServiceClient(ConnectionString);
            var dest = new TableServiceClient(DestConnectionString);

            var srcTable = src.GetTableClient(TableName);
            var destTable = dest.GetTableClient(DestTable);

            write($"creating table {TableName} in {dest.AccountName} if it doesn't already exist");
            await destTable.CreateIfNotExistsAsync();

            var items = srcTable.QueryAsync<TableEntity>();

            var sw = Stopwatch.StartNew();
            var token = string.Empty;

            do
            {
                await foreach (var page in items.AsPages(token))
                {
                    foreach (var value in page.Values)
                    {
                        masterCount++;
                        await batchLogic(value, destTable);
                    }
                }
            } while (!string.IsNullOrEmpty(token));

            // we're done, so submit whatever we might have lev
            if (batch.Count > 0)
            {
                await destTable.SubmitTransactionAsync(batch);
                batch.Clear();
            }

            sw.Stop();
            write(
                $"copied {masterCount} values from {src.AccountName} to {dest.AccountName} in {sw.Elapsed.TotalSeconds} seconds");
        }
        catch (Exception ex)
        {
            write($"{ex.GetType()}: {ex.Message}");
        }
    }

    private string lastPart = string.Empty; // last partition value
    private async Task batchLogic(TableEntity entity, TableClient destTable)
    {
        if (string.IsNullOrEmpty(lastPart))
        {
            lastPart = entity.PartitionKey;
            batch.Add(new TableTransactionAction(TableTransactionActionType.UpsertReplace, entity));
        }
        else
        {
            if (lastPart == entity.PartitionKey)
            {
                batch.Add(new TableTransactionAction(TableTransactionActionType.UpsertReplace, entity));
            }
            else
            {
                lastPart = entity.PartitionKey;
                await destTable.SubmitTransactionAsync(batch);
                batch.Clear();
                batch.Add(new TableTransactionAction(TableTransactionActionType.UpsertReplace, entity));
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
