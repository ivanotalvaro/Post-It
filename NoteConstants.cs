using System.Drawing;

namespace Post_it
{
    public static class NoteConstants
    {
        // Colores predefinidos
        public static readonly Color DefaultYellow = Color.Yellow;
        public static readonly Color DefaultBlue = Color.LightBlue;
        public static readonly Color DefaultGreen = Color.LightGreen;
        public static readonly Color DefaultPink = Color.Pink;

        // Configuraciones
        public const int MinOpacity = 30;
        public const int MaxOpacity = 100;
        public const int DefaultOpacity = 100;
        public const int AutoSaveInterval = 2000; // ms

        // Tamaños
        public static readonly Size DefaultNoteSize = new Size(250, 200);
        public static readonly Size DefaultTaskListSize = new Size(250, 250);
        public static readonly Size MinNoteSize = new Size(200, 150);

        // Archivos
        public const string NotesFileName = "sticky_notes.json";
        public const string IconFileName = "postit.ico";

        // Mensajes
        public const string ConfirmDeleteNote = "¿Estás seguro de que quieres eliminar esta nota?";
        public const string ConfirmDeleteTaskList = "¿Estás seguro de que quieres eliminar esta lista de tareas?";
        public const string AppAlreadyRunning = "La aplicación Post-it Digital ya está en ejecución.\n\nRevisa la bandeja del sistema.";
    }
}