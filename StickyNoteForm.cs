using System;
using System.Drawing;
using System.Windows.Forms;

namespace Post_it
{
    public class StickyNoteForm : Form
    {
        public StickyNoteData NoteData { get; private set; }
        private TextBox? txtTitle;
        private TextBox? txtText;
        private TrackBar? trackOpacity;
        private Label? lblOpacity;
        private ColorDialog? colorDialog;
        private ContextMenuStrip? contextMenu;
        private ToolStripMenuItem? menuColor;
        private ToolStripMenuItem? menuSave;
        private ToolStripMenuItem? menuDelete;
        private ToolStripMenuItem? menuYellow;
        private ToolStripMenuItem? menuBlue;
        private ToolStripMenuItem? menuGreen;
        private ToolStripMenuItem? menuPink;
        private System.Windows.Forms.Timer? autoSaveTimer;
        private ToolStripMenuItem? menuOpacity;

        // Variables para el arrastre
        private bool isDragging = false;
        private Point lastCursor;
        private Point lastForm;

        public event EventHandler? NoteSaved;
        public event EventHandler? NoteDeleted;

        public StickyNoteForm(StickyNoteData? data = null)
        {
            NoteData = data ?? new StickyNoteData();
            try { this.Icon = new Icon("postit.ico"); } catch { }
            InitializeComponents();
            ApplyData();
        }

        private void InitializeComponents()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = false; // Cambiado a false para que no esté siempre encima
            this.StartPosition = FormStartPosition.Manual;
            this.Size = new Size(250, 200);
            this.BackColor = Color.Yellow;
            this.ShowInTaskbar = false;

            // Auto-save timer
            autoSaveTimer = new System.Windows.Forms.Timer();
            autoSaveTimer.Interval = 2000; // Auto-guardar cada 2 segundos
            autoSaveTimer.Tick += AutoSaveTimer_Tick;

            txtTitle = new TextBox { 
                PlaceholderText = "Haz clic para agregar título...", 
                Dock = DockStyle.Fill, 
                Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold),
                BorderStyle = BorderStyle.None
            };
            
            txtText = new TextBox { 
                Multiline = true, 
                Dock = DockStyle.Fill, 
                Font = new Font(FontFamily.GenericSansSerif, 10), 
                ScrollBars = ScrollBars.None,
                BorderStyle = BorderStyle.None,
                PlaceholderText = "Escribe tu nota aquí..."
            };

            lblOpacity = new Label { 
                Text = "Opacidad: 100%", 
                Dock = DockStyle.Left, 
                Width = 80, 
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            
            trackOpacity = new TrackBar { 
                Minimum = 30, 
                Maximum = 100, 
                Value = 100, 
                Dock = DockStyle.Fill,
                Height = 25
            };

            colorDialog = new ColorDialog();

            txtText.TextChanged += TxtText_TextChanged;
            txtTitle.TextChanged += TxtText_TextChanged;
            trackOpacity.Scroll += TrackOpacity_Scroll;

            // Permitir arrastrar la ventana
            this.MouseDown += StickyNoteForm_MouseDown;
            this.MouseMove += StickyNoteForm_MouseMove;
            this.MouseUp += StickyNoteForm_MouseUp;
            txtTitle.MouseDown += StickyNoteForm_MouseDown;
            txtTitle.MouseMove += StickyNoteForm_MouseMove;
            txtTitle.MouseUp += StickyNoteForm_MouseUp;

            // Context menu mejorado
            contextMenu = new ContextMenuStrip();
            menuColor = new ToolStripMenuItem("Más colores...", null, MenuColor_Click);
            menuYellow = new ToolStripMenuItem("Amarillo", null, (s, e) => SetColor(Color.Yellow));
            menuBlue = new ToolStripMenuItem("Azul", null, (s, e) => SetColor(Color.LightBlue));
            menuGreen = new ToolStripMenuItem("Verde", null, (s, e) => SetColor(Color.LightGreen));
            menuPink = new ToolStripMenuItem("Rosa", null, (s, e) => SetColor(Color.Pink));
            menuOpacity = new ToolStripMenuItem("Cambiar opacidad...", null, MenuOpacity_Click);
            menuSave = new ToolStripMenuItem("Guardar ahora (Ctrl+S)", null, MenuSave_Click);
            menuDelete = new ToolStripMenuItem("Eliminar (Del)", null, MenuDelete_Click);
            
            contextMenu.Items.AddRange(new ToolStripItem[] { 
                menuYellow, menuBlue, menuGreen, menuPink, 
                new ToolStripSeparator(),
                menuColor, 
                new ToolStripSeparator(),
                menuOpacity,
                new ToolStripSeparator(),
                menuSave, menuDelete 
            });
            
            // Asignar el menú contextual al formulario y a todos los controles
            this.ContextMenuStrip = contextMenu;
            txtTitle.ContextMenuStrip = contextMenu;
            txtText.ContextMenuStrip = contextMenu;

            var tablePanel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
            tablePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            tablePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tablePanel.Controls.Add(txtTitle, 0, 0);
            tablePanel.Controls.Add(txtText, 0, 1);
            tablePanel.BackColor = Color.Transparent;
            tablePanel.ContextMenuStrip = contextMenu;

            // Comentar o eliminar el panel de opacidad para ocultarlo
            // var opacityPanel = new Panel { Dock = DockStyle.Bottom, Height = 25, BackColor = Color.Transparent };
            // opacityPanel.Controls.Add(trackOpacity);
            // opacityPanel.Controls.Add(lblOpacity);
            // opacityPanel.ContextMenuStrip = contextMenu;

            Controls.Add(tablePanel);
            // Controls.Add(opacityPanel); // Comentado para ocultar la barra

            // Atajos de teclado
            this.KeyPreview = true;
            this.KeyDown += StickyNoteForm_KeyDown;
        }

