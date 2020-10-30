using ats_util.Commands;
using McMaster.Extensions.CommandLineUtils;

namespace azs_util.Commands
{
    [Command(Name = "table", Description = "Table commands")]
    [Subcommand(typeof(RenamePartitionKey))]
    public class Table
    {

    }
}
