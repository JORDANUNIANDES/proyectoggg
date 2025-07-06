using HomeAutomation.BusinessLogic.Services.Devices;
using HomeAutomation.BusinessLogic.Services.Zones;
using HomeAutomation.DataAccess.Entities; // Para Device, Zone, etc.
using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic; // Para List

namespace HomeAutomation.UI.Forms.Devices
{
    public partial class DeviceManagementForm : Form
    {
        private readonly IDeviceService _deviceService;
        private readonly IZoneService _zoneService; // Para poblar combobox de zonas y asignar
        private List<Device> _allDevices; // Cache local para evitar múltiples llamadas a BD

        public DeviceManagementForm(IDeviceService deviceService, IZoneService zoneService)
        {
            InitializeComponent();
            _deviceService = deviceService;
            _zoneService = zoneService;
        }

        private void DeviceManagementForm_Load(object sender, EventArgs e)
        {
            LoadZones();
            LoadDevices();
            SetupDataGridView();
        }

        private void SetupDataGridView()
        {
            dgvDevices.AutoGenerateColumns = false;
            dgvDevices.Columns.Clear();

            dgvDevices.Columns.Add(new DataGridViewTextBoxColumn { Name = "DeviceId", HeaderText = "ID", DataPropertyName = "DeviceId", Visible = false });
            dgvDevices.Columns.Add(new DataGridViewTextBoxColumn { Name = "DeviceName", HeaderText = "Nombre", DataPropertyName = "Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvDevices.Columns.Add(new DataGridViewTextBoxColumn { Name = "DeviceType", HeaderText = "Tipo", DataPropertyName = "Type" });
            dgvDevices.Columns.Add(new DataGridViewTextBoxColumn { Name = "DeviceLocation", HeaderText = "Ubicación", DataPropertyName = "Location" });
            dgvDevices.Columns.Add(new DataGridViewTextBoxColumn { Name = "DeviceStatus", HeaderText = "Estado", DataPropertyName = "Status" });
            dgvDevices.Columns.Add(new DataGridViewTextBoxColumn { Name = "ZoneName", HeaderText = "Zona", DataPropertyName = "ZoneName" }); // Necesitará un DTO o un mapeo

            // Para el estado, podrías querer un formato más amigable o un icono
        }


        private void LoadZones()
        {
            try
            {
                var zones = _zoneService.GetAllZones();
                zones.Insert(0, new Zone { ZoneId = 0, Name = "Todas las Zonas" }); // Opción para no filtrar

                cmbZoneFilter.DataSource = zones;
                cmbZoneFilter.DisplayMember = "Name";
                cmbZoneFilter.ValueMember = "ZoneId";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar zonas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDevices()
        {
            try
            {
                _allDevices = _deviceService.GetAllDevices(); // Obtener todos una vez
                FilterAndDisplayDevices();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar dispositivos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterAndDisplayDevices()
        {
            if (_allDevices == null) return;

            IEnumerable<Device> filteredDevices = _allDevices;

            if (cmbZoneFilter.SelectedValue != null && (int)cmbZoneFilter.SelectedValue != 0)
            {
                int selectedZoneId = (int)cmbZoneFilter.SelectedValue;
                filteredDevices = _allDevices.Where(d => d.ZoneId == selectedZoneId);
            }

            // Mapear a un DTO que incluya ZoneName si es necesario
            // Por simplicidad, aquí podríamos tener que hacer un join o una consulta adicional
            // O modificar DeviceService para que devuelva Device DTOs con ZoneName.
            // Aquí asumimos que Device tiene una propiedad Zone.Name o la obtenemos.
            dgvDevices.DataSource = filteredDevices.Select(d => new {
                d.DeviceId,
                d.Name,
                Type = d.Type.ToString(), // Enum a string
                d.Location,
                Status = d.Status.ToString(), // Enum a string
                ZoneName = d.Zone?.Name ?? "Sin asignar" // Asegurarse que Zone está cargada
            }).ToList();
        }


        private void cmbZoneFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterAndDisplayDevices();
        }

        private void btnAddDevice_Click(object sender, EventArgs e)
        {
            // Aquí se abriría un formulario AddEditDeviceForm
            // var addDeviceForm = new AddEditDeviceForm(null, _deviceService, _zoneService);
            // if (addDeviceForm.ShowDialog(this) == DialogResult.OK)
            // {
            //    LoadDevices(); // Recargar la lista
            // }
            MessageBox.Show("Funcionalidad Agregar Dispositivo pendiente.", "Información");
            LoadDevices(); // Simular recarga
        }

        private void btnEditDevice_Click(object sender, EventArgs e)
        {
            if (dgvDevices.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un dispositivo para editar.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int deviceId = (int)dgvDevices.CurrentRow.Cells["DeviceId"].Value;
            var deviceToEdit = _allDevices.FirstOrDefault(d => d.DeviceId == deviceId);

            if (deviceToEdit != null)
            {
                // var editDeviceForm = new AddEditDeviceForm(deviceToEdit, _deviceService, _zoneService);
                // if (editDeviceForm.ShowDialog(this) == DialogResult.OK)
                // {
                //    LoadDevices(); // Recargar la lista
                // }
                MessageBox.Show($"Funcionalidad Editar Dispositivo ID: {deviceId} pendiente.", "Información");
                LoadDevices(); // Simular recarga
            }
        }

        private void btnDeleteDevice_Click(object sender, EventArgs e)
        {
            if (dgvDevices.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un dispositivo para eliminar.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int deviceId = (int)dgvDevices.CurrentRow.Cells["DeviceId"].Value;
            string deviceName = dgvDevices.CurrentRow.Cells["DeviceName"].Value.ToString();

            if (MessageBox.Show($"¿Está seguro de que desea eliminar el dispositivo '{deviceName}'?", "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    _deviceService.DeleteDevice(deviceId);
                    LoadDevices(); // Recargar la lista
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar el dispositivo: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnToggleDevice_Click(object sender, EventArgs e)
        {
            if (dgvDevices.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un dispositivo.", "Información");
                return;
            }

            int deviceId = (int)dgvDevices.CurrentRow.Cells["DeviceId"].Value;
            var device = _allDevices.FirstOrDefault(d => d.DeviceId == deviceId);

            if (device != null)
            {
                try
                {
                    // Determinar el comando basado en el estado actual
                    string command = (device.Status == DeviceStatus.On) ? "TurnOff" : "TurnOn";
                    _deviceService.SendCommand(deviceId, command, null);
                    // Aquí deberíamos esperar una actualización de estado vía SignalR,
                    // o forzar una recarga si no tenemos SignalR completamente implementado en el cliente aún.
                    // Por ahora, simulamos una recarga para ver el cambio (aunque no sea en tiempo real).
                    // En una implementación completa, el estado se actualizaría automáticamente.
                    MessageBox.Show($"Comando '{command}' enviado al dispositivo ID: {deviceId}. El estado se actualizará.", "Comando Enviado");
                    // Podríamos forzar una actualización del dispositivo específico o recargar todos.
                    // LoadDevices(); // Esto podría ser muy pesado. Mejor actualizar el item específico.
                    // O esperar que SignalR actualice la UI.
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al enviar comando: {ex.Message}", "Error");
                }
            }
        }
    }
}
