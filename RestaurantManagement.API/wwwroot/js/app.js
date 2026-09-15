// Restaurant Management Frontend JavaScript (Vanilla JS)

const API_BASE = '/api';

// State Variables
let currentView = 'dashboard';
let cartItems = []; // [{ platoId, nombre, precio, cantidad }]
let availableDishesList = [];

// Initialize Application
document.addEventListener('DOMContentLoaded', () => {
  setupNavigation();
  setupFilterListeners();
  loadDashboard();
});

// Navigation Setup
function setupNavigation() {
  const navItems = document.querySelectorAll('.nav-item');
  navItems.forEach(item => {
    item.addEventListener('click', () => {
      const view = item.getAttribute('data-view');
      switchView(view);
    });
  });
}

function switchView(viewName) {
  currentView = viewName;

  // Update sidebar active class
  document.querySelectorAll('.nav-menu .nav-item').forEach(el => {
    if (el.getAttribute('data-view') === viewName) {
      el.classList.add('active');
    } else {
      el.classList.remove('active');
    }
  });

  // Update Page Title
  const titles = {
    dashboard: 'Dashboard Principal',
    clientes: 'Gestión de Clientes',
    platos: 'Menú y Platos',
    mesas: 'Gestión de Mesas',
    pedidos: 'Gestión de Pedidos',
    reportes: 'Reportes por Cliente'
  };
  document.getElementById('page-title').textContent = titles[viewName] || 'Restaurante';

  // Toggle visible section
  document.querySelectorAll('.view-section').forEach(sec => sec.classList.remove('active'));
  const activeSection = document.getElementById(`view-${viewName}`);
  if (activeSection) activeSection.classList.add('active');

  // Trigger view specific load
  switch (viewName) {
    case 'dashboard':
      loadDashboard();
      break;
    case 'clientes':
      loadClientes();
      break;
    case 'platos':
      loadPlatos();
      break;
    case 'mesas':
      loadMesas();
      break;
    case 'pedidos':
      loadPedidos();
      break;
    case 'reportes':
      initReportesView();
      break;
  }
}

// Notification Alert System
function showAlert(message, type = 'danger', duration = 6000) {
  const alertBox = document.getElementById('alert-box');
  alertBox.className = `alert alert-${type}`;
  alertBox.textContent = message;
  alertBox.style.display = 'block';

  setTimeout(() => {
    alertBox.style.display = 'none';
  }, duration);
}

// Validation Helper: Ecuadorian Cédula Modulo 10 Algorithm
function validarCedulaEcuatoriana(cedula) {
  if (!cedula || typeof cedula !== 'string') return false;
  cedula = cedula.trim();
  if (cedula.length !== 10 || !/^\d{10}$/.test(cedula)) return false;

  const provincia = parseInt(cedula.substring(0, 2), 10);
  if (provincia < 1 || (provincia > 24 && provincia !== 30)) return false;

  const tercerDigito = parseInt(cedula.substring(2, 3), 10);
  if (tercerDigito >= 6) return false;

  const coeficientes = [2, 1, 2, 1, 2, 1, 2, 1, 2];
  let suma = 0;

  for (let i = 0; i < 9; i++) {
    let digito = parseInt(cedula[i], 10);
    let prod = digito * coeficientes[i];
    if (prod >= 10) prod -= 9;
    suma += prod;
  }

  const residuo = suma % 10;
  const digitoVerificador = residuo === 0 ? 0 : 10 - residuo;
  return digitoVerificador === parseInt(cedula[9], 10);
}

// Format Currency
function formatCurrency(amount) {
  return new Intl.NumberFormat('es-EC', { style: 'currency', currency: 'USD' }).format(amount || 0);
}

// Format Date Time
function formatDateTime(dateString) {
  if (!dateString) return '-';
  const date = new Date(dateString);
  return date.toLocaleString('es-EC', { dateStyle: 'short', timeStyle: 'short' });
}

