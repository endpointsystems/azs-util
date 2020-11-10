using McMaster.Extensions.CommandLineUtils;

namespace azs_util.Commands
{
    [Command(Name = "containers", Description = "Container storage commands")]
    [Subcommand(typeof(ListContainers), typeof(DeleteContainers), typeof(ContainerSize))]
    public class Containers
    {
        public void OnExecute()
        {

        }
    }
}
