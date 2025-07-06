using HomeAutomation.BusinessLogic.Services.Routines;
using HomeAutomation.BusinessLogic.Services.Devices; // Para seleccionar dispositivos para la rutina
using HomeAutomation.DataAccess.Entities; // Para Routine, User
using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

namespace HomeAutomation.UI.Forms.Routines
{
    public partial class RoutineManagementForm : Form
    {
        private readonly IRoutineService _routineService;
        private readonly IDeviceService _deviceService; // Para obtener lista de dispositivos
        private readonly User _currentUser;
        private List<Routine> _allRoutines;

        public RoutineManagementForm(IRoutineService routineService, IDeviceService deviceService, User currentUser)
        {
            InitializeComponent();
            _routineService = routineService;
            _deviceService = deviceService;
            _currentUser = currentUser;
        }

        private void RoutineManagementForm_Load(object sender, EventArgs e)
        {
            LoadRoutines();
            SetupDataGridView();
        }

        private void SetupDataGridView()
        {
            dgvRoutines.AutoGenerateColumns = false;
            dgvRoutines.Columns.Clear();

            dgvRoutines.Columns.Add(new DataGridViewTextBoxColumn { Name = "RoutineId", HeaderText = "ID", DataPropertyName = "RoutineId", Visible = false });
            dgvRoutines.Columns.Add(new DataGridViewTextBoxColumn { Name = "RoutineName", HeaderText = "Nombre", DataPropertyName = "Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvRoutines.Columns.Add(new DataGridViewTextBoxColumn { Name = "ScheduledTime", HeaderText = "Hora Programada", DataPropertyName = "ScheduledTime", DefaultCellStyle = new DataGridViewCellStyle { Format = "hh\\:mm" } });
            dgvRoutines.Columns.Add(new DataGridViewTextBoxColumn { Name = "ExecutionDays", HeaderText = "Días", DataPropertyName = "ExecutionDaysString" }); // Necesitará un DTO o propiedad formateada
            dgvRoutines.Columns.Add(new DataGridViewCheckBoxColumn { Name = "IsEnabled", HeaderText = "Habilitada", DataPropertyName = "IsEnabled" });
        }


        private void LoadRoutines()
        {
            try
            {
                // Aquí podríamos querer filtrar rutinas por _currentUser.UserId si no es Admin
                // Esto debería hacerse en el RoutineService o Repository
                _allRoutines = _routineService.GetAllRoutines().Where(r => r.UserId == _currentUser.UserId).ToList(); // O una llamada específica GetRoutinesByUserId

                dgvRoutines.DataSource = _allRoutines.Select(r => new {
                    r.RoutineId,
                    r.Name,
                    r.ScheduledTime,
                    ExecutionDaysString = FormatExecutionDays(r.ExecutionDays), // Método helper para formatear
                    r.IsEnabled
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar rutinas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string FormatExecutionDays(DayOfWeek executionDays)
        {
            // Esto es un ejemplo simple, DayOfWeek es un enum con flags.
            // Necesitarías una lógica más robusta para convertirlo a un string legible (ej: "Lu, Ma, Vi")
            // o si guardas los días de otra forma (ej: string "1,2,5" o una tabla relacionada).
            // Por ahora, un placeholder:
            List<string> days = new List<string>();
            if ((executionDays & DayOfWeek.Monday) == DayOfWeek.Monday) days.Add("Lu");
            if ((executionDays & DayOfWeek.Tuesday) == DayOfWeek.Tuesday) days.Add("Ma");
            if ((executionDays & DayOfWeek.Wednesday) == DayOfWeek.Wednesday) days.Add("Mi");
            if ((executionDays & DayOfWeek.Thursday) == DayOfWeek.Thursday) days.Add("Ju");
            if ((executionDays & DayOfWeek.Friday) == DayOfWeek.Friday) days.Add("Vi");
            if ((executionDays & DayOfWeek.Saturday) == DayOfWeek.Saturday) days.Add("Sa");
            if ((executionDays & DayOfWeek.Sunday) == DayOfWeek.Sunday) days.Add("Do");
            return string.Join(", ", days);
        }


        private void btnAddRoutine_Click(object sender, EventArgs e)
        {
            // Abrir AddEditRoutineForm
            // var addRoutineForm = new AddEditRoutineForm(null, _routineService, _deviceService, _currentUser);
            // if (addRoutineForm.ShowDialog(this) == DialogResult.OK)
            // {
            //    LoadRoutines();
            // }
            MessageBox.Show("Funcionalidad Agregar Rutina pendiente.", "Información");
            LoadRoutines();
        }

        private void btnEditRoutine_Click(object sender, EventArgs e)
        {
            if (dgvRoutines.CurrentRow == null)
            {
                MessageBox.Show("Seleccione una rutina para editar.", "Información");
                return;
            }
            int routineId = (int)dgvRoutines.CurrentRow.Cells["RoutineId"].Value;
            var routineToEdit = _allRoutines.FirstOrDefault(r => r.RoutineId == routineId);

            if (routineToEdit != null)
            {
                // var editRoutineForm = new AddEditRoutineForm(routineToEdit, _routineService, _deviceService, _currentUser);
                // if (editRoutineForm.ShowDialog(this) == DialogResult.OK)
                // {
                //    LoadRoutines();
                // }
                MessageBox.Show($"Funcionalidad Editar Rutina ID: {routineId} pendiente.", "Información");
                LoadRoutines();
            }
        }

        private void btnDeleteRoutine_Click(object sender, EventArgs e)
        {
            if (dgvRoutines.CurrentRow == null)
            {
                MessageBox.Show("Seleccione una rutina para eliminar.", "Información");
                return;
            }
            int routineId = (int)dgvRoutines.CurrentRow.Cells["RoutineId"].Value;
            string routineName = dgvRoutines.CurrentRow.Cells["RoutineName"].Value.ToString();

            if (MessageBox.Show($"¿Está seguro de que desea eliminar la rutina '{routineName}'?", "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    _routineService.DeleteRoutine(routineId);
                    LoadRoutines();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar la rutina: {ex.Message}", "Error");
                }
            }
        }

        private void btnToggleRoutine_Click(object sender, EventArgs e)
        {
            if (dgvRoutines.CurrentRow == null) return;
            int routineId = (int)dgvRoutines.CurrentRow.Cells["RoutineId"].Value;
            var routine = _allRoutines.FirstOrDefault(r => r.RoutineId == routineId);
            if (routine != null)
            {
                try
                {
                    routine.IsEnabled = !routine.IsEnabled;
                    _routineService.UpdateRoutine(routine); // Asume que UpdateRoutine maneja el cambio de IsEnabled y (des)programa la tarea
                    LoadRoutines(); // Recargar para ver el cambio en el grid
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cambiar estado de la rutina: {ex.Message}", "Error");
                    routine.IsEnabled = !routine.IsEnabled; // Revertir el cambio en el objeto local si falla
                }
            }
        }

        private void btnRunRoutineNow_Click(object sender, EventArgs e)
        {
            if (dgvRoutines.CurrentRow == null) return;
            int routineId = (int)dgvRoutines.CurrentRow.Cells["RoutineId"].Value;
            try
            {
                _routineService.ExecuteRoutine(routineId); // Asume que esto llama a SchedulingService.TriggerRoutineNow
                MessageBox.Show("Ejecutando rutina...", "Información");
                // No es necesario recargar, ya que esto no cambia el estado persistente de la rutina en sí.
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al ejecutar la rutina: {ex.Message}", "Error");
            }
        }
    }
}
