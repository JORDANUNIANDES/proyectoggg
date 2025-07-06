using HomeAutomation.BusinessLogic.Services.Devices; // Asumiendo esta ubicación
using HomeAutomation.BusinessLogic.Services.Zones;   // Asumiendo esta ubicación
using HomeAutomation.BusinessLogic.Services.Routines; // Asumiendo esta ubicación
using HomeAutomation.DataAccess.Entities;
using HomeAutomation.UI.Forms.Devices;
using HomeAutomation.UI.Forms.Routines;
using HomeAutomation.UI.Forms.Zones;
using System;
using System.Windows.Forms;

namespace HomeAutomation.UI.Forms.Dashboard
{
    public partial class DashboardForm : Form
    {
        private readonly User _currentUser;
        private readonly IDeviceService _deviceService;
        private readonly IZoneService _zoneService;
        private readonly IRoutineService _routineService;
        // Otros servicios que se necesiten para la búsqueda global

        public DashboardForm(User currentUser, IDeviceService deviceService, IZoneService zoneService, IRoutineService routineService)
        {
            InitializeComponent();
            _currentUser = currentUser;
            _deviceService = deviceService;
            _zoneService = zoneService;
            _routineService = routineService;

            lblWelcomeUser.Text = $"Bienvenido, {_currentUser.Username}!";
            // Aquí podrías cargar dispositivos favoritos, etc.
        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void cerrarSesiónToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Retry; // Usar un DialogResult específico para indicar logout
            this.Close();
        }

        private void dispositivosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Asumiendo que DeviceManagementForm toma IDeviceService y IZoneService
            var deviceManagementForm = new DeviceManagementForm(_deviceService, _zoneService);
            deviceManagementForm.ShowDialog(this);
        }

        private void zonasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var zoneManagementForm = new ZoneManagementForm(_zoneService);
            zoneManagementForm.ShowDialog(this);
        }

        private void rutinasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Asumiendo que RoutineManagementForm toma IRoutineService e IDeviceService
            var routineManagementForm = new RoutineManagementForm(_routineService, _deviceService, _currentUser);
            routineManagementForm.ShowDialog(this);
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = txtGlobalSearch.Text;
            if (string.IsNullOrWhiteSpace(searchTerm)) return;

            // Implementar lógica de búsqueda global
            // Esto implicaría llamar a métodos en _deviceService, _zoneService,
            // y potencialmente un _userService (si los admins pueden buscar usuarios)
            // y luego mostrar los resultados (quizás en un nuevo formulario o un panel).
            MessageBox.Show($"Buscando: {searchTerm}... (Implementación pendiente)", "Búsqueda Global");
        }
    }
}