        private void ApplyData()
        {
            txtTitle!.Text = NoteData.Title ?? "";
            txtText!.Text = NoteData.Text ?? "";
            
            // Aplicar color guardado o amarillo por defecto
            var noteColor = NoteData.NoteColor;
            if (noteColor == Color.Empty || noteColor == default(Color))
                noteColor = Color.Yellow;
                
            SetColor(noteColor);
            
            this.Opacity = NoteData.Opacity > 0 ? NoteData.Opacity : 1.0f;
            if (NoteData.Location != Point.Empty) this.Location = NoteData.Location;
            if (NoteData.Size != Size.Empty) this.Size = NoteData.Size;
            trackOpacity!.Value = (int)(this.Opacity * 100);
            lblOpacity!.Text = $"Opacidad: {trackOpacity.Value}%";
        }

        private void TrackOpacity_Scroll(object? sender, EventArgs e)
        {
            this.Opacity = trackOpacity!.Value / 100f;
            lblOpacity!.Text = $"Opacidad: {trackOpacity.Value}%";
        }

        private void TxtText_TextChanged(object? sender, EventArgs e)
        {
            AutoResize();
            StartAutoSaveTimer();
        }

        private void StartAutoSaveTimer()
        {
            autoSaveTimer?.Stop();
            autoSaveTimer?.Start();
        }

        private void AutoSaveTimer_Tick(object? sender, EventArgs e)
        {
            autoSaveTimer?.Stop();
            SaveNote();
        }

