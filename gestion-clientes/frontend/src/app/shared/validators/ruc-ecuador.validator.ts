import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function rucEcuadorValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;
    if (!value) {
      return null;
    }

    const ruc = String(value).trim();
    if (ruc.length !== 13 || !/^\d+$/.test(ruc)) {
      return { rucInvalido: 'El RUC debe tener exactamente 13 dígitos numéricos.' };
    }

    const provincia = parseInt(ruc.substring(0, 2), 10);
    if ((provincia < 1 || provincia > 24) && provincia !== 30) {
      return { rucInvalido: 'El código de provincia en el RUC no es válido.' };
    }

    const tercerDigito = parseInt(ruc.charAt(2), 10);

    if (tercerDigito < 6) {
      if (!validarCedula(ruc.substring(0, 10))) {
        return { rucInvalido: 'El número de cédula base en el RUC no es válido.' };
      }
      const establecimiento = parseInt(ruc.substring(10, 13), 10);
      if (establecimiento < 1) {
        return { rucInvalido: 'El código de establecimiento debe ser al menos 001.' };
      }
      return null;
    } else if (tercerDigito === 6) {
      const establecimiento = parseInt(ruc.substring(9, 13), 10);
      if (establecimiento < 1) {
        return { rucInvalido: 'El código de establecimiento debe ser al menos 0001.' };
      }
      const coeficientes = [3, 2, 7, 6, 5, 4, 3, 2];
      let suma = 0;
      for (let i = 0; i < coeficientes.length; i++) {
        suma += parseInt(ruc[i], 10) * coeficientes[i];
      }
      const residuo = suma % 11;
      const digitoVerificador = residuo === 0 ? 0 : 11 - residuo;
      if (digitoVerificador !== parseInt(ruc[8], 10)) {
        return { rucInvalido: 'El dígito verificador del RUC público es incorrecto.' };
      }
      return null;
    } else if (tercerDigito === 9) {
      const establecimiento = parseInt(ruc.substring(10, 13), 10);
      if (establecimiento < 1) {
        return { rucInvalido: 'El código de establecimiento debe ser al menos 001.' };
      }
      const coeficientes = [4, 3, 2, 7, 6, 5, 4, 3, 2];
      let suma = 0;
      for (let i = 0; i < coeficientes.length; i++) {
        suma += parseInt(ruc[i], 10) * coeficientes[i];
      }
      const residuo = suma % 11;
      const digitoVerificador = residuo === 0 ? 0 : 11 - residuo;
      if (digitoVerificador !== parseInt(ruc[9], 10)) {
        return { rucInvalido: 'El dígito verificador del RUC privado es incorrecto.' };
      }
      return null;
    }

    return { rucInvalido: 'El tipo de contribuyente en el RUC no es válido.' };
  };
}

function validarCedula(cedula: string): boolean {
  if (cedula.length !== 10 || !/^\d+$/.test(cedula)) {
    return false;
  }
  const provincia = parseInt(cedula.substring(0, 2), 10);
  if ((provincia < 1 || provincia > 24) && provincia !== 30) {
    return false;
  }
  const tercerDigito = parseInt(cedula.charAt(2), 10);
  if (tercerDigito >= 6) {
    return false;
  }

  const coeficientes = [2, 1, 2, 1, 2, 1, 2, 1, 2];
  let suma = 0;
  for (let i = 0; i < coeficientes.length; i++) {
    const valor = parseInt(cedula[i], 10) * coeficientes[i];
    suma += valor >= 10 ? valor - 9 : valor;
  }
  const residuo = suma % 10;
  const digitoVerificador = residuo === 0 ? 0 : 10 - residuo;
  return digitoVerificador === parseInt(cedula[9], 10);
}
