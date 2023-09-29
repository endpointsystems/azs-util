using McMaster.Extensions.CommandLineUtils;

namespace azs_util.Table;

[Command(Name = "table", Description = "Table commands")]
[Subcommand(typeof(RenamePartitionKey), typeof(ListPartitionKeys),
    typeof(ListRowKeys), typeof(CopyColumn), typeof(DeleteColumn), typeof(RenameColumn), typeof(CopyTable))]
public class Table
{
    public void OnExecute() { }
}