// -------------------------------------------------------------
// 1. DASHBOARD MODULE
// -------------------------------------------------------------
async function loadDashboard() {
  try {
    const resSummary = await fetch(`${API_BASE}/reportes/dashboard`);
    if (resSummary.ok) {
      const summary = await resSummary.json();
      document.getElementById('dash-clientes').textContent = summary.totalClientes;
      document.getElementById('dash-platos').textContent = summary.totalPlatos;
      document.getElementById('dash-mesas-disponibles').textContent = summary.mesasDisponibles;
      document.getElementById('dash-mesas-ocupadas').textContent = summary.mesasOcupadas;
      document.getElementById('dash-pedidos-hoy').textContent = summary.pedidosHoy;
      document.getElementById('dash-ventas-hoy').textContent = formatCurrency(summary.ventasHoy);
    }

    const resPedidos = await fetch(`${API_BASE}/pedidos`);
    if (resPedidos.ok) {
      const pedidos = await resPedidos.json();
      const tbody = document.getElementById('dash-table-body');
      tbody.innerHTML = '';

      if (pedidos.length === 0) {
        tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;">No hay pedidos registrados.</td></tr>';
        return;
      }

      pedidos.slice(0, 5).forEach(p => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
          <td><strong>#${p.id}</strong></td>
          <td>${p.clienteNombreCompleto}</td>
          <td>${p.clienteCedula}</td>
          <td>${p.numeroMesa ? `Mesa ${p.numeroMesa}` : 'Sin mesa'}</td>
          <td>${formatDateTime(p.fecha)}</td>
          <td><span class="badge badge-${p.estado.toLowerCase()}">${p.estado}</span></td>
          <td><strong>${formatCurrency(p.total)}</strong></td>
        `;
        tbody.appendChild(tr);
      });
    }
  } catch (err) {
    console.error('Error al cargar dashboard:', err);
  }
}

// -------------------------------------------------------------
// 2. CLIENTES MODULE
// -------------------------------------------------------------
async function loadClientes() {
  const search = document.getElementById('cliente-search').value.trim();
  let url = `${API_BASE}/clientes`;
  if (search) url += `?buscar=${encodeURIComponent(search)}`;

  try {
    const res = await fetch(url);
    if (!res.ok) throw new Error('Error al cargar clientes');
    const clientes = await res.json();

    const tbody = document.getElementById('clientes-table-body');
    tbody.innerHTML = '';

    if (clientes.length === 0) {
      tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;">No se encontraron clientes.</td></tr>';
      return;
    }

    clientes.forEach(c => {
      const tr = document.createElement('tr');
      tr.innerHTML = `
        <td>${c.id}</td>
        <td><strong>${c.cedula}</strong></td>
        <td>${c.nombre} ${c.apellido}</td>
        <td>${c.telefono || '-'}</td>
        <td>${c.email || '-'}</td>
        <td><span class="badge ${c.activo ? 'badge-disponible' : 'badge-cancelado'}">${c.activo ? 'Activo' : 'Inactivo'}</span></td>
        <td>
          <button type="button" class="btn btn-secondary btn-sm" onclick="editCliente(${c.id})">Editar</button>
          <button type="button" class="btn btn-danger btn-sm" onclick="eliminarCliente(${c.id})">Eliminar</button>
        </td>
      `;
      tbody.appendChild(tr);
    });
  } catch (err) {
    showAlert(err.message);
  }
}

function openClienteModal(cliente = null) {
  document.getElementById('form-cliente').reset();
  if (cliente) {
    document.getElementById('modal-cliente-title').textContent = 'Editar Cliente';
    document.getElementById('cliente-id').value = cliente.id;
    document.getElementById('cliente-nombre').value = cliente.nombre;
    document.getElementById('cliente-apellido').value = cliente.apellido;
    document.getElementById('cliente-cedula').value = cliente.cedula;
    document.getElementById('cliente-telefono').value = cliente.telefono;
    document.getElementById('cliente-email').value = cliente.email;
    document.getElementById('cliente-activo').checked = cliente.activo;
  } else {
    document.getElementById('modal-cliente-title').textContent = 'Registrar Cliente';
    document.getElementById('cliente-id').value = '';
    document.getElementById('cliente-activo').checked = true;
  }
  document.getElementById('modal-cliente').classList.add('active');
}

function closeClienteModal() {
  document.getElementById('modal-cliente').classList.remove('active');
}

async function editCliente(id) {
  try {
    const res = await fetch(`${API_BASE}/clientes/${id}`);
    if (!res.ok) throw new Error('No se pudo obtener la información del cliente.');
    const cliente = await res.json();
    openClienteModal(cliente);
  } catch (err) {
    showAlert(err.message);
  }
}

async function guardarCliente(event) {
  event.preventDefault();
  const id = document.getElementById('cliente-id').value;
  const nombre = document.getElementById('cliente-nombre').value.trim();
  const apellido = document.getElementById('cliente-apellido').value.trim();
  const cedula = document.getElementById('cliente-cedula').value.trim();
  const telefono = document.getElementById('cliente-telefono').value.trim();
  const email = document.getElementById('cliente-email').value.trim();
  const activo = document.getElementById('cliente-activo').checked;

  if (!validarCedulaEcuatoriana(cedula)) {
    showAlert('La cédula ingresada no es válida según el algoritmo de verificación ecuatoriano (10 dígitos).', 'danger');
    return;
  }

  const payload = { nombre, apellido, cedula, telefono, email, activo };
  const method = id ? 'PUT' : 'POST';
  const url = id ? `${API_BASE}/clientes/${id}` : `${API_BASE}/clientes`;

  try {
    const res = await fetch(url, {
      method,
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    const data = await res.json();
    if (!res.ok) {
      throw new Error(data.mensaje || 'Error al guardar cliente.');
    }

    showAlert(`Cliente ${id ? 'actualizado' : 'registrado'} exitosamente.`, 'success');
    closeClienteModal();
    loadClientes();
  } catch (err) {
    showAlert(err.message);
  }
}

async function eliminarCliente(id) {
  if (!confirm('¿Está seguro de que desea eliminar este cliente?')) return;

  try {
    const res = await fetch(`${API_BASE}/clientes/${id}`, { method: 'DELETE' });
    const data = await res.json();

    if (res.status === 409) {
      // Conflict business rule triggered
      if (confirm(`${data.mensaje}\n\n¿Desea desactivar al cliente en su lugar?`)) {
        await desactivarCliente(id);
      }
      return;
    }

    if (!res.ok) {
      throw new Error(data.mensaje || 'Error al eliminar cliente.');
    }

    showAlert(data.mensaje || 'Cliente eliminado exitosamente.', 'success');
    loadClientes();
  } catch (err) {
    showAlert(err.message);
  }
}

async function desactivarCliente(id) {
  try {
    const resGet = await fetch(`${API_BASE}/clientes/${id}`);
    if (!resGet.ok) return;
    const cliente = await resGet.json();

    cliente.activo = false;
    const resPut = await fetch(`${API_BASE}/clientes/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(cliente)
    });

    if (resPut.ok) {
      showAlert('El cliente ha sido desactivado exitosamente.', 'success');
      loadClientes();
    }
  } catch (err) {
    showAlert(err.message);
  }
}

