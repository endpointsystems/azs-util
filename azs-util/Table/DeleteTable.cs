namespace azs_util.Table;

[Command(Name ="dt",Description = "Delete a table from the storage account",FullName = "delete")]
public class DeleteTable : BaseCommand
{
    [Argument(0, Description = "The name of the table to delete.")]
    public string TableName { get; set; }
    public async Task OnExecuteAsync()
    {
        if (string.IsNullOrEmpty(ConnectionString))
        {
            write("connection string required!");
            return;
        }

        if (string.IsNullOrEmpty(TableName))
        {
            write("table name required!");
            return;
        }
        var sw = Stopwatch.StartNew();
        var client = new TableServiceClient(ConnectionString);
        await client.DeleteTableAsync(TableName);
        sw.Stop();
        write($"Table '{TableName}' deleted in {sw.Elapsed.TotalSeconds} seconds");
        write("Be sure to wait 90-120 seconds before rebuilding");
    }
    /// <inheritdoc />
    public DeleteTable(IConsole iConsole) : base(iConsole)
    {
    }
}
