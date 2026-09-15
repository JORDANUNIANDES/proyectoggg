using System.Linq;

namespace RestaurantManagement.API.Services
{
    public static class EcuadorianValidator
    {
        public static bool ValidarCedula(string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula) || cedula.Length != 10 || !cedula.All(char.IsDigit))
            {
                return false;
            }

            int provincia = int.Parse(cedula.Substring(0, 2));
            if (provincia < 1 || (provincia > 24 && provincia != 30))
            {
                return false;
            }

            int tercerDigito = int.Parse(cedula.Substring(2, 1));
            if (tercerDigito >= 6)
            {
                return false; // Para persona natural la cedula debe tener el 3er digito menor a 6
            }

            int[] coeficientes = { 2, 1, 2, 1, 2, 1, 2, 1, 2 };
            int suma = 0;

            for (int i = 0; i < 9; i++)
            {
                int valor = int.Parse(cedula[i].ToString()) * coeficientes[i];
                if (valor >= 10)
                {
                    valor -= 9;
                }
                suma += valor;
            }

            int digitoVerificador = int.Parse(cedula[9].ToString());
            int decenaSuperior = ((suma + 9) / 10) * 10;
            int calculado = decenaSuperior - suma;
            if (calculado == 10)
            {
                calculado = 0;
            }

            return calculado == digitoVerificador;
        }
    }
}
