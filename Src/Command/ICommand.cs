namespace Neurocommenting.Command;

public interface ICommand
{
    Task ExecuteAsync();
}