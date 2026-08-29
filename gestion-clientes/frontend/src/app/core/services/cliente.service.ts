import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Cliente, CrearCliente, ActualizarCliente, DashboardSummary } from '../models/cliente.model';

@Injectable({
  providedIn: 'root'
})
export class ClienteService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5000/api/clientes';
  private dashboardUrl = 'http://localhost:5000/api/dashboard';

  obtenerClientes(busqueda?: string, activo?: boolean): Observable<Cliente[]> {
    let params = new HttpParams();
    if (busqueda && busqueda.trim()) {
      params = params.set('busqueda', busqueda.trim());
    }
    if (activo !== undefined && activo !== null) {
      params = params.set('activo', activo.toString());
    }
    return this.http.get<Cliente[]>(this.apiUrl, { params });
  }

  obtenerPorId(id: number): Observable<Cliente> {
    return this.http.get<Cliente>(`${this.apiUrl}/${id}`);
  }

  crear(cliente: CrearCliente): Observable<Cliente> {
    return this.http.post<Cliente>(this.apiUrl, cliente);
  }

  actualizar(id: number, cliente: ActualizarCliente): Observable<Cliente> {
    return this.http.put<Cliente>(`${this.apiUrl}/${id}`, cliente);
  }

  desactivar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  activar(id: number): Observable<{ message: string }> {
    return this.http.patch<{ message: string }>(`${this.apiUrl}/${id}/activar`, {});
  }

  obtenerDashboard(): Observable<DashboardSummary> {
    return this.http.get<DashboardSummary>(this.dashboardUrl);
  }
}
