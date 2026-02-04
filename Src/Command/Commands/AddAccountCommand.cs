using Neurocommenting.Infrastructure;

namespace Neurocommenting.Command.Commands;

public class AddAccountCommand : ICommand
{
    public Task ExecuteAsync()
    {
        AccountSetup.AddAccount();
        return Task.CompletedTask;
    }
}