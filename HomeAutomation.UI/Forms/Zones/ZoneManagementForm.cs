using HomeAutomation.BusinessLogic.Services.Zones;
using HomeAutomation.DataAccess.Entities; // Para Zone
using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

namespace HomeAutomation.UI.Forms.Zones
{
    public partial class ZoneManagementForm : Form
    {
        private readonly IZoneService _zoneService;
        private List<Zone> _allZones;

        public ZoneManagementForm(IZoneService zoneService)
        {
            InitializeComponent();
            _zoneService = zoneService;
        }

        private void ZoneManagementForm_Load(object sender, EventArgs e)
        {
            LoadZones();
            SetupDataGridView();
        }

        private void SetupDataGridView()
        {
            dgvZones.AutoGenerateColumns = false;
            dgvZones.Columns.Clear();

            dgvZones.Columns.Add(new DataGridViewTextBoxColumn { Name = "ZoneId", HeaderText = "ID", DataPropertyName = "ZoneId", Visible = false });
            dgvZones.Columns.Add(new DataGridViewTextBoxColumn { Name = "ZoneName", HeaderText = "Nombre", DataPropertyName = "Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvZones.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Descripción", DataPropertyName = "Description", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        }


        private void LoadZones()
        {
            try
            {
                _allZones = _zoneService.GetAllZones();
                dgvZones.DataSource = _allZones.Select(z => new {
                    z.ZoneId,
                    z.Name,
                    z.Description
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar zonas: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAddZone_Click(object sender, EventArgs e)
        {
            // Abrir un AddEditZoneForm
            // var addZoneForm = new AddEditZoneForm(null, _zoneService);
            // if (addZoneForm.ShowDialog(this) == DialogResult.OK)
            // {
            //    LoadZones();
            // }
            MessageBox.Show("Funcionalidad Agregar Zona pendiente.", "Información");
            LoadZones();
        }

        private void btnEditZone_Click(object sender, EventArgs e)
        {
            if (dgvZones.CurrentRow == null)
            {
                MessageBox.Show("Seleccione una zona para editar.", "Información");
                return;
            }
            int zoneId = (int)dgvZones.CurrentRow.Cells["ZoneId"].Value;
            var zoneToEdit = _allZones.FirstOrDefault(z => z.ZoneId == zoneId);

            if (zoneToEdit != null)
            {
                // var editZoneForm = new AddEditZoneForm(zoneToEdit, _zoneService);
                // if (editZoneForm.ShowDialog(this) == DialogResult.OK)
                // {
                //    LoadZones();
                // }
                MessageBox.Show($"Funcionalidad Editar Zona ID: {zoneId} pendiente.", "Información");
                LoadZones();
            }
        }

        private void btnDeleteZone_Click(object sender, EventArgs e)
        {
            if (dgvZones.CurrentRow == null)
            {
                MessageBox.Show("Seleccione una zona para eliminar.", "Información");
                return;
            }

            int zoneId = (int)dgvZones.CurrentRow.Cells["ZoneId"].Value;
            string zoneName = dgvZones.CurrentRow.Cells["ZoneName"].Value.ToString();

            // ADVERTENCIA: Eliminar una zona podría requerir lógica adicional
            // (ej. qué pasa con los dispositivos en esa zona? Desasignarlos? Impedir eliminación si tiene dispositivos?)
            // Esta lógica debería estar en ZoneService.
            if (MessageBox.Show($"¿Está seguro de que desea eliminar la zona '{zoneName}'? Esto podría afectar a los dispositivos asignados.", "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    _zoneService.DeleteZone(zoneId);
                    LoadZones();
                }
                catch (Exception ex) // Capturar excepciones específicas (ej: si la zona tiene dispositivos y no se puede eliminar)
                {
                    MessageBox.Show($"Error al eliminar la zona: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
