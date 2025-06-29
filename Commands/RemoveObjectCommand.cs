// Models/Commands/RemoveObjectCommand.cs
namespace PaintTool.Commands
{
    public class RemoveObjectCommand : ICommand
    {
        private readonly Canvas _canvas;
        private readonly CanvasObject _objectToRemove;

        public RemoveObjectCommand(Canvas canvas, CanvasObject objectToRemove)
        {
            _canvas = canvas;
            _objectToRemove = objectToRemove;
        }

        public void Execute()
        {
            _canvas.RemoveObjectInternal(_objectToRemove);
        }

        public void Unexecute()
        {
            _canvas.AddObjectInternal(_objectToRemove);
        }
    }
}