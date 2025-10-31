namespace azs_util.Table;

[Command(Name = "table", Description = "Table commands")]
[Subcommand(typeof(RenamePartitionKey), typeof(ListPartitionKeys),
    typeof(ListRowKeys), typeof(CopyColumn), typeof(DeleteColumn), typeof(RenameColumn),
    typeof(CopyTable), typeof(DeleteTable), typeof(EmptyTable))]
public class Table
{
    public void OnExecute() { }
}