        private void StickyNoteForm_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                lastCursor = Cursor.Position;
                lastForm = this.Location;
            }
        }

        private void StickyNoteForm_MouseMove(object? sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point currentCursor = Cursor.Position;
                Point offset = new Point(currentCursor.X - lastCursor.X, currentCursor.Y - lastCursor.Y);
                this.Location = new Point(lastForm.X + offset.X, lastForm.Y + offset.Y);
            }
        }

        private void StickyNoteForm_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
                // Guardar la nueva posición automáticamente
                SaveNote();
            }
        }

        private void StickyNoteForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                SaveNote();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Delete && e.Control)
            {
                MenuDelete_Click(sender, e);
                e.Handled = true;
            }
        }

        private void SetColor(Color color)
        {
            this.BackColor = color;
            // Sincronizar el color de fondo de los campos de texto
            txtTitle!.BackColor = color;
            txtText!.BackColor = color;
        }

        private void MenuColor_Click(object? sender, EventArgs e)
        {
            if (colorDialog!.ShowDialog() == DialogResult.OK)
            {
                SetColor(colorDialog.Color);
            }
        }

        private void MenuOpacity_Click(object? sender, EventArgs e)
        {
            // Crear un formulario simple para cambiar la opacidad
            using (var opacityForm = new Form())
            {
                opacityForm.Text = "Cambiar Opacidad";
                opacityForm.Size = new Size(400, 250);
                opacityForm.StartPosition = FormStartPosition.CenterScreen;
                opacityForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                opacityForm.MaximizeBox = false;
                opacityForm.MinimizeBox = false;
                opacityForm.TopMost = true; // Asegurar que esté por encima de todo
                opacityForm.ShowInTaskbar = false;

                var label = new Label { Text = "Opacidad (30-100%):", Location = new Point(20, 20), Size = new Size(150, 20) };
                var trackBar = new TrackBar { 
                    Location = new Point(20, 50), 
                    Size = new Size(200, 45),
                    Minimum = 30, 
                    Maximum = 100, 
                    Value = (int)(this.Opacity * 100) 
                };
                var lblValue = new Label { 
                    Location = new Point(230, 60), 
                    Size = new Size(50, 20),
                    Text = trackBar.Value + "%"
                };
                var btnOK = new Button { 
                    Text = "OK", 
                    Location = new Point(120, 140), 
                    Size = new Size(60, 30),
                    DialogResult = DialogResult.OK
                };
                var btnCancel = new Button { 
                    Text = "Cancelar", 
                    Location = new Point(190, 140), 
                    Size = new Size(70, 30),
                    DialogResult = DialogResult.Cancel
                };

                trackBar.Scroll += (s, e) => {
                    lblValue.Text = trackBar.Value + "%";
                    // Mostrar el cambio de opacidad en tiempo real en el Post-it
                    this.Opacity = trackBar.Value / 100f;
                };
                
                opacityForm.Controls.AddRange(new Control[] { label, trackBar, lblValue, btnOK, btnCancel });
                opacityForm.AcceptButton = btnOK;
                opacityForm.CancelButton = btnCancel;

                // Guardar opacidad original para poder restaurarla si se cancela
                float originalOpacity = (float)this.Opacity;

                var result = opacityForm.ShowDialog();

                if (result == DialogResult.OK)
                {
                    this.Opacity = trackBar.Value / 100f;
                    trackOpacity!.Value = trackBar.Value;
                    // Guardar automáticamente el cambio de opacidad
                    SaveNote();
                }
                else
                {
                    // Restaurar opacidad original si se canceló
                    this.Opacity = originalOpacity;
                }
            }
        }

        private void MenuSave_Click(object? sender, EventArgs e)
        {
            SaveNote();
        }

        private void SaveNote()
        {
            NoteData.Title = txtTitle!.Text;
            NoteData.Text = txtText!.Text;
            NoteData.NoteColor = this.BackColor;
            NoteData.Opacity = (float)this.Opacity;
            NoteData.Location = this.Location;
            NoteData.Size = this.Size;
            
            // Debug: Mostrar el color que se está guardando
            Console.WriteLine($"Guardando nota con color: {this.BackColor.Name} (ARGB: {this.BackColor.ToArgb()})");
            
            NoteSaved?.Invoke(this, EventArgs.Empty);
        }

        private void MenuDelete_Click(object? sender, EventArgs e)
        {
            var result = MessageBox.Show("¿Estás seguro de que quieres eliminar esta nota?", 
                                       "Confirmar eliminación", 
                                       MessageBoxButtons.YesNo, 
                                       MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                NoteDeleted?.Invoke(this, EventArgs.Empty);
                this.Close();
            }
        }

        private void AutoResize()
        {
            int minHeight = 150;
            int minWidth = 220;
            
            // Calcular tamaño basado en el contenido del texto
            var textSize = TextRenderer.MeasureText(txtText!.Text, txtText.Font);
            var titleSize = TextRenderer.MeasureText(txtTitle!.Text, txtTitle.Font);
            
            int textHeight = Math.Max(textSize.Height, 80) + 80; // Espacio extra para título y controles
            int textWidth = Math.Max(Math.Max(textSize.Width, titleSize.Width) + 40, minWidth);
            
            this.Size = new Size(textWidth, Math.Max(textHeight, minHeight));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Crear bordes redondeados
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            int radius = 10;
            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(this.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(this.Width - radius, this.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, this.Height - radius, radius, radius, 90, 90);
            path.CloseAllFigures();

            this.Region = new Region(path);
            base.OnPaint(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                autoSaveTimer?.Stop();
                autoSaveTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
