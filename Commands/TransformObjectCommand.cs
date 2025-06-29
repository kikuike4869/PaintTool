// PaintTool/Commands/TransformObjectCommand.cs

namespace PaintTool.Commands
{
    public record TransformState(PointF Location, SizeF Size, float Angle, float Scale);

    public class TransformObjectCommand : ICommand
    {
        private readonly TransformableObject _target;
        private readonly TransformState _beforteState;
        private TransformState _afterState = null!;

        public TransformObjectCommand(TransformableObject target, TransformState beforeState)
        {
            _target = target;
            _beforteState = beforeState;
        }

        /// <summary>
        /// 操作完了後の状態をキャプチャします
        /// <summary>
        public void CaptureAfterState()
        {
            _afterState = new TransformState(_target.Location, _target.Size, _target.Angle, _target.Scale);
        }

        public void Execute()
        {
            _target.SetState(_afterState);
        }

        public void Unexecute()
        {
            _target.SetState(_beforteState);
        }
    }
}