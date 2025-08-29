using System;

namespace Post_it
{
    [Serializable]
    public class TaskItem
    {
        public string Text { get; set; } = "";
        public bool IsCompleted { get; set; } = false;
    }
}