using Neurocommenting.Infrastructure;

namespace Neurocommenting.Command;

public class CommandInvoker
{
    private Dictionary<char, ICommand> commands;

    public CommandInvoker()
    {
        commands = new Dictionary<char, ICommand>();
    }

    public void Set(char mode, ICommand command)
    {
        commands[mode] = command;
    }

    public async Task Run(char mode)
    {
        if (commands.TryGetValue(mode, out var command))
        {
            await command.ExecuteAsync();
        }
        else
        {
            Printer.PrintError("Ошибка: Введена неверная команда");
        }
    }
}