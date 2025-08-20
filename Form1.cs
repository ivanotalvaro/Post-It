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

    public Form1()
    {
        InitializeComponent();
        
        try { this.Icon = new System.Drawing.Icon("postit.ico"); } catch { }
        
        this.Text = "Post-it Manager";
        this.Size = new Size(300, 200);
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
        
        lblInfo = new Label { 
            Text = "• Clic derecho en las notas para más opciones\n• Ctrl+S para guardar\n• Ctrl+Del para eliminar\n• Arrastra las notas para moverlas", 
            Dock = DockStyle.Fill,
            Font = new Font(FontFamily.GenericSansSerif, 9),
            Padding = new Padding(10),
            BackColor = Color.WhiteSmoke
        };
        
        btnNewNote.Click += BtnNewNote_Click;
        
        Controls.Add(lblInfo);
        Controls.Add(btnNewNote);
        Controls.Add(lblTitle);
        
        LoadNotes();
    }

    private void SetupNotifyIcon()
    {
        // Crear menú contextual para el icono de la bandeja
        trayMenu = new ContextMenuStrip();
        trayMenu.Items.Add("Nueva Nota", null, BtnNewNote_Click);
        trayMenu.Items.Add("Mostrar/Ocultar", null, ToggleWindow_Click);
        trayMenu.Items.Add(new ToolStripSeparator());
        trayMenu.Items.Add("Salir", null, Exit_Click);

        // Configurar NotifyIcon
        notifyIcon = new NotifyIcon();
        notifyIcon.Icon = this.Icon ?? SystemIcons.Application;
        notifyIcon.Text = "Post-it Digital - Haz clic para abrir";
        notifyIcon.Visible = true;
        notifyIcon.ContextMenuStrip = trayMenu;
        notifyIcon.DoubleClick += ToggleWindow_Click;
    }

    private void ToggleWindow_Click(object? sender, EventArgs e)
    {
        if (this.WindowState == FormWindowState.Minimized || !this.Visible)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            this.BringToFront();
        }
        else
        {
            this.Hide();
            this.ShowInTaskbar = false;
        }
    }

    private void Exit_Click(object? sender, EventArgs e)
    {
        notifyIcon?.Dispose();
        Application.Exit();
    }

    protected override void SetVisibleCore(bool value)
    {
        // Evitar que la ventana se muestre al inicio
        base.SetVisibleCore(value && this.WindowState != FormWindowState.Minimized);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Al cerrar, minimizar a la bandeja en lugar de cerrar completamente
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            this.Hide();
            this.ShowInTaskbar = false;
            
            // Mostrar notificación la primera vez
            if (notifyIcon != null)
            {
                notifyIcon.ShowBalloonTip(3000, "Post-it Digital", 
                    "La aplicación sigue ejecutándose en la bandeja del sistema", 
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
        var noteForm = new StickyNoteForm();
        noteForm.NoteSaved += NoteForm_NoteSaved;
        noteForm.NoteDeleted += NoteForm_NoteDeleted;
        
        // Posicionar la nueva nota en una ubicación visible
        var random = new Random();
        noteForm.Location = new Point(random.Next(100, 800), random.Next(100, 600));
        
        noteForm.Show();
    }

    private void NoteForm_NoteSaved(object? sender, EventArgs e)
    {
        var form = sender as StickyNoteForm;
        if (form != null)
        {
            notes.RemoveAll(n => n.Location == form.NoteData.Location && n.Size == form.NoteData.Size);
            notes.Add(form.NoteData);
            SaveNotes();
        }
    }

    private void LoadNotes()
    {
        if (File.Exists(NotesFile))
        {
            try
            {
                var json = File.ReadAllText(NotesFile);
                var options = new System.Text.Json.JsonSerializerOptions { IncludeFields = true };
                var loadedNotes = System.Text.Json.JsonSerializer.Deserialize<List<StickyNoteData>>(json, options);
                if (loadedNotes != null)
                {
                    notes = loadedNotes;
                    Console.WriteLine($"Cargadas {notes.Count} notas desde {NotesFile}");
                    foreach (var note in notes)
                    {
                        Console.WriteLine($"Cargando nota con color: {note.NoteColor.Name} (ARGB: {note.NoteColor.ToArgb()})");
                        var noteForm = new StickyNoteForm(note);
                        noteForm.NoteSaved += NoteForm_NoteSaved;
                        noteForm.NoteDeleted += NoteForm_NoteDeleted;
                        noteForm.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar notas: {ex.Message}");
            }
        }
    }

    private void NoteForm_NoteDeleted(object? sender, EventArgs e)
    {
        var form = sender as StickyNoteForm;
        if (form != null)
        {
            notes.RemoveAll(n => n.Location == form.NoteData.Location && n.Size == form.NoteData.Size);
            SaveNotes();
        }
    }

    private void SaveNotes()
    {
        var options = new System.Text.Json.JsonSerializerOptions 
        { 
            IncludeFields = true,
            WriteIndented = true
        };
        var json = System.Text.Json.JsonSerializer.Serialize(notes, options);
        File.WriteAllText(NotesFile, json);
        
        // Debug: Mostrar cuántas notas se guardaron
        Console.WriteLine($"Guardadas {notes.Count} notas en {NotesFile}");
    }
}
