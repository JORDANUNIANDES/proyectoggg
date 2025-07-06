using HomeAutomation.BusinessLogic.Services.Auth; // Asumiendo esta ubicación para IAuthService
using HomeAutomation.DataAccess.Entities; // Para la entidad User
using System;
using System.Windows.Forms;

namespace HomeAutomation.UI.Forms.Auth
{
    public partial class LoginForm : Form
    {
        private readonly IAuthService _authService; // Se inyectará o instanciará

        public User AuthenticatedUser { get; private set; }

        public LoginForm(IAuthService authService)
        {
            InitializeComponent();
            _authService = authService;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                lblErrorMessage.Text = "Usuario y contraseña son requeridos.";
                lblErrorMessage.Visible = true;
                return;
            }

            try
            {
                AuthenticatedUser = _authService.Authenticate(username, password);
                if (AuthenticatedUser != null)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    lblErrorMessage.Text = "Credenciales inválidas.";
                    lblErrorMessage.Visible = true;
                    AuthenticatedUser = null;
                }
            }
            catch (Exception ex)
            {
                // Idealmente, loguear el error ex
                lblErrorMessage.Text = "Error durante el inicio de sesión.";
                lblErrorMessage.Visible = true;
                AuthenticatedUser = null;
            }
        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {
            lblErrorMessage.Visible = false;
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            lblErrorMessage.Visible = false;
        }
    }
}
