namespace HomeAutomation.UI.Forms.Zones
{
    partial class ZoneManagementForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dgvZones;
        private System.Windows.Forms.Button btnAddZone;
        private System.Windows.Forms.Button btnEditZone;
        private System.Windows.Forms.Button btnDeleteZone;

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
            this.dgvZones = new System.Windows.Forms.DataGridView();
            this.btnAddZone = new System.Windows.Forms.Button();
            this.btnEditZone = new System.Windows.Forms.Button();
            this.btnDeleteZone = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvZones)).BeginInit();
            this.SuspendLayout();
            //
            // dgvZones
            //
            this.dgvZones.AllowUserToAddRows = false;
            this.dgvZones.AllowUserToDeleteRows = false;
            this.dgvZones.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvZones.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvZones.Location = new System.Drawing.Point(12, 12);
            this.dgvZones.MultiSelect = false;
            this.dgvZones.Name = "dgvZones";
            this.dgvZones.ReadOnly = true;
            this.dgvZones.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvZones.Size = new System.Drawing.Size(760, 398);
            this.dgvZones.TabIndex = 0;
            //
            // btnAddZone
            //
            this.btnAddZone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddZone.Location = new System.Drawing.Point(12, 416);
            this.btnAddZone.Name = "btnAddZone";
            this.btnAddZone.Size = new System.Drawing.Size(100, 23);
            this.btnAddZone.TabIndex = 1;
            this.btnAddZone.Text = "Agregar Zona";
            this.btnAddZone.UseVisualStyleBackColor = true;
            this.btnAddZone.Click += new System.EventHandler(this.btnAddZone_Click);
            //
            // btnEditZone
            //
            this.btnEditZone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnEditZone.Location = new System.Drawing.Point(118, 416);
            this.btnEditZone.Name = "btnEditZone";
            this.btnEditZone.Size = new System.Drawing.Size(100, 23);
            this.btnEditZone.TabIndex = 2;
            this.btnEditZone.Text = "Editar Zona";
            this.btnEditZone.UseVisualStyleBackColor = true;
            this.btnEditZone.Click += new System.EventHandler(this.btnEditZone_Click);
            //
            // btnDeleteZone
            //
            this.btnDeleteZone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDeleteZone.Location = new System.Drawing.Point(224, 416);
            this.btnDeleteZone.Name = "btnDeleteZone";
            this.btnDeleteZone.Size = new System.Drawing.Size(100, 23);
            this.btnDeleteZone.TabIndex = 3;
            this.btnDeleteZone.Text = "Eliminar Zona";
            this.btnDeleteZone.UseVisualStyleBackColor = true;
            this.btnDeleteZone.Click += new System.EventHandler(this.btnDeleteZone_Click);
            //
            // ZoneManagementForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 451);
            this.Controls.Add(this.btnDeleteZone);
            this.Controls.Add(this.btnEditZone);
            this.Controls.Add(this.btnAddZone);
            this.Controls.Add(this.dgvZones);
            this.Name = "ZoneManagementForm";
            this.Text = "Gestión de Zonas";
            this.Load += new System.EventHandler(this.ZoneManagementForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvZones)).EndInit();
            this.ResumeLayout(false);

        }
    }
}
