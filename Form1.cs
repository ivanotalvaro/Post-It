namespace Post_it;

public partial class Form1 : Form
{
    private const string NotesFile = "sticky_notes.json";
    private List<StickyNoteData> notes = new List<StickyNoteData>();
    private Button? btnNewNote;
    private Label? lblTitle;
    private Label? lblInfo;
    private NotifyIcon? notifyIcon;
    private ContextMenuStrip? trayMenu;
    private ListBox? lstNotes;
    private Panel? pnlBottom;

    public Form1()
    {
        InitializeComponent();
        
        // Cargar el icono para el formulario
        try 
        { 
            this.Icon = new System.Drawing.Icon("postit.ico"); 
        } 
        catch 
        { 
            // Si no se encuentra el archivo externo, usar icono por defecto
            this.Icon = SystemIcons.Application;
        }
        
        this.Text = "Post-it Manager";
        this.Size = new Size(400, 500);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.WindowState = FormWindowState.Minimized;
        this.ShowInTaskbar = false;
        
        // Configurar NotifyIcon
        SetupNotifyIcon();
        
        lblTitle = new Label { 
            Text = "📝 Post-it Digital", 
            Dock = DockStyle.Top, 
            Font = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Height = 40,
            BackColor = Color.LightYellow
        };
        
        btnNewNote = new Button { 
            Text = "➕ Crear Nueva Nota", 
            Dock = DockStyle.Top, 
            Height = 50,
            Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold),
            BackColor = Color.Yellow,
            FlatStyle = FlatStyle.Flat
        };

        // Panel inferior para info y lista de notas
        pnlBottom = new Panel { Dock = DockStyle.Fill };
        
        lblInfo = new Label { 
            Text = "• Doble clic en una nota para abrirla\n• Clic derecho para opciones", 
            Dock = DockStyle.Top,
            Font = new Font(FontFamily.GenericSansSerif, 9),
            Padding = new Padding(10),
            BackColor = Color.WhiteSmoke,
            Height = 50
        };

        lstNotes = new ListBox { 
            Dock = DockStyle.Fill,
            Font = new Font(FontFamily.GenericSansSerif, 9),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        
        btnNewNote.Click += BtnNewNote_Click;
        lstNotes.DoubleClick += LstNotes_DoubleClick;
        lstNotes.MouseDown += LstNotes_MouseDown;
        
        pnlBottom.Controls.Add(lstNotes);
        pnlBottom.Controls.Add(lblInfo);
        
        Controls.Add(pnlBottom);
        Controls.Add(btnNewNote);
        Controls.Add(lblTitle);
        
        LoadNotes();
        UpdateNotesList();
        
        // Mostrar las notas existentes automáticamente al iniciar
        ShowExistingNotes();
    }

    private void SetupNotifyIcon()
    {
        // Crear menú contextual para el icono de la bandeja
        trayMenu = new ContextMenuStrip();
        trayMenu.Items.Add("Nueva Nota", null, BtnNewNote_Click);
        trayMenu.Items.Add("Mostrar/Ocultar", null, ToggleWindow_Click);
        trayMenu.Items.Add(new ToolStripSeparator());
        trayMenu.Items.Add("Salir", null, Exit_Click);

        // Configurar NotifyIcon con el mismo icono del formulario
        notifyIcon = new NotifyIcon();
        notifyIcon.Icon = this.Icon;
        notifyIcon.Text = "Post-it Digital - Haz clic para abrir";
        notifyIcon.Visible = true;
        notifyIcon.ContextMenuStrip = trayMenu;
        notifyIcon.DoubleClick += ToggleWindow_Click;
    }

    private void ShowExistingNotes()
    {
        // Mostrar todas las notas existentes al iniciar la aplicación
        foreach (var note in notes)
        {
            // Verificar que no esté ya abierta
            var existingForm = Application.OpenForms.Cast<Form>()
                .OfType<StickyNoteForm>()
                .FirstOrDefault(f => f.NoteData == note);
                
            if (existingForm == null)
            {
                var noteForm = new StickyNoteForm(note);
                noteForm.NoteSaved += NoteForm_NoteSaved;
                noteForm.NoteDeleted += NoteForm_NoteDeleted;
                
                // Si la nota no tiene posición guardada, asignar una por defecto
                if (note.Location == Point.Empty)
                {
                    Random rand = new Random();
                    note.Location = new Point(
                        rand.Next(50, Screen.PrimaryScreen.WorkingArea.Width - 300),
                        rand.Next(50, Screen.PrimaryScreen.WorkingArea.Height - 250)
                    );
                }
                
                noteForm.Show();
            }
        }
    }

    private void ToggleWindow_Click(object? sender, EventArgs e)
    {
        if (this.WindowState == FormWindowState.Minimized || !this.Visible)
        {
            // Mostrar la ventana
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            this.BringToFront();
            this.Activate();
            this.Focus();
            
            // Forzar que la ventana aparezca al frente
            this.TopMost = true;
            this.TopMost = false;
        }
        else
        {
            // Ocultar la ventana
            this.Hide();
            this.ShowInTaskbar = false;
        }
    }

