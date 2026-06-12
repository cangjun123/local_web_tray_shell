using System.Windows.Forms;

namespace LocalWebTrayShell
{
    internal sealed class BufferedScrollPanel : Panel
    {
        public BufferedScrollPanel()
        {
            AutoScroll = true;
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
            DoubleBuffered = true;
            UpdateStyles();
        }
    }
}
