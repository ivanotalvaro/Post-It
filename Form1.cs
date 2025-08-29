using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;

namespace Post_it
{
    public partial class Form1 : Form
    {
        private List<StickyNoteData> notes = new List<StickyNoteData>();
        private ListBox? lstNotes;
        private Button? btnNewNote;
        private Button? btnNewTaskList;
        private Button? btnDeleteNote;
        private NotifyIcon? notifyIcon;
        private ContextMenuStrip? trayMenu;

        public Form1()
        {
            InitializeComponent();
            InitializeCustomComponents();
            LoadNotes();
            SetupTrayIcon();
            ShowExistingNotes();
            
            // Minimizar inmediatamente al cargar
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Hide();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Post-it Digital";
            this.Size = new Size(300, 400);
            this.StartPosition = FormStartPosition.CenterScreen;

            btnNewNote = new Button
            {
                Text = "📝 Nueva Nota",
                Dock = DockStyle.Top,
                Height = 40,
                Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold),
                BackColor = Color.LightYellow,
                FlatStyle = FlatStyle.Flat
            };
            btnNewNote.Click += BtnNewNote_Click;

            btnNewTaskList = new Button
            {
                Text = "📋 Nueva Lista de Tareas",
                Dock = DockStyle.Top,
                Height = 40,
                Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold),
                BackColor = Color.LightSkyBlue,
                FlatStyle = FlatStyle.Flat
            };
            btnNewTaskList.Click += BtnNewTaskList_Click;

