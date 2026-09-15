using System;
using System.Text.RegularExpressions;

namespace RestaurantManagement.API.Helpers
{
    public static class EcuadorianValidator
    {
        public static bool ValidarCedula(string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula))
                return false;

            cedula = cedula.Trim();

            if (cedula.Length != 10 || !Regex.IsMatch(cedula, @"^\d{10}$"))
                return false;

            int provincia = int.Parse(cedula.Substring(0, 2));
            if (provincia < 1 || (provincia > 24 && provincia != 30))
                return false;

            int tercerDigito = int.Parse(cedula.Substring(2, 1));
            if (tercerDigito >= 6)
                return false;

            int[] coeficientes = { 2, 1, 2, 1, 2, 1, 2, 1, 2 };
            int suma = 0;

            for (int i = 0; i < 9; i++)
            {
                int digito = int.Parse(cedula[i].ToString());
                int producto = digito * coeficientes[i];
                if (producto >= 10)
                    producto -= 9;

                suma += producto;
            }

            int residuo = suma % 10;
            int digitoVerificadorCalculado = residuo == 0 ? 0 : 10 - residuo;
            int digitoVerificadorReal = int.Parse(cedula[9].ToString());

            return digitoVerificadorCalculado == digitoVerificadorReal;
        }
    }
}
