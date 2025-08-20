using System;
using System.Drawing;
using System.Text.Json.Serialization;

namespace Post_it
{
    [Serializable]
    public class StickyNoteData
    {
        public string Title { get; set; } = "";
        public string Text { get; set; } = "";
        
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
