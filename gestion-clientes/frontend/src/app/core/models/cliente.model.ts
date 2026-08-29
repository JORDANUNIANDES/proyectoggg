export interface Cliente {
  id: number;
  nombres: string;
  apellidos: string;
  ruc: string;
  email: string;
  telefono: string;
  direccion: string;
  fechaRegistro: string;
  activo: boolean;
}

export interface CrearCliente {
  nombres: string;
  apellidos: string;
  ruc: string;
  email: string;
  telefono: string;
  direccion: string;
}

export interface ActualizarCliente {
  nombres: string;
  apellidos: string;
  ruc: string;
  email: string;
  telefono: string;
  direccion: string;
}

export interface DashboardSummary {
  totalClientes: number;
  clientesActivos: number;
  clientesInactivos: number;
  clientesRecientes: number;
  ultimosClientes: Cliente[];
}
