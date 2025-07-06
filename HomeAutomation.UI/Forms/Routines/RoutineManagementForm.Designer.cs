namespace HomeAutomation.UI.Forms.Routines
{
    partial class RoutineManagementForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dgvRoutines;
        private System.Windows.Forms.Button btnAddRoutine;
        private System.Windows.Forms.Button btnEditRoutine;
        private System.Windows.Forms.Button btnDeleteRoutine;
        private System.Windows.Forms.Button btnToggleRoutine;
        private System.Windows.Forms.Button btnRunRoutineNow;

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
            this.dgvRoutines = new System.Windows.Forms.DataGridView();
            this.btnAddRoutine = new System.Windows.Forms.Button();
            this.btnEditRoutine = new System.Windows.Forms.Button();
            this.btnDeleteRoutine = new System.Windows.Forms.Button();
            this.btnToggleRoutine = new System.Windows.Forms.Button();
            this.btnRunRoutineNow = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRoutines)).BeginInit();
            this.SuspendLayout();
            //
            // dgvRoutines
            //
            this.dgvRoutines.AllowUserToAddRows = false;
            this.dgvRoutines.AllowUserToDeleteRows = false;
            this.dgvRoutines.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvRoutines.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRoutines.Location = new System.Drawing.Point(12, 12);
            this.dgvRoutines.MultiSelect = false;
            this.dgvRoutines.Name = "dgvRoutines";
            this.dgvRoutines.ReadOnly = true;
            this.dgvRoutines.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRoutines.Size = new System.Drawing.Size(760, 398);
            this.dgvRoutines.TabIndex = 0;
            //
            // btnAddRoutine
            //
            this.btnAddRoutine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddRoutine.Location = new System.Drawing.Point(12, 416);
            this.btnAddRoutine.Name = "btnAddRoutine";
            this.btnAddRoutine.Size = new System.Drawing.Size(100, 23);
            this.btnAddRoutine.TabIndex = 1;
            this.btnAddRoutine.Text = "Agregar Rutina";
            this.btnAddRoutine.UseVisualStyleBackColor = true;
            this.btnAddRoutine.Click += new System.EventHandler(this.btnAddRoutine_Click);
            //
            // btnEditRoutine
            //
            this.btnEditRoutine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnEditRoutine.Location = new System.Drawing.Point(118, 416);
            this.btnEditRoutine.Name = "btnEditRoutine";
            this.btnEditRoutine.Size = new System.Drawing.Size(100, 23);
            this.btnEditRoutine.TabIndex = 2;
            this.btnEditRoutine.Text = "Editar Rutina";
            this.btnEditRoutine.UseVisualStyleBackColor = true;
            this.btnEditRoutine.Click += new System.EventHandler(this.btnEditRoutine_Click);
            //
            // btnDeleteRoutine
            //
            this.btnDeleteRoutine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDeleteRoutine.Location = new System.Drawing.Point(224, 416);
            this.btnDeleteRoutine.Name = "btnDeleteRoutine";
            this.btnDeleteRoutine.Size = new System.Drawing.Size(100, 23);
            this.btnDeleteRoutine.TabIndex = 3;
            this.btnDeleteRoutine.Text = "Eliminar Rutina";
            this.btnDeleteRoutine.UseVisualStyleBackColor = true;
            this.btnDeleteRoutine.Click += new System.EventHandler(this.btnDeleteRoutine_Click);
            //
            // btnToggleRoutine
            //
            this.btnToggleRoutine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnToggleRoutine.Location = new System.Drawing.Point(530, 416);
            this.btnToggleRoutine.Name = "btnToggleRoutine";
            this.btnToggleRoutine.Size = new System.Drawing.Size(120, 23);
            this.btnToggleRoutine.TabIndex = 4;
            this.btnToggleRoutine.Text = "Habilitar/Deshabilitar";
            this.btnToggleRoutine.UseVisualStyleBackColor = true;
            this.btnToggleRoutine.Click += new System.EventHandler(this.btnToggleRoutine_Click);
            //
            // btnRunRoutineNow
            //
            this.btnRunRoutineNow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRunRoutineNow.Location = new System.Drawing.Point(656, 416);
            this.btnRunRoutineNow.Name = "btnRunRoutineNow";
            this.btnRunRoutineNow.Size = new System.Drawing.Size(116, 23);
            this.btnRunRoutineNow.TabIndex = 5;
            this.btnRunRoutineNow.Text = "Ejecutar Ahora";
            this.btnRunRoutineNow.UseVisualStyleBackColor = true;
            this.btnRunRoutineNow.Click += new System.EventHandler(this.btnRunRoutineNow_Click);
            //
            // RoutineManagementForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 451);
            this.Controls.Add(this.btnRunRoutineNow);
            this.Controls.Add(this.btnToggleRoutine);
            this.Controls.Add(this.btnDeleteRoutine);
            this.Controls.Add(this.btnEditRoutine);
            this.Controls.Add(this.btnAddRoutine);
            this.Controls.Add(this.dgvRoutines);
            this.Name = "RoutineManagementForm";
            this.Text = "Gestión de Rutinas";
            this.Load += new System.EventHandler(this.RoutineManagementForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRoutines)).EndInit();
            this.ResumeLayout(false);

        }
    }
}
