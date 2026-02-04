using Neurocommenting.Infrastructure;

namespace Neurocommenting.Command.Commands;

public class CreateGroupCommand : ICommand
{
    public Task ExecuteAsync()
    {
        AccountSetup.CreateGroup();
        return Task.CompletedTask;
    }
}