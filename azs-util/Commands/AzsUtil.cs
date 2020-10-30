using ats_util.Commands;
using McMaster.Extensions.CommandLineUtils;

namespace azs_util.Commands
{
    [Command(Name = "azs-util", Description = "azure storage command utility")]
    [Subcommand(typeof(Table), typeof(Containers))]
    public class AzsUtil
    {
        private const string help = "-?|-h|--help";
    }
}