// -------------------------------------------------------------
// 3. PLATOS MODULE
// -------------------------------------------------------------
async function loadPlatos() {
  const categoria = document.getElementById('plato-categoria-filter').value;
  let url = `${API_BASE}/platos`;
  if (categoria) url += `?categoria=${encodeURIComponent(categoria)}`;

  try {
    const res = await fetch(url);
    if (!res.ok) throw new Error('Error al cargar menú de platos.');
    const platos = await res.json();

    const tbody = document.getElementById('platos-table-body');
    tbody.innerHTML = '';

    if (platos.length === 0) {
      tbody.innerHTML = '<tr><td colspan="8" style="text-align:center;">No hay platos registrados.</td></tr>';
      return;
    }

    platos.forEach(p => {
      const tr = document.createElement('tr');
      tr.innerHTML = `
        <td>${p.id}</td>
        <td><strong>${p.nombre}</strong></td>
        <td>${p.categoria}</td>
        <td>${p.descripcion || '-'}</td>
        <td><strong>${formatCurrency(p.precio)}</strong></td>
        <td><span class="badge ${p.disponible ? 'badge-disponible' : 'badge-cancelado'}">${p.disponible ? 'Disponible' : 'Agotado'}</span></td>
        <td><span class="badge ${p.activo ? 'badge-disponible' : 'badge-cancelado'}">${p.activo ? 'Activo' : 'Inactivo'}</span></td>
        <td>
          <button type="button" class="btn btn-secondary btn-sm" onclick="editPlato(${p.id})">Editar</button>
          <button type="button" class="btn btn-danger btn-sm" onclick="eliminarPlato(${p.id})">Eliminar</button>
        </td>
      `;
      tbody.appendChild(tr);
    });
  } catch (err) {
    showAlert(err.message);
  }
}

function openPlatoModal(plato = null) {
  document.getElementById('form-plato').reset();
  if (plato) {
    document.getElementById('modal-plato-title').textContent = 'Editar Plato';
    document.getElementById('plato-id').value = plato.id;
    document.getElementById('plato-nombre').value = plato.nombre;
    document.getElementById('plato-categoria').value = plato.categoria;
    document.getElementById('plato-precio').value = plato.precio;
    document.getElementById('plato-descripcion').value = plato.descripcion;
    document.getElementById('plato-disponible').checked = plato.disponible;
    document.getElementById('plato-activo').checked = plato.activo;
  } else {
    document.getElementById('modal-plato-title').textContent = 'Registrar Plato';
    document.getElementById('plato-id').value = '';
    document.getElementById('plato-disponible').checked = true;
    document.getElementById('plato-activo').checked = true;
  }
  document.getElementById('modal-plato').classList.add('active');
}

function closePlatoModal() {
  document.getElementById('modal-plato').classList.remove('active');
}

async function editPlato(id) {
  try {
    const res = await fetch(`${API_BASE}/platos/${id}`);
    if (!res.ok) throw new Error('No se pudo obtener la información del plato.');
    const plato = await res.json();
    openPlatoModal(plato);
  } catch (err) {
    showAlert(err.message);
  }
}

