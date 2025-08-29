using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Post_it
{
    public abstract class BaseNoteForm : Form
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public StickyNoteData NoteData { get; protected set; }
        
        protected TextBox? txtTitle;
        protected TrackBar? trackOpacity;
        protected Label? lblOpacity;
        protected ColorDialog? colorDialog;
        protected ContextMenuStrip? contextMenu;
        protected ToolStripMenuItem? menuColor;
        protected ToolStripMenuItem? menuSave;
        protected ToolStripMenuItem? menuDelete;
        protected ToolStripMenuItem? menuYellow;
        protected ToolStripMenuItem? menuBlue;
        protected ToolStripMenuItem? menuGreen;
        protected ToolStripMenuItem? menuPink;
        protected ToolStripMenuItem? menuOpacity;
        protected System.Windows.Forms.Timer? autoSaveTimer;

        // Variables para arrastre
        private bool isDragging = false;
        private Point dragStartPoint;

        public event EventHandler? NoteSaved;
        public event EventHandler? NoteDeleted;

        protected BaseNoteForm(StickyNoteData? data = null)
        {
            NoteData = data ?? CreateDefaultNoteData();
            InitializeBaseComponents();
            LoadIcon();
        }

        protected abstract StickyNoteData CreateDefaultNoteData();

        private void LoadIcon()
        {
            try 
            { 
                this.Icon = new Icon("postit.ico"); 
            } 
            catch 
            { 
                this.Icon = SystemIcons.Application;
            }
        }

        protected virtual void InitializeBaseComponents()
        {
            // Configuración base del formulario
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Size = new Size(250, 200);
            this.BackColor = Color.Yellow;
            this.ShowInTaskbar = false;
            this.KeyPreview = true;

            // Auto-save timer
            autoSaveTimer = new System.Windows.Forms.Timer();
            autoSaveTimer.Interval = 2000;
            autoSaveTimer.Tick += AutoSaveTimer_Tick;

            // Título
            txtTitle = new TextBox
            {
                PlaceholderText = "Haz clic para agregar título...",
                Dock = DockStyle.Fill,
                Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold),
                BorderStyle = BorderStyle.None
            };
            txtTitle.TextChanged += TxtTitle_TextChanged;

            // Controles de opacidad
            lblOpacity = new Label
            {
                Text = "Opacidad: 100%",
                Dock = DockStyle.Left,
                Width = 80,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            trackOpacity = new TrackBar
            {
                Minimum = 30,
                Maximum = 100,
                Value = 100,
                Dock = DockStyle.Fill,
                Height = 25
            };
            trackOpacity.Scroll += TrackOpacity_Scroll;

            colorDialog = new ColorDialog();

            InitializeContextMenu();
            SetupDragEvents();

            // Eventos de teclado
            this.KeyDown += BaseNoteForm_KeyDown;
        }

        private void InitializeContextMenu()
        {
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

            this.ContextMenuStrip = contextMenu;
        }

        private void SetupDragEvents()
        {
            this.MouseDown += BaseNoteForm_MouseDown;
            this.MouseMove += BaseNoteForm_MouseMove;
            this.MouseUp += BaseNoteForm_MouseUp;

            if (txtTitle != null)
            {
                txtTitle.MouseDown += Title_MouseDown;
                txtTitle.MouseMove += Title_MouseMove;
                txtTitle.MouseUp += Title_MouseUp;
                txtTitle.ContextMenuStrip = contextMenu;
            }
        }

        protected virtual void ApplyData()
        {
            if (txtTitle != null)
                txtTitle.Text = NoteData.Title ?? "";

            var noteColor = NoteData.NoteColor;
            if (noteColor == Color.Empty || noteColor == default(Color))
                noteColor = Color.Yellow;

            SetColor(noteColor);

            this.Opacity = NoteData.Opacity > 0 ? NoteData.Opacity : 1.0f;
            if (NoteData.Location != Point.Empty) this.Location = NoteData.Location;
            if (NoteData.Size != Size.Empty) this.Size = NoteData.Size;

            if (trackOpacity != null)
            {
                trackOpacity.Value = (int)(this.Opacity * 100);
                if (lblOpacity != null)
                    lblOpacity.Text = $"Opacidad: {trackOpacity.Value}%";
            }
        }

        protected virtual void SetColor(Color color)
        {
            this.BackColor = color;
            if (txtTitle != null)
                txtTitle.BackColor = color;
        }

        #region Event Handlers

        private void TxtTitle_TextChanged(object? sender, EventArgs e)
        {
            StartAutoSaveTimer();
        }

        private void TrackOpacity_Scroll(object? sender, EventArgs e)
        {
            if (trackOpacity != null && lblOpacity != null)
            {
                this.Opacity = trackOpacity.Value / 100f;
                lblOpacity.Text = $"Opacidad: {trackOpacity.Value}%";
            }
        }

        protected void StartAutoSaveTimer()
        {
            autoSaveTimer?.Stop();
            autoSaveTimer?.Start();
        }

        private void AutoSaveTimer_Tick(object? sender, EventArgs e)
        {
            autoSaveTimer?.Stop();
            SaveNote();
        }

        private void BaseNoteForm_KeyDown(object? sender, KeyEventArgs e)
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

        #endregion

        #region Context Menu Handlers

        private void MenuColor_Click(object? sender, EventArgs e)
        {
            if (colorDialog?.ShowDialog() == DialogResult.OK)
            {
                SetColor(colorDialog.Color);
                SaveNote();
            }
        }

        private void MenuOpacity_Click(object? sender, EventArgs e)
        {
            ShowOpacityDialog();
        }

        private void MenuSave_Click(object? sender, EventArgs e)
        {
            SaveNote();
        }

        private void MenuDelete_Click(object? sender, EventArgs e)
        {
            var noteType = NoteData.Type == NoteType.TaskList ? "lista de tareas" : "nota";
            var result = MessageBox.Show($"¿Estás seguro de que quieres eliminar esta {noteType}?",
                               "Confirmar eliminación",
                               MessageBoxButtons.YesNo,
                               MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                OnNoteDeleted(); // Cambiar NoteDeleted?.Invoke por OnNoteDeleted()
                this.Close();
            }
        }

        #endregion

        #region Drag and Drop

        private void Title_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && txtTitle != null && !txtTitle.Focused)
            {
                StartDrag(e);
            }
        }

        private void Title_MouseMove(object? sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                MoveDrag(e);
            }
        }

        private void Title_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                EndDrag();
            }
        }

        private void BaseNoteForm_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                StartDrag(e);
            }
        }

        private void BaseNoteForm_MouseMove(object? sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                MoveDrag(e);
            }
        }

        private void BaseNoteForm_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                EndDrag();
            }
        }

        private void StartDrag(MouseEventArgs e)
        {
            isDragging = true;
            dragStartPoint = new Point(e.X, e.Y);
            this.Cursor = Cursors.SizeAll;
        }

        private void MoveDrag(MouseEventArgs e)
        {
            if (isDragging)
            {
                Point screenPoint = this.PointToScreen(new Point(e.X, e.Y));
                this.Location = new Point(screenPoint.X - dragStartPoint.X, screenPoint.Y - dragStartPoint.Y);
            }
        }

        private void EndDrag()
        {
            if (isDragging)
            {
                isDragging = false;
                this.Cursor = Cursors.Default;
                SaveNote();
            }
        }

        #endregion

        #region Helper Methods

        private void ShowOpacityDialog()
        {
            using (var opacityForm = new Form())
            {
                opacityForm.Text = "Cambiar Opacidad";
                opacityForm.Size = new Size(400, 250);
                opacityForm.StartPosition = FormStartPosition.CenterScreen;
                opacityForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                opacityForm.MaximizeBox = false;
                opacityForm.MinimizeBox = false;
                opacityForm.TopMost = true;
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
                    this.Opacity = trackBar.Value / 100f;
                };

                opacityForm.Controls.AddRange(new Control[] { label, trackBar, lblValue, btnOK, btnCancel });
                opacityForm.AcceptButton = btnOK;
                opacityForm.CancelButton = btnCancel;

                float originalOpacity = (float)this.Opacity;
                var result = opacityForm.ShowDialog();

                if (result == DialogResult.OK)
                {
                    this.Opacity = trackBar.Value / 100f;
                    if (trackOpacity != null)
                        trackOpacity.Value = trackBar.Value;
                    SaveNote();
                }
                else
                {
                    this.Opacity = originalOpacity;
                }
            }
        }

        protected static string ShowMultilineInput(string title, string prompt, string defaultText = "")
        {
            using (var form = new Form())
            {
                form.Text = title;
                form.Size = new Size(350, 220);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterParent;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.ShowInTaskbar = false;

                var lbl = new Label() { Text = prompt, Dock = DockStyle.Top, Height = 30 };
                var txt = new TextBox()
                {
                    Text = defaultText,
                    Multiline = true,
                    Dock = DockStyle.Fill,
                    ScrollBars = ScrollBars.Vertical,
                    AcceptsReturn = true
                };
                var btnOk = new Button() { Text = "Aceptar", DialogResult = DialogResult.OK, Dock = DockStyle.Bottom };
                var btnCancel = new Button() { Text = "Cancelar", DialogResult = DialogResult.Cancel, Dock = DockStyle.Bottom };

                form.Controls.Add(txt);
                form.Controls.Add(lbl);
                form.Controls.Add(btnOk);
                form.Controls.Add(btnCancel);

                form.AcceptButton = btnOk;
                form.CancelButton = btnCancel;

                var result = form.ShowDialog();
                return result == DialogResult.OK ? txt.Text : "";
            }
        }

        #endregion

        #region Abstract Methods

        public abstract void SaveNote();

        #endregion

        #region Override Methods

        protected override void OnPaint(PaintEventArgs e)
        {
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
                contextMenu?.Dispose();
                colorDialog?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        // Agregar estos métodos protegidos para que las clases derivadas puedan invocar los eventos
        protected virtual void OnNoteSaved()
        {
            NoteSaved?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnNoteDeleted()
        {
            NoteDeleted?.Invoke(this, EventArgs.Empty);
        }
    }
}