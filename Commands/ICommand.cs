// Models/Commands/ICommand.cs
namespace PaintTool.Commands
{
    public interface ICommand
    {
        void Execute();
        void Unexecute();
    }
}