async function guardarPlato(event) {
  event.preventDefault();
  const id = document.getElementById('plato-id').value;
  const nombre = document.getElementById('plato-nombre').value.trim();
  const categoria = document.getElementById('plato-categoria').value;
  const precio = parseFloat(document.getElementById('plato-precio').value);
  const descripcion = document.getElementById('plato-descripcion').value.trim();
  const disponible = document.getElementById('plato-disponible').checked;
  const activo = document.getElementById('plato-activo').checked;

  if (precio <= 0) {
    showAlert('El precio del plato debe ser un monto mayor a cero.', 'danger');
    return;
  }

  const payload = { nombre, categoria, precio, descripcion, disponible, activo };
  const method = id ? 'PUT' : 'POST';
  const url = id ? `${API_BASE}/platos/${id}` : `${API_BASE}/platos`;

  try {
    const res = await fetch(url, {
      method,
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    const data = await res.json();
    if (!res.ok) throw new Error(data.mensaje || 'Error al guardar el plato.');

    showAlert(`Plato ${id ? 'actualizado' : 'registrado'} exitosamente.`, 'success');
    closePlatoModal();
    loadPlatos();
  } catch (err) {
    showAlert(err.message);
  }
}

async function eliminarPlato(id) {
  if (!confirm('¿Está seguro de que desea eliminar este plato?')) return;

  try {
    const res = await fetch(`${API_BASE}/platos/${id}`, { method: 'DELETE' });
    const data = await res.json();

    if (res.status === 409) {
      if (confirm(`${data.mensaje}\n\n¿Desea marcar el plato como NO disponible en su lugar?`)) {
        await desactivarPlato(id);
      }
      return;
    }

    if (!res.ok) throw new Error(data.mensaje || 'Error al eliminar plato.');

    showAlert(data.mensaje || 'Plato eliminado exitosamente.', 'success');
    loadPlatos();
  } catch (err) {
    showAlert(err.message);
  }
}

async function desactivarPlato(id) {
  try {
    const resGet = await fetch(`${API_BASE}/platos/${id}`);
    if (!resGet.ok) return;
    const plato = await resGet.json();

    plato.disponible = false;
    const resPut = await fetch(`${API_BASE}/platos/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(plato)
    });

    if (resPut.ok) {
      showAlert('El plato ha sido marcado como NO disponible.', 'success');
      loadPlatos();
    }
  } catch (err) {
    showAlert(err.message);
  }
}

// -------------------------------------------------------------
// 4. MESAS MODULE
// -------------------------------------------------------------
async function loadMesas() {
  try {
    const res = await fetch(`${API_BASE}/mesas`);
    if (!res.ok) throw new Error('Error al cargar mesas.');
    const mesas = await res.json();

    const grid = document.getElementById('mesas-grid');
    grid.innerHTML = '';

    if (mesas.length === 0) {
      grid.innerHTML = '<div style="grid-column: 1/-1; text-align:center;">No hay mesas registradas.</div>';
      return;
    }

    mesas.forEach(m => {
      const card = document.createElement('div');
      card.className = 'table-card';
      const estadoBadgeClass = `badge-${m.estado.toLowerCase()}`;
      card.innerHTML = `
        <div class="table-card-header">
          <div class="table-number">Mesa #${m.numeroMesa}</div>
          <span class="badge ${estadoBadgeClass}">${m.estado}</span>
        </div>
        <div class="table-capacity">
          <span>👥 Capacidad:</span>
          <strong>${m.capacidad} personas</strong>
        </div>
        <div>
          <span class="badge ${m.activo ? 'badge-disponible' : 'badge-cancelado'}">${m.activo ? 'Habilitada' : 'Inhabilitada'}</span>
        </div>
        <div style="display:flex; gap:0.5rem; margin-top:0.5rem;">
          <button type="button" class="btn btn-secondary btn-sm" style="flex:1;" onclick="editMesa(${m.id})">Editar</button>
          <button type="button" class="btn btn-danger btn-sm" onclick="eliminarMesa(${m.id})">Eliminar</button>
        </div>
      `;
      grid.appendChild(card);
    });
  } catch (err) {
    showAlert(err.message);
  }
}

function openMesaModal(mesa = null) {
  document.getElementById('form-mesa').reset();
  if (mesa) {
    document.getElementById('modal-mesa-title').textContent = 'Editar Mesa';
    document.getElementById('mesa-id').value = mesa.id;
    document.getElementById('mesa-numero').value = mesa.numeroMesa;
    document.getElementById('mesa-capacidad').value = mesa.capacidad;
    document.getElementById('mesa-estado').value = mesa.estado;
    document.getElementById('mesa-activo').checked = mesa.activo;
  } else {
    document.getElementById('modal-mesa-title').textContent = 'Registrar Mesa';
    document.getElementById('mesa-id').value = '';
    document.getElementById('mesa-estado').value = 'Disponible';
    document.getElementById('mesa-activo').checked = true;
  }
  document.getElementById('modal-mesa').classList.add('active');
}

function closeMesaModal() {
  document.getElementById('modal-mesa').classList.remove('active');
}

async function editMesa(id) {
  try {
    const res = await fetch(`${API_BASE}/mesas/${id}`);
    if (!res.ok) throw new Error('No se pudo obtener la información de la mesa.');
    const mesa = await res.json();
    openMesaModal(mesa);
  } catch (err) {
    showAlert(err.message);
  }
}

async function guardarMesa(event) {
  event.preventDefault();
  const id = document.getElementById('mesa-id').value;
  const numeroMesa = parseInt(document.getElementById('mesa-numero').value, 10);
  const capacidad = parseInt(document.getElementById('mesa-capacidad').value, 10);
  const estado = document.getElementById('mesa-estado').value;
  const activo = document.getElementById('mesa-activo').checked;

  if (numeroMesa <= 0 || capacidad <= 0) {
    showAlert('El número de mesa y la capacidad deben ser mayores a cero.', 'danger');
    return;
  }

  const payload = { numeroMesa, capacidad, estado, activo };
  const method = id ? 'PUT' : 'POST';
  const url = id ? `${API_BASE}/mesas/${id}` : `${API_BASE}/mesas`;

  try {
    const res = await fetch(url, {
      method,
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    const data = await res.json();
    if (!res.ok) throw new Error(data.mensaje || 'Error al guardar la mesa.');

    showAlert(`Mesa ${id ? 'actualizada' : 'registrada'} exitosamente.`, 'success');
    closeMesaModal();
    loadMesas();
  } catch (err) {
    showAlert(err.message);
  }
}

async function eliminarMesa(id) {
  if (!confirm('¿Está seguro de que desea eliminar esta mesa?')) return;

  try {
    const res = await fetch(`${API_BASE}/mesas/${id}`, { method: 'DELETE' });
    const data = await res.json();

    if (res.status === 409) {
      showAlert(data.mensaje, 'danger', 8000);
      return;
    }

    if (!res.ok) throw new Error(data.mensaje || 'Error al eliminar la mesa.');

    showAlert(data.mensaje || 'Mesa eliminada exitosamente.', 'success');
    loadMesas();
  } catch (err) {
    showAlert(err.message);
  }
}

// -------------------------------------------------------------
// 5. PEDIDOS MODULE
// -------------------------------------------------------------
async function loadPedidos() {
  const estado = document.getElementById('order-filter-estado').value;
  let url = `${API_BASE}/pedidos`;
  if (estado) url += `?estado=${encodeURIComponent(estado)}`;

  try {
    const res = await fetch(url);
    if (!res.ok) throw new Error('Error al cargar historial de pedidos.');
    const pedidos = await res.json();

    const tbody = document.getElementById('pedidos-table-body');
    tbody.innerHTML = '';

    if (pedidos.length === 0) {
      tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;">No hay pedidos en el historial.</td></tr>';
      return;
    }

    pedidos.forEach(p => {
      const tr = document.createElement('tr');
      tr.innerHTML = `
        <td><strong>#${p.id}</strong></td>
        <td>${p.clienteNombreCompleto} <br><small style="color:var(--text-dim);">${p.clienteCedula}</small></td>
        <td>${p.numeroMesa ? `Mesa #${p.numeroMesa}` : 'Sin mesa'}</td>
        <td>${formatDateTime(p.fecha)}</td>
        <td><span class="badge badge-${p.estado.toLowerCase()}">${p.estado}</span></td>
        <td><strong>${formatCurrency(p.total)}</strong></td>
        <td>
          <button type="button" class="btn btn-secondary btn-sm" onclick="verPedidoDetalle(${p.id})">Ver Detalles</button>
        </td>
      `;
      tbody.appendChild(tr);
    });
  } catch (err) {
    showAlert(err.message);
  }
}

async function toggleOrderCreationForm(show) {
  const card = document.getElementById('order-creation-card');
  if (show) {
    card.style.display = 'block';
    cartItems = [];
    updateCartDisplay();
    await populateOrderDropdownsAndDishes();
  } else {
    card.style.display = 'none';
  }
}

async function populateOrderDropdownsAndDishes() {
  // Populate active clients
  try {
    const resClientes = await fetch(`${API_BASE}/clientes?soloActivos=true`);
    const clientes = await resClientes.json();
    const clienteSelect = document.getElementById('order-cliente-select');
    clienteSelect.innerHTML = '<option value="">Seleccione un cliente...</option>';
    clientes.forEach(c => {
      clienteSelect.innerHTML += `<option value="${c.id}">${c.apellido} ${c.nombre} (C.I.: ${c.cedula})</option>`;
    });

    // Populate available tables (Disponibles)
    const resMesas = await fetch(`${API_BASE}/mesas?soloActivas=true&estado=Disponible`);
    const mesas = await resMesas.json();
    const mesaSelect = document.getElementById('order-mesa-select');
    mesaSelect.innerHTML = '<option value="">Sin mesa asignada</option>';
    mesas.forEach(m => {
      mesaSelect.innerHTML += `<option value="${m.id}">Mesa #${m.numeroMesa} (Capacidad: ${m.capacidad} personas)</option>`;
    });

    // Populate available dishes
    const resPlatos = await fetch(`${API_BASE}/platos?soloDisponibles=true&soloActivos=true`);
    availableDishesList = await resPlatos.json();
    const dishGrid = document.getElementById('order-dish-grid');
    dishGrid.innerHTML = '';

    availableDishesList.forEach(p => {
      const dCard = document.createElement('div');
      dCard.className = 'dish-card';
      dCard.onclick = () => addDishToCart(p);
      dCard.innerHTML = `
        <div>
          <div class="dish-name">${p.nombre}</div>
          <div class="dish-category">${p.categoria}</div>
        </div>
        <div style="display:flex; justify-content:space-between; align-items:center;">
          <span class="dish-price">${formatCurrency(p.precio)}</span>
          <span class="btn btn-primary btn-sm">+ Agregar</span>
        </div>
      `;
      dishGrid.appendChild(dCard);
    });
  } catch (err) {
    showAlert('Error al cargar datos para la creación del pedido.');
  }
}

function addDishToCart(plato) {
  const existing = cartItems.find(item => item.platoId === plato.id);
  if (existing) {
    existing.cantidad += 1;
  } else {
    cartItems.push({
      platoId: plato.id,
      nombre: plato.nombre,
      precio: plato.precio,
      cantidad: 1
    });
  }
  updateCartDisplay();
}

function updateCartItemQty(platoId, delta) {
  const item = cartItems.find(i => i.platoId === platoId);
  if (!item) return;

  item.cantidad += delta;
  if (item.cantidad <= 0) {
    cartItems = cartItems.filter(i => i.platoId !== platoId);
  }
  updateCartDisplay();
}

function updateCartDisplay() {
  const container = document.getElementById('cart-items-container');
  container.innerHTML = '';

  if (cartItems.length === 0) {
    container.innerHTML = '<div style="text-align: center; color: var(--text-dim); padding: 2rem 0;">No ha seleccionado ningún plato.</div>';
    document.getElementById('cart-total-amount').textContent = '$0.00';
    return;
  }

  let totalCalculadoVisual = 0;

  cartItems.forEach(item => {
    const subtotal = item.precio * item.cantidad;
    totalCalculadoVisual += subtotal;

    const div = document.createElement('div');
    div.className = 'cart-item';
    div.innerHTML = `
      <div class="cart-item-info">
        <div class="cart-item-title">${item.nombre}</div>
        <div class="cart-item-price">${formatCurrency(item.precio)} c/u = <strong>${formatCurrency(subtotal)}</strong></div>
      </div>
      <div class="cart-qty-controls">
        <button type="button" class="cart-qty-btn" onclick="updateCartItemQty(${item.platoId}, -1)">-</button>
        <span style="font-weight:bold; width:20px; text-align:center;">${item.cantidad}</span>
        <button type="button" class="cart-qty-btn" onclick="updateCartItemQty(${item.platoId}, 1)">+</button>
      </div>
    `;
    container.appendChild(div);
  });

  document.getElementById('cart-total-amount').textContent = formatCurrency(totalCalculadoVisual);
}

async function guardarNuevoPedido() {
  const clienteId = parseInt(document.getElementById('order-cliente-select').value, 10);
  const mesaIdVal = document.getElementById('order-mesa-select').value;
  const mesaId = mesaIdVal ? parseInt(mesaIdVal, 10) : null;

  if (!clienteId) {
    showAlert('Debe seleccionar un cliente para registrar el pedido.', 'danger');
    return;
  }

  if (cartItems.length === 0) {
    showAlert('Debe agregar al menos un plato al pedido.', 'danger');
    return;
  }

  const payload = {
    clienteId,
    mesaId,
    detalles: cartItems.map(item => ({
      platoId: item.platoId,
      cantidad: item.cantidad
    }))
  };

  try {
    const res = await fetch(`${API_BASE}/pedidos`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    const data = await res.json();
    if (!res.ok) throw new Error(data.mensaje || 'Error al crear el pedido.');

    showAlert(`¡Pedido #${data.id} registrado exitosamente por un total de ${formatCurrency(data.total)}!`, 'success');
    toggleOrderCreationForm(false);
    loadPedidos();
    loadDashboard();
  } catch (err) {
    showAlert(err.message);
  }
}

async function verPedidoDetalle(id) {
  try {
    const res = await fetch(`${API_BASE}/pedidos/${id}`);
    if (!res.ok) throw new Error('No se pudo cargar la información del pedido.');
    const p = await res.json();

    document.getElementById('modal-pedido-title').textContent = `Detalle del Pedido #${p.id}`;
    const content = document.getElementById('modal-pedido-content');

    const esTerminal = p.estado === 'Completado' || p.estado === 'Cancelado';

    content.innerHTML = `
      <div style="display:grid; grid-template-columns: 1fr 1fr; gap: 1rem; margin-bottom: 1rem;">
        <div>
          <div style="color:var(--text-dim); font-size:0.8rem;">Cliente:</div>
          <div style="font-weight:bold; font-size:1rem;">${p.clienteNombreCompleto}</div>
          <div style="color:var(--text-dim); font-size:0.85rem;">C.I.: ${p.clienteCedula}</div>
        </div>
        <div>
          <div style="color:var(--text-dim); font-size:0.8rem;">Mesa / Ubicación:</div>
          <div style="font-weight:bold; font-size:1rem;">${p.numeroMesa ? `Mesa #${p.numeroMesa}` : 'Sin Mesa Asignada'}</div>
          <div style="color:var(--text-dim); font-size:0.85rem;">Fecha: ${formatDateTime(p.fecha)}</div>
        </div>
      </div>

      <div class="card" style="padding:1rem; margin-bottom:1rem; background-color: var(--bg-input);">
        <div style="display:flex; justify-content:space-between; align-items:center;">
          <span>Estado Actual: <span class="badge badge-${p.estado.toLowerCase()}">${p.estado}</span></span>
          ${!esTerminal ? `
            <div style="display:flex; align-items:center; gap:0.5rem;">
              <select id="change-estado-select" class="form-control" style="padding:0.4rem 0.6rem;">
                ${p.estado === 'Pendiente' ? '<option value="EnPreparacion">En Preparación</option>' : ''}
                <option value="Completado">Completado</option>
                <option value="Cancelado">Cancelado</option>
              </select>
              <button type="button" class="btn btn-primary btn-sm" onclick="ejecutarCambioEstado(${p.id})">Actualizar Estado</button>
            </div>
          ` : '<span style="font-size:0.8rem; color:var(--text-dim);">(Estado Final - No Modificable)</span>'}
        </div>
      </div>

      <h4 style="margin-bottom:0.75rem;">Platos Solicitados</h4>
      <div class="table-wrapper">
        <table class="data-table">
          <thead>
            <tr>
              <th>Plato</th>
              <th>Cant.</th>
              <th>Precio Unit.</th>
              <th>Subtotal</th>
            </tr>
          </thead>
          <tbody>
            ${p.detalles.map(d => `
              <tr>
                <td><strong>${d.platoNombre}</strong></td>
                <td>${d.cantidad}</td>
                <td>${formatCurrency(d.precioUnitario)}</td>
                <td><strong>${formatCurrency(d.subtotal)}</strong></td>
              </tr>
            `).join('')}
          </tbody>
        </table>
      </div>

      <div class="order-total-summary" style="margin-top:1rem; margin-bottom:0;">
        <span class="order-total-label">TOTAL DEL PEDIDO:</span>
        <span class="order-total-amount">${formatCurrency(p.total)}</span>
      </div>
    `;

    document.getElementById('modal-pedido-detalle').classList.add('active');
  } catch (err) {
    showAlert(err.message);
  }
}

function closePedidoDetalleModal() {
  document.getElementById('modal-pedido-detalle').classList.remove('active');
}

async function ejecutarCambioEstado(pedidoId) {
  const nuevoEstado = document.getElementById('change-estado-select').value;

  try {
    const res = await fetch(`${API_BASE}/pedidos/${pedidoId}/estado`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ nuevoEstado })
    });

    const data = await res.json();
    if (!res.ok) throw new Error(data.mensaje || 'Error al cambiar el estado del pedido.');

    showAlert(data.mensaje || 'Estado actualizado.', 'success');
    closePedidoDetalleModal();
    loadPedidos();
    loadDashboard();
  } catch (err) {
    showAlert(err.message);
  }
}

// -------------------------------------------------------------
// 6. REPORTES MODULE
// -------------------------------------------------------------
async function initReportesView() {
  try {
    const resClientes = await fetch(`${API_BASE}/clientes`);
    const clientes = await resClientes.json();
    const select = document.getElementById('reporte-cliente-select');
    select.innerHTML = '<option value="">Seleccione un cliente...</option>';
    clientes.forEach(c => {
      select.innerHTML += `<option value="${c.id}">${c.apellido} ${c.nombre} (C.I.: ${c.cedula})</option>`;
    });
  } catch (err) {
    showAlert('Error al cargar la lista de clientes para reportes.');
  }
}

async function cargarReporteCliente() {
  const clienteId = document.getElementById('reporte-cliente-select').value;
  if (!clienteId) {
    showAlert('Por favor, seleccione un cliente para generar el reporte.', 'danger');
    return;
  }

  const fechaInicio = document.getElementById('reporte-fecha-inicio').value;
  const fechaFin = document.getElementById('reporte-fecha-fin').value;
  const estado = document.getElementById('reporte-estado-select').value;

  let url = `${API_BASE}/reportes/cliente/${clienteId}?`;
  if (fechaInicio) url += `fechaInicio=${encodeURIComponent(fechaInicio)}&`;
  if (fechaFin) url += `fechaFin=${encodeURIComponent(fechaFin)}&`;
  if (estado) url += `estado=${encodeURIComponent(estado)}&`;

  try {
    const res = await fetch(url);
    if (!res.ok) throw new Error('Error al obtener el reporte del cliente.');
    const data = await res.json();

    const container = document.getElementById('reporte-resultado-container');

    if (data.pedidos.length === 0) {
      container.innerHTML = `
        <div class="card" style="text-align:center; padding: 2rem;">
          <h3>${data.cliente.nombre} ${data.cliente.apellido}</h3>
          <p style="color:var(--text-dim); margin-top:0.5rem;">Cédula: ${data.cliente.cedula} | Email: ${data.cliente.email || 'N/A'}</p>
          <hr style="border-color:var(--border-color); margin: 1rem 0;">
          <p>No se encontraron pedidos registrados con los filtros seleccionados.</p>
        </div>
      `;
      return;
    }

    let html = `
      <div class="card">
        <div style="display:flex; justify-content:space-between; align-items:center; flex-wrap:wrap; gap:1rem;">
          <div>
            <h3 style="font-size:1.3rem; margin-bottom:0.25rem;">REPORTE DE PEDIDOS POR CLIENTE</h3>
            <h4 style="color:var(--accent-primary); font-size:1.1rem;">${data.cliente.nombre} ${data.cliente.apellido}</h4>
            <div style="color:var(--text-dim); font-size:0.85rem; margin-top:0.25rem;">
              <span>Cédula: <strong>${data.cliente.cedula}</strong></span> |
              <span>Teléfono: <strong>${data.cliente.telefono || 'N/A'}</strong></span> |
              <span>Email: <strong>${data.cliente.email || 'N/A'}</strong></span>
            </div>
          </div>
          <div style="text-align:right;">
            <div style="font-size:0.85rem; color:var(--text-dim);">Total de Pedidos: <strong>${data.totalPedidos}</strong></div>
            <div style="font-size:1.2rem; font-weight:800; color:var(--accent-primary); margin-top:0.25rem;">Ventas Acumuladas: ${formatCurrency(data.totalVentas)}</div>
          </div>
        </div>
      </div>
    `;

    data.pedidos.forEach(p => {
      html += `
        <div class="card" style="margin-bottom: 1.25rem;">
          <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom: 0.75rem; border-bottom:1px solid var(--border-color); padding-bottom:0.5rem;">
            <div>
              <strong>Pedido #${p.id}</strong> - <span style="color:var(--text-dim); font-size:0.85rem;">${formatDateTime(p.fecha)}</span>
              <span style="margin-left:0.75rem;">${p.numeroMesa ? `Mesa #${p.numeroMesa}` : 'Sin mesa'}</span>
            </div>
            <div>
              <span class="badge badge-${p.estado.toLowerCase()}">${p.estado}</span>
            </div>
          </div>

          <div class="table-wrapper">
            <table class="data-table">
              <thead>
                <tr>
                  <th>Plato</th>
                  <th>Cantidad</th>
                  <th>Precio Unitario</th>
                  <th>Subtotal</th>
                </tr>
              </thead>
              <tbody>
                ${p.detalles.map(d => `
                  <tr>
                    <td><strong>${d.platoNombre}</strong></td>
                    <td>${d.cantidad}</td>
                    <td>${formatCurrency(d.precioUnitario)}</td>
                    <td><strong>${formatCurrency(d.subtotal)}</strong></td>
                  </tr>
                `).join('')}
              </tbody>
            </table>
          </div>

          <div style="text-align:right; margin-top:0.75rem; font-size:1.05rem;">
            Total del Pedido: <strong style="color:var(--accent-primary);">${formatCurrency(p.total)}</strong>
          </div>
        </div>
      `;
    });

    container.innerHTML = html;
  } catch (err) {
    showAlert(err.message);
  }
}

// Event Listeners for Filter Inputs
function setupFilterListeners() {
  document.getElementById('cliente-search').addEventListener('input', () => {
    if (currentView === 'clientes') loadClientes();
  });

  document.getElementById('plato-categoria-filter').addEventListener('change', () => {
    if (currentView === 'platos') loadPlatos();
  });

  document.getElementById('order-filter-estado').addEventListener('change', () => {
    if (currentView === 'pedidos') loadPedidos();
  });
}
