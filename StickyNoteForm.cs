using System;
using System.Drawing;
using System.Windows.Forms;

namespace Post_it
{
    public class StickyNoteForm : BaseNoteForm
    {
        private TextBox? txtText;

        public StickyNoteForm(StickyNoteData? data = null) : base(data)
        {
            InitializeSpecificComponents();
            ApplyData();
        }

        protected override StickyNoteData CreateDefaultNoteData()
        {
            return new StickyNoteData { Type = NoteType.Normal };
        }

        private void InitializeSpecificComponents()
        {
            // Campo de texto específico para notas normales
            txtText = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                Font = new Font(FontFamily.GenericSansSerif, 10),
                ScrollBars = ScrollBars.None,
                BorderStyle = BorderStyle.None,
                PlaceholderText = "Escribe tu nota aquí..."
            };

            txtText.TextChanged += TxtText_TextChanged;
            txtText.ContextMenuStrip = contextMenu;

            // Configurar arrastre para el campo de texto
            txtText.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left && !txtText.Focused) base.OnMouseDown(e); };
            txtText.MouseMove += (s, e) => base.OnMouseMove(e);
            txtText.MouseUp += (s, e) => { if (e.Button == MouseButtons.Left) base.OnMouseUp(e); };

            // Layout
            var tablePanel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
            tablePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            tablePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            
            // Verificar que txtTitle no sea null antes de agregarlo
            if (txtTitle != null)
                tablePanel.Controls.Add(txtTitle, 0, 0);
            if (txtText != null)
                tablePanel.Controls.Add(txtText, 0, 1);
            
            tablePanel.BackColor = Color.Transparent;
            tablePanel.ContextMenuStrip = contextMenu;

            // Configurar arrastre para el panel
            tablePanel.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) base.OnMouseDown(e); };
            tablePanel.MouseMove += (s, e) => base.OnMouseMove(e);
            tablePanel.MouseUp += (s, e) => { if (e.Button == MouseButtons.Left) base.OnMouseUp(e); };

            Controls.Add(tablePanel);
        }

        protected override void SetColor(Color color)
        {
            base.SetColor(color);
            if (txtText != null)
                txtText.BackColor = color;
        }

        protected override void ApplyData()
        {
            base.ApplyData();
            if (txtText != null)
                txtText.Text = NoteData.Text ?? "";
            AutoResize();
        }

        private void TxtText_TextChanged(object? sender, EventArgs e)
        {
            AutoResize();
            StartAutoSaveTimer();
        }

        public override void SaveNote()
        {
            if (txtTitle != null)
                NoteData.Title = txtTitle.Text;
            if (txtText != null)
                NoteData.Text = txtText.Text;
            
            NoteData.NoteColor = this.BackColor;
            NoteData.Opacity = (float)this.Opacity;
            NoteData.Location = this.Location;
            NoteData.Size = this.Size;

            OnNoteSaved(); // Cambiar NoteSaved?.Invoke por OnNoteSaved()
        }

        private void AutoResize()
        {
            if (txtText == null || txtTitle == null) return;

            int minHeight = 150;
            int minWidth = 220;

            var textSize = TextRenderer.MeasureText(txtText.Text, txtText.Font);
            var titleSize = TextRenderer.MeasureText(txtTitle.Text, txtTitle.Font);

            int textHeight = Math.Max(textSize.Height, 80) + 80;
            int textWidth = Math.Max(Math.Max(textSize.Width, titleSize.Width) + 40, minWidth);

            this.Size = new Size(textWidth, Math.Max(textHeight, minHeight));
        }
    }    
}
