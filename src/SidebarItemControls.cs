using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LocalWebTrayShell
{
    internal abstract class SidebarItemCard : UserControl
    {
        private bool hover;
        private bool selected;
        private Color accentColor;
        private Color selectedFillColor;
        private Color selectedBorderColor;
        private Color normalFillColor;
        private Color normalBorderColor;
        private Color currentFillColor;
        private Color currentBorderColor;
        private readonly Panel accentBar;

        protected SidebarItemCard()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            DoubleBuffered = true;
            Cursor = Cursors.Hand;
            BackColor = Color.White;
            Margin = new Padding(0, 0, 0, 10);
            TabStop = false;
            MinimumSize = new Size(0, 0);

            accentBar = new Panel();
            accentBar.Width = 6;
            accentBar.BackColor = Color.FromArgb(116, 126, 141);
            Controls.Add(accentBar);

            NormalFillColor = Color.White;
            NormalBorderColor = Color.FromArgb(231, 236, 241);
            SelectedFillColor = Color.FromArgb(226, 242, 237);
            SelectedBorderColor = Color.FromArgb(167, 210, 198);
            AccentColor = Color.FromArgb(116, 126, 141);

            AttachInteraction(this);
            AttachInteraction(accentBar);
            UpdateVisualState();
            UpdateAccentBarBounds();
        }

        public bool Selected
        {
            get { return selected; }
            set
            {
                if (selected == value)
                {
                    return;
                }

                selected = value;
                UpdateVisualState();
            }
        }

        protected Color AccentColor
        {
            get { return accentColor; }
            set
            {
                accentColor = value;
                accentBar.BackColor = value;
                Invalidate();
            }
        }

        protected Color SelectedFillColor
        {
            get { return selectedFillColor; }
            set { selectedFillColor = value; }
        }

        protected Color SelectedBorderColor
        {
            get { return selectedBorderColor; }
            set { selectedBorderColor = value; }
        }

        protected Color NormalFillColor
        {
            get { return normalFillColor; }
            set { normalFillColor = value; }
        }

        protected Color NormalBorderColor
        {
            get { return normalBorderColor; }
            set { normalBorderColor = value; }
        }

        protected Color CurrentFillColor
        {
            get { return currentFillColor; }
        }

        public event EventHandler CardClicked;

        protected void AttachInteraction(Control control)
        {
            foreach (Control child in control.Controls)
            {
                child.MouseEnter += OnInteractiveMouseEnter;
                child.MouseLeave += OnInteractiveMouseLeave;
                child.MouseDown += OnInteractiveMouseDown;
                AttachInteraction(child);
            }
        }

        protected void RegisterCardControl(Control control)
        {
            Controls.Add(control);
            control.MouseEnter += OnInteractiveMouseEnter;
            control.MouseLeave += OnInteractiveMouseLeave;
            control.MouseDown += OnInteractiveMouseDown;
            AttachInteraction(control);
            ApplyFillColor(control, currentFillColor);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (!hover)
            {
                hover = true;
                UpdateVisualState();
            }

            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (!ClientRectangle.Contains(PointToClient(Cursor.Position)))
            {
                hover = false;
                UpdateVisualState();
            }

            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            EventHandler handler;

            if (e.Button != MouseButtons.Left)
            {
                base.OnMouseDown(e);
                return;
            }

            handler = CardClicked;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }

            base.OnMouseDown(e);
        }

        protected override void OnResize(EventArgs e)
        {
            UpdateAccentBarBounds();
            base.OnResize(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle bounds = new Rectangle(0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1));

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath path = CreateRoundedRectanglePath(bounds, 16))
            using (SolidBrush fillBrush = new SolidBrush(currentFillColor))
            using (Pen borderPen = new Pen(currentBorderColor))
            {
                e.Graphics.FillPath(fillBrush, path);
                e.Graphics.DrawPath(borderPen, path);
            }

            base.OnPaint(e);
        }

        private void OnInteractiveMouseEnter(object sender, EventArgs e)
        {
            if (!hover)
            {
                hover = true;
                UpdateVisualState();
            }
        }

        private void OnInteractiveMouseLeave(object sender, EventArgs e)
        {
            if (ClientRectangle.Contains(PointToClient(Cursor.Position)))
            {
                return;
            }

            if (hover)
            {
                hover = false;
                UpdateVisualState();
            }
        }

        private void OnInteractiveMouseDown(object sender, MouseEventArgs e)
        {
            EventHandler handler;

            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            handler = CardClicked;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void UpdateVisualState()
        {
            currentFillColor = selected
                ? SelectedFillColor
                : hover ? Blend(NormalFillColor, AccentColor, 0.06f) : NormalFillColor;
            currentBorderColor = selected
                ? SelectedBorderColor
                : hover ? Blend(NormalBorderColor, AccentColor, 0.18f) : NormalBorderColor;

            accentBar.BackColor = AccentColor;
            ApplyFillColor(this, currentFillColor);
            accentBar.BackColor = AccentColor;
            Invalidate();
        }

        private void ApplyFillColor(Control control, Color color)
        {
            foreach (Control child in control.Controls)
            {
                if (!object.ReferenceEquals(child, accentBar))
                {
                    child.BackColor = color;
                }

                ApplyFillColor(child, color);
            }

            if (!object.ReferenceEquals(control, accentBar))
            {
                control.BackColor = color;
            }
        }

        private void UpdateAccentBarBounds()
        {
            accentBar.Location = new Point(12, 12);
            accentBar.Height = Math.Max(10, Height - 24);
        }

        protected GraphicsPath CreateRoundedRectanglePath(Rectangle bounds, int radius)
        {
            int diameter = Math.Max(2, radius * 2);
            GraphicsPath path = new GraphicsPath();

            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        protected Color Blend(Color baseColor, Color overlay, float amount)
        {
            amount = Math.Max(0f, Math.Min(1f, amount));
            return Color.FromArgb(
                255,
                (int)(baseColor.R + ((overlay.R - baseColor.R) * amount)),
                (int)(baseColor.G + ((overlay.G - baseColor.G) * amount)),
                (int)(baseColor.B + ((overlay.B - baseColor.B) * amount)));
        }
    }

    internal sealed class CommandSidebarCard : SidebarItemCard
    {
        private readonly Panel contentPanel;
        private readonly TableLayoutPanel headerTable;
        private readonly Label titleLabel;
        private readonly Label badgeLabel;
        private readonly Label metaLabel;
        private CommandRuntimeSnapshot snapshot;
        private CommandEntry command;

        public CommandSidebarCard()
        {
            Height = 64;
            AccentColor = Color.FromArgb(116, 126, 141);

            contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Padding = new Padding(28, 10, 16, 10);
            contentPanel.MinimumSize = new Size(0, 0);

            headerTable = new TableLayoutPanel();
            headerTable.Dock = DockStyle.Top;
            headerTable.Height = 24;
            headerTable.Margin = new Padding(0);
            headerTable.Padding = new Padding(0);
            headerTable.MinimumSize = new Size(0, 0);
            headerTable.ColumnCount = 2;
            headerTable.RowCount = 1;
            headerTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            headerTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76f));

            titleLabel = CreateTextLabel(new Font("Microsoft YaHei UI", 9.5f, FontStyle.Bold), Color.FromArgb(26, 37, 49));
            titleLabel.Dock = DockStyle.Fill;
            titleLabel.MaximumSize = new Size(0, 24);

            badgeLabel = CreateTextLabel(new Font("Microsoft YaHei UI", 8.25f, FontStyle.Bold), Color.FromArgb(116, 126, 141));
            badgeLabel.Dock = DockStyle.Fill;
            badgeLabel.TextAlign = ContentAlignment.MiddleCenter;
            badgeLabel.Margin = new Padding(6, 0, 0, 0);

            metaLabel = CreateTextLabel(new Font("Microsoft YaHei UI", 8.5f, FontStyle.Regular), Color.FromArgb(104, 114, 128));
            metaLabel.Dock = DockStyle.Top;
            metaLabel.Height = 18;
            metaLabel.Margin = new Padding(0, 6, 0, 0);
            metaLabel.MaximumSize = new Size(0, 18);

            headerTable.Controls.Add(titleLabel, 0, 0);
            headerTable.Controls.Add(badgeLabel, 1, 0);

            contentPanel.Controls.Add(metaLabel);
            contentPanel.Controls.Add(headerTable);

            RegisterCardControl(contentPanel);
        }

        public CommandEntry Command
        {
            get { return command; }
        }

        public string CommandId
        {
            get { return command == null ? null : command.Id; }
        }

        public void Bind(CommandEntry value, CommandRuntimeSnapshot runtimeSnapshot)
        {
            string titleText;

            command = value;
            snapshot = runtimeSnapshot;
            titleText = command == null ? string.Empty : command.Name ?? string.Empty;

            if (command != null && command.EnabledOnStart)
            {
                titleText += "  [\u81ea\u542f]";
            }

            if (command != null && command.AutoRetry != null && command.AutoRetry.Enabled)
            {
                titleText += "  [\u91cd\u8bd5]";
            }

            titleLabel.Text = titleText;
            metaLabel.Text = command == null
                ? string.Empty
                : RunModeCatalog.GetDisplayName(command.RunMode) + "  |  " + SummarizeCommand(command.Command);
            badgeLabel.Text = snapshot == null ? "\u5df2\u505c\u6b62" : snapshot.GetDisplayStatus();

            AccentColor = GetStatusAccent(runtimeSnapshot == null ? CommandStatus.Stopped : runtimeSnapshot.Status);
            ApplyBadgeStyle(runtimeSnapshot == null ? CommandStatus.Stopped : runtimeSnapshot.Status);
        }

        private void ApplyBadgeStyle(CommandStatus status)
        {
            badgeLabel.ForeColor = GetStatusAccent(status);
            badgeLabel.BackColor = GetStatusBadgeBackground(status);
        }

        private Label CreateTextLabel(Font font, Color foreColor)
        {
            Label label = new Label();
            label.AutoEllipsis = true;
            label.AutoSize = false;
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.Font = font;
            label.ForeColor = foreColor;
            label.Margin = new Padding(0);
            return label;
        }

        private Color GetStatusAccent(CommandStatus status)
        {
            if (status == CommandStatus.Running)
            {
                return Color.FromArgb(39, 135, 77);
            }

            if (status == CommandStatus.Error)
            {
                return Color.FromArgb(179, 54, 73);
            }

            if (status == CommandStatus.Starting ||
                status == CommandStatus.Stopping ||
                status == CommandStatus.WaitingRetry)
            {
                return Color.FromArgb(189, 123, 18);
            }

            return Color.FromArgb(116, 126, 141);
        }

        private Color GetStatusBadgeBackground(CommandStatus status)
        {
            if (status == CommandStatus.Running)
            {
                return Color.FromArgb(228, 245, 233);
            }

            if (status == CommandStatus.Error)
            {
                return Color.FromArgb(252, 234, 236);
            }

            if (status == CommandStatus.Starting ||
                status == CommandStatus.Stopping ||
                status == CommandStatus.WaitingRetry)
            {
                return Color.FromArgb(255, 244, 217);
            }

            return Color.FromArgb(238, 242, 246);
        }

        private string SummarizeCommand(string value)
        {
            value = (value ?? string.Empty).Replace("\r", " ").Replace("\n", " ").Trim();

            if (value.Length <= 240)
            {
                return value;
            }

            return value.Substring(0, 237) + "...";
        }
    }

    internal sealed class SiteSidebarCard : SidebarItemCard
    {
        private readonly Panel contentPanel;
        private readonly Label titleLabel;
        private readonly Label urlLabel;
        private SiteEntry site;

        public SiteSidebarCard()
        {
            Height = 60;
            AccentColor = Color.FromArgb(214, 143, 51);
            SelectedFillColor = Color.FromArgb(245, 239, 226);
            SelectedBorderColor = Color.FromArgb(228, 202, 161);

            contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Padding = new Padding(28, 10, 16, 9);
            contentPanel.MinimumSize = new Size(0, 0);

            titleLabel = CreateTextLabel(new Font("Microsoft YaHei UI", 9.5f, FontStyle.Bold), Color.FromArgb(26, 37, 49));
            titleLabel.Dock = DockStyle.Top;
            titleLabel.Height = 20;
            titleLabel.MaximumSize = new Size(0, 20);

            urlLabel = CreateTextLabel(new Font("Microsoft YaHei UI", 8.5f, FontStyle.Regular), Color.FromArgb(104, 114, 128));
            urlLabel.Dock = DockStyle.Top;
            urlLabel.Height = 18;
            urlLabel.Margin = new Padding(0, 5, 0, 0);
            urlLabel.MaximumSize = new Size(0, 18);

            contentPanel.Controls.Add(urlLabel);
            contentPanel.Controls.Add(titleLabel);

            RegisterCardControl(contentPanel);
        }

        public SiteEntry SiteEntry
        {
            get { return site; }
        }

        public string SiteId
        {
            get { return site == null ? null : site.Id; }
        }

        public void Bind(SiteEntry value)
        {
            site = value;
            titleLabel.Text = site == null ? string.Empty : site.Name ?? string.Empty;
            urlLabel.Text = site == null ? string.Empty : site.Url ?? string.Empty;
        }

        private Label CreateTextLabel(Font font, Color foreColor)
        {
            Label label = new Label();
            label.AutoEllipsis = true;
            label.AutoSize = false;
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.Font = font;
            label.ForeColor = foreColor;
            label.Margin = new Padding(0);
            return label;
        }
    }
}