            btnDeleteNote = new Button
            {
                Text = "🗑️ Eliminar Seleccionada",
                Dock = DockStyle.Bottom,
                Height = 40,
                Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold),
                BackColor = Color.LightCoral,
                FlatStyle = FlatStyle.Flat
            };
            btnDeleteNote.Click += BtnDeleteNote_Click;

            lstNotes = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font(FontFamily.GenericSansSerif, 9)
            };
            lstNotes.DoubleClick += LstNotes_DoubleClick;

            Controls.Add(lstNotes);
            Controls.Add(btnDeleteNote);
            Controls.Add(btnNewTaskList);
            Controls.Add(btnNewNote);

            UpdateNotesList();
        }

        protected override void SetVisibleCore(bool value)
        {
            // Evitar que el formulario se muestre automáticamente al inicio
            if (!this.IsHandleCreated)
            {
                this.CreateHandle();
                base.SetVisibleCore(false);
                return;
            }
            
            base.SetVisibleCore(value);
        }

        // Sobrescribir el método para manejar el evento de minimizar
        protected override void WndProc(ref Message m)
        {
            const int WM_SIZE = 0x0005;
            const int SIZE_MINIMIZED = 1;

            if (m.Msg == WM_SIZE && (int)m.WParam == SIZE_MINIMIZED)
            {
                this.Hide();
                return;
            }

            base.WndProc(ref m);
        }

        private void SetupTrayIcon()
        {
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Nueva Nota", null, BtnNewNote_Click);
            trayMenu.Items.Add("Nueva Lista de Tareas", null, BtnNewTaskList_Click);
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("Mostrar", null, (s, e) => { 
                this.Show(); 
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
                this.BringToFront();
                this.Activate();
            });
            trayMenu.Items.Add("Salir", null, (s, e) => Application.Exit());

            notifyIcon = new NotifyIcon
            {
                Icon = this.Icon ?? SystemIcons.Application,
                Text = "Post-it Digital",
                Visible = true,
                ContextMenuStrip = trayMenu
            };
            
            notifyIcon.DoubleClick += (s, e) => { 
                if (this.Visible)
                {
                    this.Hide();
                    this.ShowInTaskbar = false;
                }
                else
                {
                    this.Show(); 
                    this.WindowState = FormWindowState.Normal;
                    this.ShowInTaskbar = true;
                    this.BringToFront();
                    this.Activate();
                }
            };
        }

        private void UpdateNotesList()
        {
            lstNotes?.Items.Clear();
            foreach (var note in notes)
            {
                string tipo = note.Type == NoteType.TaskList ? "[Lista de tareas]" : "[Nota]";
                string titulo = string.IsNullOrWhiteSpace(note.Title) ? "(Sin título)" : note.Title;
                lstNotes?.Items.Add($"{tipo} {titulo}");
            }
        }

        private void BtnNewNote_Click(object? sender, EventArgs e)
        {
            var newNote = new StickyNoteData();
            var noteForm = new StickyNoteForm(newNote);
            SetupNoteFormEvents(noteForm);

            noteForm.Location = new Point(100 + (notes.Count * 30), 100 + (notes.Count * 30));
            noteForm.Show();

            notes.Add(newNote);
            UpdateNotesList();
            SaveNotes();
        }

        private void BtnNewTaskList_Click(object? sender, EventArgs e)
        {
            var newNote = new StickyNoteData { Type = NoteType.TaskList, Tasks = new List<TaskItem>() };
            var noteForm = new TaskListNoteForm(newNote);
            SetupNoteFormEvents(noteForm);

            noteForm.Location = new Point(120 + (notes.Count * 30), 120 + (notes.Count * 30));
            noteForm.Show();

            notes.Add(newNote);
            UpdateNotesList();
            SaveNotes();
        }

        private void BtnDeleteNote_Click(object? sender, EventArgs e)
        {
            DeleteSelectedNote();
        }

        private void LstNotes_DoubleClick(object? sender, EventArgs e)
        {
            if (lstNotes?.SelectedItem is string selectedText)
            {
                var note = FindNoteByDisplayText(selectedText);
                if (note != null)
                {
                    var openForm = FindExistingNoteForm(note);
                    if (openForm != null)
                    {
                        openForm.BringToFront();
                        openForm.Activate();
                    }
                    else
                    {
                        var noteForm = CreateNoteForm(note);
                        SetupNoteFormEvents(noteForm);
                        noteForm.Show();
                    }
                }
            }
        }

        private void DeleteSelectedNote()
        {
            if (lstNotes?.SelectedItem is string selectedText)
            {
                var note = FindNoteByDisplayText(selectedText);
                if (note != null)
                {
                    var result = MessageBox.Show($"¿Estás seguro de que quieres eliminar la nota '{note.Title}'?",
                                                 "Confirmar eliminación",
                                                 MessageBoxButtons.YesNo,
                                                 MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        notes.Remove(note);
                        UpdateNotesList();
                        SaveNotes();

                        // Cerrar la ventana si está abierta
                        var openForm = FindExistingNoteForm(note);
                        openForm?.Close();
                    }
                }
            }
        }

        private BaseNoteForm? FindExistingNoteForm(StickyNoteData note)
        {
            return Application.OpenForms.Cast<Form>()
                .OfType<BaseNoteForm>()
                .FirstOrDefault(f => f.NoteData == note);
        }

        private BaseNoteForm CreateNoteForm(StickyNoteData note)
        {
            return note.Type == NoteType.TaskList 
                ? new TaskListNoteForm(note) 
                : new StickyNoteForm(note);
        }

        private void SetupNoteFormEvents(BaseNoteForm noteForm)
        {
            noteForm.NoteSaved += NoteForm_NoteSaved;
            noteForm.NoteDeleted += NoteForm_NoteDeleted;
        }

        private StickyNoteData? FindNoteByDisplayText(string displayText)
        {
            return notes.FirstOrDefault(n =>
            {
                string tipo = n.Type == NoteType.TaskList ? "[Lista de tareas]" : "[Nota]";
                string titulo = string.IsNullOrWhiteSpace(n.Title) ? "(Sin título)" : n.Title;
                return $"{tipo} {titulo}" == displayText;
            });
        }

        private void ShowExistingNotes()
        {
            foreach (var note in notes)
            {
                var existingForm = FindExistingNoteForm(note);
                if (existingForm == null)
                {
                    var noteForm = CreateNoteForm(note);
                    SetupNoteFormEvents(noteForm);

                    if (note.Location == Point.Empty)
                    {
                        note.Location = GenerateRandomLocation();
                    }

                    noteForm.Location = note.Location;
                    noteForm.Show();
                }
            }
        }

        private Point GenerateRandomLocation()
        {
            Random rand = new Random();
            var workingArea = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1024, 768);
            return new Point(
                rand.Next(50, Math.Max(350, workingArea.Width - 300)),
                rand.Next(50, Math.Max(300, workingArea.Height - 250))
            );
        }

        private void NoteForm_NoteSaved(object? sender, EventArgs e)
        {
            UpdateNotesList();
            SaveNotes();
        }

        private void NoteForm_NoteDeleted(object? sender, EventArgs e)
        {
            if (sender is BaseNoteForm form)
            {
                notes.Remove(form.NoteData);
                UpdateNotesList();
                SaveNotes();
            }
        }

        private void LoadNotes()
        {
            try
            {
                string notesFile = "sticky_notes.json";
                if (File.Exists(notesFile))
                {
                    string json = File.ReadAllText(notesFile);
                    notes = JsonSerializer.Deserialize<List<StickyNoteData>>(json) ?? new List<StickyNoteData>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando notas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                notes = new List<StickyNoteData>();
            }
        }

        private void SaveNotes()
        {
            try
            {
                string json = JsonSerializer.Serialize(notes, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText("sticky_notes.json", json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error guardando notas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Remover el método Dispose y agregar este método de limpieza
        private void CleanupResources()
        {
            notifyIcon?.Dispose();
            trayMenu?.Dispose();
        }

        // Llamar CleanupResources en el evento FormClosing
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Si el usuario cierra la ventana, minimizar a la bandeja en lugar de cerrar
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                this.ShowInTaskbar = false;
                
                // Mostrar notificación la primera vez
                if (notifyIcon != null && notifyIcon.Visible)
                {
                    notifyIcon.BalloonTipTitle = "Post-it Digital";
                    notifyIcon.BalloonTipText = "La aplicación se ha minimizado a la bandeja del sistema.";
                    notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                    notifyIcon.ShowBalloonTip(3000);
                }
                return;
            }
            
            CleanupResources();
            base.OnFormClosing(e);
        }
    }
}