    private void Exit_Click(object? sender, EventArgs e)
    {
        // Cerrar todas las notas abiertas
        var openForms = Application.OpenForms.Cast<Form>().ToArray();
        foreach (var form in openForms)
        {
            if (form is StickyNoteForm)
            {
                form.Close();
            }
        }
        
        // Limpiar el NotifyIcon
        if (notifyIcon != null)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
        }
        
        Application.Exit();
    }

    protected override void SetVisibleCore(bool value)
    {
        // Iniciar siempre minimizado en la bandeja
        base.SetVisibleCore(value);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            // No cerrar la aplicación, solo minimizar a la bandeja
            e.Cancel = true;
            this.Hide();
            this.ShowInTaskbar = false;
            if (notifyIcon != null)
            {
                // Mostrar un globo informativo la primera vez
                notifyIcon.ShowBalloonTip(3000, "Post-it Digital", 
                    "La aplicación sigue ejecutándose en segundo plano. " +
                    "Haz clic derecho en el icono para ver las opciones.", 
                    ToolTipIcon.Info);
            }
        }
        else
        {
            base.OnFormClosing(e);
        }
    }

    private void BtnNewNote_Click(object? sender, EventArgs e)
    {
        var newNote = new StickyNoteData();
        var noteForm = new StickyNoteForm(newNote);
        noteForm.NoteSaved += NoteForm_NoteSaved;
        noteForm.NoteDeleted += NoteForm_NoteDeleted;
        
        // Posicionar la nueva nota en una ubicación visible
        noteForm.Location = new Point(100 + (notes.Count * 30), 100 + (notes.Count * 30));
        noteForm.Show();
        
        notes.Add(newNote);
        UpdateNotesList();
        SaveNotes();
    }

    private void LstNotes_DoubleClick(object? sender, EventArgs e)
    {
        if (lstNotes?.SelectedItem is string selectedText)
        {
            var note = notes.FirstOrDefault(n => GetNoteDisplayText(n) == selectedText);
            if (note != null)
            {
                // Verificar si ya hay una ventana abierta para esta nota
                var openForm = Application.OpenForms.Cast<Form>()
                    .OfType<StickyNoteForm>()
                    .FirstOrDefault(f => f.NoteData == note);
                
                if (openForm != null)
                {
                    // Si ya está abierta, traerla al frente
                    openForm.BringToFront();
                    openForm.Activate();
                }
                else
                {
                    // Si no está abierta, crear nueva ventana
                    var noteForm = new StickyNoteForm(note);
                    noteForm.NoteSaved += NoteForm_NoteSaved;
                    noteForm.NoteDeleted += NoteForm_NoteDeleted;
                    noteForm.Show();
                }
            }
        }
    }

    private void LstNotes_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right && lstNotes != null)
        {
            int index = lstNotes.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                lstNotes.SelectedIndex = index;
                var contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add("Abrir", null, (s, args) => LstNotes_DoubleClick(s, args));
                contextMenu.Items.Add("Eliminar", null, (s, args) => DeleteSelectedNote());
                contextMenu.Show(lstNotes, e.Location);
            }
        }
    }

    private void DeleteSelectedNote()
    {
        if (lstNotes?.SelectedItem is string selectedText)
        {
            var note = notes.FirstOrDefault(n => GetNoteDisplayText(n) == selectedText);
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
                    var openForm = Application.OpenForms.Cast<Form>()
                        .OfType<StickyNoteForm>()
                        .FirstOrDefault(f => f.NoteData == note);
                    openForm?.Close();
                }
            }
        }
    }

    private string GetNoteDisplayText(StickyNoteData note)
    {
        // Mostrar solo el título, o "Sin título" si está vacío
        string title = string.IsNullOrWhiteSpace(note.Title) ? "Sin título" : note.Title.Trim();
        
        // Limitar la longitud del título mostrado
        if (title.Length > 40)
        {
            title = title.Substring(0, 37) + "...";
        }
        
        return title;
    }

    private void UpdateNotesList()
    {
        if (lstNotes != null)
        {
            lstNotes.Items.Clear();
            lstNotes.Items.AddRange(notes.Select(GetNoteDisplayText).ToArray());
            
            if (lblInfo != null)
            {
                lblInfo.Text = $"• Total de notas: {notes.Count}\n• Doble clic para abrir • Clic derecho para opciones";
            }
        }
    }

    private void NoteForm_NoteSaved(object? sender, EventArgs e)
    {
        if (sender is StickyNoteForm form)
        {
            UpdateNotesList();
            SaveNotes();
        }
    }

    private void LoadNotes()
    {
        if (File.Exists(NotesFile))
        {
            try
            {
                string json = File.ReadAllText(NotesFile);
                var loadedNotes = System.Text.Json.JsonSerializer.Deserialize<List<StickyNoteData>>(json);
                if (loadedNotes != null)
                {
                    notes = loadedNotes;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar las notas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    private void NoteForm_NoteDeleted(object? sender, EventArgs e)
    {
        if (sender is StickyNoteForm form)
        {
            notes.Remove(form.NoteData);
            UpdateNotesList();
            SaveNotes();
        }
    }

    private void SaveNotes()
    {
        try
        {
            string json = System.Text.Json.JsonSerializer.Serialize(notes, new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(NotesFile, json);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al guardar las notas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
