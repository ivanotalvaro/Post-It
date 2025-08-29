using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

namespace Post_it
{
    public class TaskListNoteForm : BaseNoteForm
    {
        private FlowLayoutPanel? panelTasks;
        private Button? btnAddTask;

        public TaskListNoteForm(StickyNoteData? data = null) : base(data)
        {
            InitializeSpecificComponents();
            ApplyData();
        }

        protected override StickyNoteData CreateDefaultNoteData()
        {
            return new StickyNoteData 
            { 
                Type = NoteType.TaskList, 
                Tasks = new List<TaskItem>() 
            };
        }

        private void InitializeSpecificComponents()
        {
            // Asegurar que Tasks no sea null
            if (NoteData.Tasks == null) 
                NoteData.Tasks = new List<TaskItem>();

            // Panel para las tareas
            panelTasks = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(10)
            };
            panelTasks.ContextMenuStrip = contextMenu;

            // Botón para agregar tareas
            btnAddTask = new Button
            {
                Text = "➕ Agregar tarea",
                Dock = DockStyle.Bottom,
                Height = 30,
                BackColor = Color.LightGreen,
                FlatStyle = FlatStyle.Flat
            };
            btnAddTask.Click += BtnAddTask_Click;

            // Configurar arrastre para los componentes
            panelTasks.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) base.OnMouseDown(e); };
            panelTasks.MouseMove += (s, e) => base.OnMouseMove(e);
            panelTasks.MouseUp += (s, e) => { if (e.Button == MouseButtons.Left) base.OnMouseUp(e); };

            // Layout principal
            var tablePanel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
            tablePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            tablePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tablePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            
            // Verificar que los controles no sean null antes de agregarlos
            if (txtTitle != null)
                tablePanel.Controls.Add(txtTitle, 0, 0);
            if (panelTasks != null)
                tablePanel.Controls.Add(panelTasks, 0, 1);
            if (btnAddTask != null)
                tablePanel.Controls.Add(btnAddTask, 0, 2);
            
            tablePanel.BackColor = Color.Transparent;
            tablePanel.ContextMenuStrip = contextMenu;

            // Configurar arrastre para el panel principal
            tablePanel.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) base.OnMouseDown(e); };
            tablePanel.MouseMove += (s, e) => base.OnMouseMove(e);
            tablePanel.MouseUp += (s, e) => { if (e.Button == MouseButtons.Left) base.OnMouseUp(e); };

            Controls.Add(tablePanel);
        }

        protected override void SetColor(Color color)
        {
            base.SetColor(color);
            if (panelTasks != null)
            {
                panelTasks.BackColor = color;
                foreach (Control c in panelTasks.Controls)
                {
                    if (c is CheckBox chk)
                        chk.BackColor = color;
                }
            }
        }

        protected override void ApplyData()
        {
            base.ApplyData();
            
            if (panelTasks != null)
            {
                panelTasks.Controls.Clear();
                if (NoteData.Tasks != null)
                {
                    foreach (var task in NoteData.Tasks)
                    {
                        AddTaskControl(task);
                    }
                }
            }
            
            AdjustFormSizeToContent();
        }

        private void AddTaskControl(TaskItem task)
        {
            if (panelTasks == null) return;

            var chk = new CheckBox
            {
                Text = task.Text,
                Checked = task.IsCompleted,
                AutoSize = true,
                Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular),
                BackColor = this.BackColor,
                Padding = new Padding(2)
            };

            // Aplicar estilo según el estado
            chk.Font = new Font(chk.Font, chk.Checked ? FontStyle.Strikeout : FontStyle.Regular);

            // Eventos
            chk.CheckedChanged += (s, e) =>
            {
                task.IsCompleted = chk.Checked;
                chk.Font = new Font(chk.Font, chk.Checked ? FontStyle.Strikeout : FontStyle.Regular);
                StartAutoSaveTimer();
                AdjustFormSizeToContent();
            };

            // Editar tarea con doble clic
            chk.DoubleClick += (s, e) => EditTask(task, chk);

            // Menú contextual específico para tareas
            var taskMenu = new ContextMenuStrip();
            taskMenu.Items.Add("Editar", null, (s, e) => EditTask(task, chk));
            taskMenu.Items.Add("Eliminar", null, (s, e) => RemoveTask(task, chk));
            taskMenu.Items.Add(new ToolStripSeparator());
            
            // Agregar elementos del menú principal con nuevos manejadores
            if (contextMenu != null)
            {
                // Amarillo
                taskMenu.Items.Add("Amarillo", null, (s, e) => SetColor(Color.Yellow));
                // Azul
                taskMenu.Items.Add("Azul", null, (s, e) => SetColor(Color.LightBlue));
                // Verde
                taskMenu.Items.Add("Verde", null, (s, e) => SetColor(Color.LightGreen));
                // Rosa
                taskMenu.Items.Add("Rosa", null, (s, e) => SetColor(Color.Pink));
                
                taskMenu.Items.Add(new ToolStripSeparator());
                
                // Más colores
                taskMenu.Items.Add("Más colores...", null, (s, e) => 
                {
                    if (colorDialog?.ShowDialog() == DialogResult.OK)
                    {
                        SetColor(colorDialog.Color);
                        SaveNote();
                    }
                });
                
                taskMenu.Items.Add(new ToolStripSeparator());
                
                // Cambiar opacidad
                taskMenu.Items.Add("Cambiar opacidad...", null, (s, e) => ShowOpacityDialog());
                
                taskMenu.Items.Add(new ToolStripSeparator());
                
                // Guardar
                taskMenu.Items.Add("Guardar ahora (Ctrl+S)", null, (s, e) => SaveNote());
                // Eliminar nota completa
                taskMenu.Items.Add("Eliminar nota", null, (s, e) => 
                {
                    var noteType = NoteData.Type == NoteType.TaskList ? "lista de tareas" : "nota";
                    var result = MessageBox.Show($"¿Estás seguro de que quieres eliminar esta {noteType}?",
                                               "Confirmar eliminación",
                                               MessageBoxButtons.YesNo,
                                               MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        OnNoteDeleted(); // CAMBIAR: NoteSaved?.Invoke por OnNoteDeleted()
                        this.Close();
                    }
                });
            }
            
            chk.ContextMenuStrip = taskMenu;

            panelTasks.Controls.Add(chk);
            AdjustFormSizeToContent();
        }

        private void EditTask(TaskItem task, CheckBox checkbox)
        {
            var input = ShowMultilineInput("Editar tarea", "Edita la tarea:", checkbox.Text);
            if (!string.IsNullOrWhiteSpace(input))
            {
                checkbox.Text = input;
                task.Text = input;
                StartAutoSaveTimer();
                AdjustFormSizeToContent();
            }
        }

        private void RemoveTask(TaskItem task, CheckBox checkbox)
        {
            var result = MessageBox.Show($"¿Eliminar la tarea '{task.Text}'?", 
                                       "Confirmar eliminación", 
                                       MessageBoxButtons.YesNo, 
                                       MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                NoteData.Tasks?.Remove(task);
                panelTasks?.Controls.Remove(checkbox);
                checkbox.Dispose();
                AdjustFormSizeToContent();
                StartAutoSaveTimer();
            }
        }

        private void BtnAddTask_Click(object? sender, EventArgs e)
        {
            var input = ShowMultilineInput("Nueva tarea", "Escribe la tarea:", "");
            if (!string.IsNullOrWhiteSpace(input))
            {
                var task = new TaskItem { Text = input, IsCompleted = false };
                NoteData.Tasks?.Add(task);
                AddTaskControl(task);
                StartAutoSaveTimer();
            }
        }

        public override void SaveNote()
        {
            if (txtTitle != null)
                NoteData.Title = txtTitle.Text;

            NoteData.NoteColor = this.BackColor;
            NoteData.Opacity = (float)this.Opacity;
            NoteData.Location = this.Location;
            NoteData.Size = this.Size;

            // Actualizar tareas desde los controles
            if (panelTasks != null)
            {
                NoteData.Tasks = new List<TaskItem>();
                foreach (Control c in panelTasks.Controls)
                {
                    if (c is CheckBox chk)
                    {
                        NoteData.Tasks.Add(new TaskItem { Text = chk.Text, IsCompleted = chk.Checked });
                    }
                }
            }

            OnNoteSaved(); // Cambiar NoteSaved?.Invoke por OnNoteSaved()
        }

        private void AdjustFormSizeToContent()
        {
            if (panelTasks == null || btnAddTask == null || txtTitle == null) return;

            int minWidth = 250;
            int minHeight = 150;
            int width = minWidth;
            int height = 60 + btnAddTask.Height + txtTitle.Height;

            foreach (Control c in panelTasks.Controls)
            {
                width = Math.Max(width, c.PreferredSize.Width + 40);
                height += c.PreferredSize.Height + 5;
            }

            this.Size = new Size(width, Math.Max(height, minHeight));
            NoteData.Size = this.Size;
        }

        // Agregar método para mostrar diálogo de opacidad (necesario para el menú contextual)
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
    }
}