using System;
using System.Drawing;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Post_it
{
    public enum NoteType
    {
        Normal,
        TaskList
    }

    [Serializable]
    public class StickyNoteData
    {
        public string Title { get; set; } = "";
        public string Text { get; set; } = "";

        // Nueva propiedad para tareas
        public List<TaskItem>? Tasks { get; set; }

        public NoteType Type { get; set; } = NoteType.Normal;

        // Propiedades para serializar el color como ARGB
        [JsonIgnore]
        public Color NoteColor { get; set; } = Color.Yellow;
        
        public int NoteColorArgb 
        { 
            get => NoteColor.ToArgb(); 
            set => NoteColor = Color.FromArgb(value); 
        }
        
        public float Opacity { get; set; } = 1.0f;
        public Point Location { get; set; }
        public Size Size { get; set; }
    }
}
