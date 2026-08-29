namespace GestionClientes.Application.Validators;

public static class RucEcuadorValidator
{
    public static bool EsRucValido(string? ruc)
    {
        if (string.IsNullOrWhiteSpace(ruc))
            return false;

        ruc = ruc.Trim();

        if (ruc.Length != 13 || !ruc.All(char.IsDigit))
            return false;

        int provincia = int.Parse(ruc.Substring(0, 2));
        if ((provincia < 1 || provincia > 24) && provincia != 30)
            return false;

        int tercerDigito = int.Parse(ruc.Substring(2, 1));

        if (tercerDigito < 6)
        {
            // Persona Natural
            if (!ValidarCedula(ruc.Substring(0, 10)))
                return false;

            string establecimiento = ruc.Substring(10, 3);
            return int.Parse(establecimiento) >= 1;
        }
        else if (tercerDigito == 6)
        {
            // Sociedad Pública
            string establecimiento = ruc.Substring(9, 4);
            if (int.Parse(establecimiento) < 1)
                return false;

            int[] coeficientes = { 3, 2, 7, 6, 5, 4, 3, 2 };
            int suma = 0;
            for (int i = 0; i < coeficientes.Length; i++)
            {
                suma += int.Parse(ruc[i].ToString()) * coeficientes[i];
            }

            int residuo = suma % 11;
            int digitoVerificador = residuo == 0 ? 0 : 11 - residuo;

            return digitoVerificador == int.Parse(ruc[8].ToString());
        }
        else if (tercerDigito == 9)
        {
            // Sociedad Privada / Extranjera
            string establecimiento = ruc.Substring(10, 3);
            if (int.Parse(establecimiento) < 1)
                return false;

            int[] coeficientes = { 4, 3, 2, 7, 6, 5, 4, 3, 2 };
            int suma = 0;
            for (int i = 0; i < coeficientes.Length; i++)
            {
                suma += int.Parse(ruc[i].ToString()) * coeficientes[i];
            }

            int residuo = suma % 11;
            int digitoVerificador = residuo == 0 ? 0 : 11 - residuo;

            return digitoVerificador == int.Parse(ruc[9].ToString());
        }

        return false;
    }

    public static bool ValidarCedula(string cedula)
    {
        if (cedula.Length != 10 || !cedula.All(char.IsDigit))
            return false;

        int provincia = int.Parse(cedula.Substring(0, 2));
        if ((provincia < 1 || provincia > 24) && provincia != 30)
            return false;

        int tercerDigito = int.Parse(cedula.Substring(2, 1));
        if (tercerDigito >= 6)
            return false;

        int[] coeficientes = { 2, 1, 2, 1, 2, 1, 2, 1, 2 };
        int suma = 0;

        for (int i = 0; i < coeficientes.Length; i++)
        {
            int valor = int.Parse(cedula[i].ToString()) * coeficientes[i];
            suma += valor >= 10 ? valor - 9 : valor;
        }

        int residuo = suma % 10;
        int digitoVerificador = residuo == 0 ? 0 : 10 - residuo;

        return digitoVerificador == int.Parse(cedula[9].ToString());
    }
}
