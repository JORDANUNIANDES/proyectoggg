namespace HomeAutomation.UI.Forms.Devices
{
    partial class DeviceManagementForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dgvDevices;
        private System.Windows.Forms.Button btnAddDevice;
        private System.Windows.Forms.Button btnEditDevice;
        private System.Windows.Forms.Button btnDeleteDevice;
        private System.Windows.Forms.Button btnToggleDevice;
        private System.Windows.Forms.ComboBox cmbZoneFilter;
        private System.Windows.Forms.Label lblZoneFilter;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.dgvDevices = new System.Windows.Forms.DataGridView();
            this.btnAddDevice = new System.Windows.Forms.Button();
            this.btnEditDevice = new System.Windows.Forms.Button();
            this.btnDeleteDevice = new System.Windows.Forms.Button();
            this.btnToggleDevice = new System.Windows.Forms.Button();
            this.cmbZoneFilter = new System.Windows.Forms.ComboBox();
            this.lblZoneFilter = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDevices)).BeginInit();
            this.SuspendLayout();
            //
            // dgvDevices
            //
            this.dgvDevices.AllowUserToAddRows = false;
            this.dgvDevices.AllowUserToDeleteRows = false;
            this.dgvDevices.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvDevices.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDevices.Location = new System.Drawing.Point(12, 41);
            this.dgvDevices.MultiSelect = false;
            this.dgvDevices.Name = "dgvDevices";
            this.dgvDevices.ReadOnly = true;
            this.dgvDevices.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDevices.Size = new System.Drawing.Size(760, 378);
            this.dgvDevices.TabIndex = 0;
            //
            // btnAddDevice
            //
            this.btnAddDevice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddDevice.Location = new System.Drawing.Point(12, 425);
            this.btnAddDevice.Name = "btnAddDevice";
            this.btnAddDevice.Size = new System.Drawing.Size(120, 23);
            this.btnAddDevice.TabIndex = 1;
            this.btnAddDevice.Text = "Agregar Dispositivo";
            this.btnAddDevice.UseVisualStyleBackColor = true;
            this.btnAddDevice.Click += new System.EventHandler(this.btnAddDevice_Click);
            //
            // btnEditDevice
            //
            this.btnEditDevice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnEditDevice.Location = new System.Drawing.Point(138, 425);
            this.btnEditDevice.Name = "btnEditDevice";
            this.btnEditDevice.Size = new System.Drawing.Size(120, 23);
            this.btnEditDevice.TabIndex = 2;
            this.btnEditDevice.Text = "Editar Dispositivo";
            this.btnEditDevice.UseVisualStyleBackColor = true;
            this.btnEditDevice.Click += new System.EventHandler(this.btnEditDevice_Click);
            //
            // btnDeleteDevice
            //
            this.btnDeleteDevice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDeleteDevice.Location = new System.Drawing.Point(264, 425);
            this.btnDeleteDevice.Name = "btnDeleteDevice";
            this.btnDeleteDevice.Size = new System.Drawing.Size(120, 23);
            this.btnDeleteDevice.TabIndex = 3;
            this.btnDeleteDevice.Text = "Eliminar Dispositivo";
            this.btnDeleteDevice.UseVisualStyleBackColor = true;
            this.btnDeleteDevice.Click += new System.EventHandler(this.btnDeleteDevice_Click);
            //
            // btnToggleDevice
            //
            this.btnToggleDevice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnToggleDevice.Location = new System.Drawing.Point(652, 425);
            this.btnToggleDevice.Name = "btnToggleDevice";
            this.btnToggleDevice.Size = new System.Drawing.Size(120, 23);
            this.btnToggleDevice.TabIndex = 4;
            this.btnToggleDevice.Text = "Encender/Apagar";
            this.btnToggleDevice.UseVisualStyleBackColor = true;
            this.btnToggleDevice.Click += new System.EventHandler(this.btnToggleDevice_Click);
            //
            // cmbZoneFilter
            //
            this.cmbZoneFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbZoneFilter.FormattingEnabled = true;
            this.cmbZoneFilter.Location = new System.Drawing.Point(80, 12);
            this.cmbZoneFilter.Name = "cmbZoneFilter";
            this.cmbZoneFilter.Size = new System.Drawing.Size(200, 21);
            this.cmbZoneFilter.TabIndex = 5;
            this.cmbZoneFilter.SelectedIndexChanged += new System.EventHandler(this.cmbZoneFilter_SelectedIndexChanged);
            //
            // lblZoneFilter
            //
            this.lblZoneFilter.AutoSize = true;
            this.lblZoneFilter.Location = new System.Drawing.Point(12, 15);
            this.lblZoneFilter.Name = "lblZoneFilter";
            this.lblZoneFilter.Size = new System.Drawing.Size(62, 13);
            this.lblZoneFilter.TabIndex = 6;
            this.lblZoneFilter.Text = "Filtrar Zona:";
            //
            // DeviceManagementForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 461);
            this.Controls.Add(this.lblZoneFilter);
            this.Controls.Add(this.cmbZoneFilter);
            this.Controls.Add(this.btnToggleDevice);
            this.Controls.Add(this.btnDeleteDevice);
            this.Controls.Add(this.btnEditDevice);
            this.Controls.Add(this.btnAddDevice);
            this.Controls.Add(this.dgvDevices);
            this.Name = "DeviceManagementForm";
            this.Text = "Gestión de Dispositivos";
            this.Load += new System.EventHandler(this.DeviceManagementForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDevices)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
