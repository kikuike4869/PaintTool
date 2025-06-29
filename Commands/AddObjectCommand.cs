// Models/Commands/AddObjectCommand.cs
namespace PaintTool.Commands
{
    public class AddObjectCommand : ICommand
    {
        private readonly Canvas _canvas;
        private readonly CanvasObject _objectToAdd;

        public AddObjectCommand(Canvas canvas, CanvasObject objectToAdd)
        {
            _canvas = canvas;
            _objectToAdd = objectToAdd;
        }

        public void Execute()
        {
            _canvas.AddObjectInternal(_objectToAdd);
        }

        public void Unexecute()
        {
            _canvas.RemoveObjectInternal(_objectToAdd);
        }
    }
}