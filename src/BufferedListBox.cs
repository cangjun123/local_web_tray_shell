using System.Windows.Forms;

namespace LocalWebTrayShell
{
    internal sealed class BufferedListBox : ListBox
    {
        public BufferedListBox()
        {
            BorderStyle = BorderStyle.None;
            DrawMode = DrawMode.OwnerDrawFixed;
            IntegralHeight = false;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            DoubleBuffered = true;
            UpdateStyles();
        }

        protected override void OnNotifyMessage(Message m)
        {
            if (m.Msg != 0x0014)
            {
                base.OnNotifyMessage(m);
            }
        }
    }
}
