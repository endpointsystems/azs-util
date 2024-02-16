namespace azs_util.Containers;

[Command(Name = "containers", Description = "Container storage commands")]
[Subcommand(typeof(ListContainers), typeof(DeleteContainers), typeof(ContainerSize))]
public class Containers
{
    public void OnExecute()
    {

    }
}
