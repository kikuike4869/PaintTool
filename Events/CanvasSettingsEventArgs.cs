using System;
using System.Drawing;

namespace PaintTool
{
    public class CanvasSettingsEventArgs : EventArgs
    {
        public double AspectRatio { get; }
        public Color BackgroundColor { get; }

        public CanvasSettingsEventArgs(double aspectRatio, Color backgroundColor)
        {
            AspectRatio = aspectRatio;
            BackgroundColor = backgroundColor;
        }
    }
}