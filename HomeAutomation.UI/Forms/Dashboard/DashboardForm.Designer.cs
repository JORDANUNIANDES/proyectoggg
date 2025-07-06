namespace HomeAutomation.UI.Forms.Dashboard
{
    partial class DashboardForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem archivoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem salirToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gestionarToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dispositivosToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem zonasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rutinasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem usuarioToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cerrarSesiónToolStripMenuItem;
        private System.Windows.Forms.Label lblWelcomeUser;
        private System.Windows.Forms.TextBox txtGlobalSearch;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Panel pnlFavoriteDevices; // Placeholder

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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.archivoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.salirToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gestionarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dispositivosToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zonasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rutinasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.usuarioToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cerrarSesiónToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lblWelcomeUser = new System.Windows.Forms.Label();
            this.txtGlobalSearch = new System.Windows.Forms.TextBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.pnlFavoriteDevices = new System.Windows.Forms.Panel();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            //
            // menuStrip1
            //
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.archivoToolStripMenuItem,
            this.gestionarToolStripMenuItem,
            this.usuarioToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(784, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            //
            // archivoToolStripMenuItem
            //
            this.archivoToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.salirToolStripMenuItem});
            this.archivoToolStripMenuItem.Name = "archivoToolStripMenuItem";
            this.archivoToolStripMenuItem.Size = new System.Drawing.Size(60, 20);
            this.archivoToolStripMenuItem.Text = "&Archivo";
            //
            // salirToolStripMenuItem
            //
            this.salirToolStripMenuItem.Name = "salirToolStripMenuItem";
            this.salirToolStripMenuItem.Size = new System.Drawing.Size(96, 22);
            this.salirToolStripMenuItem.Text = "&Salir";
            this.salirToolStripMenuItem.Click += new System.EventHandler(this.salirToolStripMenuItem_Click);
            //
            // gestionarToolStripMenuItem
            //
            this.gestionarToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dispositivosToolStripMenuItem,
            this.zonasToolStripMenuItem,
            this.rutinasToolStripMenuItem});
            this.gestionarToolStripMenuItem.Name = "gestionarToolStripMenuItem";
            this.gestionarToolStripMenuItem.Size = new System.Drawing.Size(69, 20);
            this.gestionarToolStripMenuItem.Text = "&Gestionar";
            //
            // dispositivosToolStripMenuItem
            //
            this.dispositivosToolStripMenuItem.Name = "dispositivosToolStripMenuItem";
            this.dispositivosToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.dispositivosToolStripMenuItem.Text = "&Dispositivos";
            this.dispositivosToolStripMenuItem.Click += new System.EventHandler(this.dispositivosToolStripMenuItem_Click);
            //
            // zonasToolStripMenuItem
            //
            this.zonasToolStripMenuItem.Name = "zonasToolStripMenuItem";
            this.zonasToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.zonasToolStripMenuItem.Text = "&Zonas";
            this.zonasToolStripMenuItem.Click += new System.EventHandler(this.zonasToolStripMenuItem_Click);
            //
            // rutinasToolStripMenuItem
            //
            this.rutinasToolStripMenuItem.Name = "rutinasToolStripMenuItem";
            this.rutinasToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.rutinasToolStripMenuItem.Text = "&Rutinas";
            this.rutinasToolStripMenuItem.Click += new System.EventHandler(this.rutinasToolStripMenuItem_Click);
            //
            // usuarioToolStripMenuItem
            //
            this.usuarioToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cerrarSesiónToolStripMenuItem});
            this.usuarioToolStripMenuItem.Name = "usuarioToolStripMenuItem";
            this.usuarioToolStripMenuItem.Size = new System.Drawing.Size(59, 20);
            this.usuarioToolStripMenuItem.Text = "&Usuario";
            //
            // cerrarSesiónToolStripMenuItem
            //
            this.cerrarSesiónToolStripMenuItem.Name = "cerrarSesiónToolStripMenuItem";
            this.cerrarSesiónToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.cerrarSesiónToolStripMenuItem.Text = "&Cerrar Sesión";
            this.cerrarSesiónToolStripMenuItem.Click += new System.EventHandler(this.cerrarSesiónToolStripMenuItem_Click);
            //
            // lblWelcomeUser
            //
            this.lblWelcomeUser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblWelcomeUser.AutoSize = true;
            this.lblWelcomeUser.Location = new System.Drawing.Point(580, 30);
            this.lblWelcomeUser.Name = "lblWelcomeUser";
            this.lblWelcomeUser.Size = new System.Drawing.Size(71, 13);
            this.lblWelcomeUser.TabIndex = 1;
            this.lblWelcomeUser.Text = "Bienvenido, ...";
            this.lblWelcomeUser.TextAlign = System.Drawing.ContentAlignment.TopRight;
            //
            // txtGlobalSearch
            //
            this.txtGlobalSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGlobalSearch.Location = new System.Drawing.Point(15, 30);
            this.txtGlobalSearch.Name = "txtGlobalSearch";
            this.txtGlobalSearch.Size = new System.Drawing.Size(450, 20);
            this.txtGlobalSearch.TabIndex = 2;
            //
            // btnSearch
            //
            this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearch.Location = new System.Drawing.Point(470, 28);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(75, 23);
            this.btnSearch.TabIndex = 3;
            this.btnSearch.Text = "Buscar";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            //
            // pnlFavoriteDevices
            //
            this.pnlFavoriteDevices.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlFavoriteDevices.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlFavoriteDevices.Location = new System.Drawing.Point(15, 60);
            this.pnlFavoriteDevices.Name = "pnlFavoriteDevices";
            this.pnlFavoriteDevices.Size = new System.Drawing.Size(750, 480);
            this.pnlFavoriteDevices.TabIndex = 4;
            //
            // DashboardForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.pnlFavoriteDevices);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.txtGlobalSearch);
            this.Controls.Add(this.lblWelcomeUser);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "DashboardForm";
            this.Text = "Plataforma Domótica - Dashboard";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
