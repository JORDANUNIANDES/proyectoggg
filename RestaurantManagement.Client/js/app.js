const API_BASE = '/api';

// State
let clientesState = [];
let platosState = [];
let mesasState = [];
let pedidosState = [];
let orderItemsBuilder = [];

// DOM Loaded
document.addEventListener('DOMContentLoaded', () => {
    initNavigation();
    initSearchFilters();
    loadDashboardData();
});

// Navigation
function initNavigation() {
    const navButtons = document.querySelectorAll('.nav-btn');
    navButtons.forEach(btn => {
        btn.addEventListener('click', () => {
            const targetView = btn.getAttribute('data-view');

            navButtons.forEach(b => b.classList.remove('active'));
            btn.classList.add('active');

            document.querySelectorAll('.view-section').forEach(sec => sec.classList.remove('active'));
            document.getElementById(`view-${targetView}`).classList.add('active');

            document.getElementById('page-title').textContent = btn.textContent.trim();

            loadViewData(targetView);
        });
    });
}

function loadViewData(viewName) {
    switch (viewName) {
        case 'dashboard':
            loadDashboardData();
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
            loadReportesClientesSelect();
            break;
    }
}

// Notifications
function showAlert(message, type = 'danger') {
    const container = document.getElementById('alert-container');
    const alert = document.createElement('div');
    alert.className = `alert alert-${type}`;
    alert.innerHTML = `
        <span>${message}</span>
        <button onclick="this.parentElement.remove()" style="background:none;border:none;cursor:pointer;font-weight:bold;">&times;</button>
    `;
    container.appendChild(alert);

    setTimeout(() => {
        if (alert.parentNode) alert.remove();
    }, 5000);
}

// Modal Control
function closeModal(modalId) {
    document.getElementById(modalId).classList.remove('active');
}

function openModal(modalId) {
    document.getElementById(modalId).classList.add('active');
}

// DASHBOARD
async function loadDashboardData() {
    try {
        const [dashRes, pedidosRes] = await Promise.all([
            fetch(`${API_BASE}/reportes/dashboard`),
            fetch(`${API_BASE}/pedidos`)
        ]);

        if (dashRes.ok) {
            const data = await dashRes.json();
            document.getElementById('dash-clientes').textContent = data.totalClientes;
            document.getElementById('dash-platos').textContent = data.platosDisponibles;
            document.getElementById('dash-mesas').textContent = data.totalMesas;
            document.getElementById('dash-pedidos').textContent = data.pedidosRegistrados;
            document.getElementById('dash-pendientes').textContent = data.pedidosPendientes;
            document.getElementById('dash-ventas').textContent = `$${data.ventasTotales.toFixed(2)}`;
        }

        if (pedidosRes.ok) {
            const pedidos = await pedidosRes.json();
            const tbody = document.querySelector('#table-dashboard-pedidos tbody');
            tbody.innerHTML = '';
            pedidos.slice(0, 5).forEach(p => {
                const tr = document.createElement('tr');
                tr.innerHTML = `
                    <td>#${p.id}</td>
                    <td>${p.clienteNombreCompleto}</td>
                    <td>${p.mesaNumero ? 'Mesa ' + p.mesaNumero : 'N/A'}</td>
                    <td>${new Date(p.fechaPedido).toLocaleString()}</td>
                    <td><span class="badge ${getBadgeClass(p.estado)}">${p.estado}</span></td>
                    <td>$${p.total.toFixed(2)}</td>
                `;
                tbody.appendChild(tr);
            });
        }
    } catch (err) {
        console.error("Error al cargar dashboard:", err);
    }
}

// CLIENTES
async function loadClientes() {
    try {
        const res = await fetch(`${API_BASE}/clientes`);
        if (res.ok) {
            clientesState = await res.json();
            renderClientesTable(clientesState);
        }
    } catch (err) {
        showAlert("Error al obtener el listado de clientes.");
    }
}

function renderClientesTable(clientes) {
    const tbody = document.querySelector('#table-clientes tbody');
    tbody.innerHTML = '';

    clientes.forEach(c => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${c.id}</td>
            <td>${c.nombre} ${c.apellido}</td>
            <td>${c.cedula}</td>
            <td>${c.telefono || '-'}</td>
            <td>${c.email || '-'}</td>
            <td>${c.direccion || '-'}</td>
            <td><span class="badge ${c.estado ? 'badge-success' : 'badge-danger'}">${c.estado ? 'Activo' : 'Inactivo'}</span></td>
            <td>
                <button class="btn btn-secondary btn-sm" onclick="editCliente(${c.id})"><i class="fa-solid fa-pen"></i></button>
                <button class="btn btn-danger btn-sm" onclick="deleteCliente(${c.id})"><i class="fa-solid fa-trash"></i></button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

function openClienteModal() {
    document.getElementById('form-cliente').reset();
    document.getElementById('cliente-id').value = '';
    document.getElementById('modal-cliente-title').textContent = 'Nuevo Cliente';
    openModal('modal-cliente');
}

function editCliente(id) {
    const cliente = clientesState.find(c => c.id === id);
    if (!cliente) return;

    document.getElementById('cliente-id').value = cliente.id;
    document.getElementById('cliente-nombre').value = cliente.nombre;
    document.getElementById('cliente-apellido').value = cliente.apellido;
    document.getElementById('cliente-cedula').value = cliente.cedula;
    document.getElementById('cliente-telefono').value = cliente.telefono || '';
    document.getElementById('cliente-email').value = cliente.email || '';
    document.getElementById('cliente-direccion').value = cliente.direccion || '';

    document.getElementById('modal-cliente-title').textContent = 'Editar Cliente';
    openModal('modal-cliente');
}

async function saveCliente(e) {
    e.preventDefault();
    const id = document.getElementById('cliente-id').value;
    const payload = {
        nombre: document.getElementById('cliente-nombre').value.trim(),
        apellido: document.getElementById('cliente-apellido').value.trim(),
        cedula: document.getElementById('cliente-cedula').value.trim(),
        telefono: document.getElementById('cliente-telefono').value.trim() || null,
        email: document.getElementById('cliente-email').value.trim() || null,
        direccion: document.getElementById('cliente-direccion').value.trim() || null,
        estado: true
    };

    const url = id ? `${API_BASE}/clientes/${id}` : `${API_BASE}/clientes`;
    const method = id ? 'PUT' : 'POST';

    try {
        const res = await fetch(url, {
            method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (res.ok) {
            closeModal('modal-cliente');
            showAlert(id ? "Cliente actualizado correctamente." : "Cliente registrado exitosamente.", "success");
            loadClientes();
        } else {
            const errData = await res.json();
            showAlert(errData.message || "Error al procesar la solicitud de cliente.");
        }
    } catch (err) {
        showAlert("Error de conexión con el servidor.");
    }
}

async function deleteCliente(id) {
    if (!confirm("¿Está seguro de que desea eliminar este cliente?")) return;

    try {
        const res = await fetch(`${API_BASE}/clientes/${id}`, { method: 'DELETE' });
        if (res.ok) {
            showAlert("Cliente eliminado exitosamente.", "success");
            loadClientes();
        } else {
            const errData = await res.json();
            showAlert(errData.message || "No se pudo eliminar el cliente.", "danger");
        }
    } catch (err) {
        showAlert("Error al intentar eliminar el cliente.");
    }
}

// PLATOS
async function loadPlatos() {
    try {
        const res = await fetch(`${API_BASE}/platos`);
        if (res.ok) {
            platosState = await res.json();
            renderPlatosTable(platosState);
        }
    } catch (err) {
        showAlert("Error al obtener el listado de platos.");
    }
}

function renderPlatosTable(platos) {
    const tbody = document.querySelector('#table-platos tbody');
    tbody.innerHTML = '';

    platos.forEach(p => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${p.id}</td>
            <td><strong>${p.nombre}</strong></td>
            <td><span class="badge badge-secondary">${p.categoria}</span></td>
            <td>$${p.precio.toFixed(2)}</td>
            <td>${p.descripcion || '-'}</td>
            <td><span class="badge ${p.disponible ? 'badge-success' : 'badge-danger'}">${p.disponible ? 'Disponible' : 'No disponible'}</span></td>
            <td>
                <button class="btn btn-secondary btn-sm" onclick="editPlato(${p.id})"><i class="fa-solid fa-pen"></i></button>
                <button class="btn btn-danger btn-sm" onclick="deletePlato(${p.id})"><i class="fa-solid fa-trash"></i></button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

function openPlatoModal() {
    document.getElementById('form-plato').reset();
    document.getElementById('plato-id').value = '';
    document.getElementById('plato-disponible').checked = true;
    document.getElementById('modal-plato-title').textContent = 'Nuevo Plato';
    openModal('modal-plato');
}

function editPlato(id) {
    const plato = platosState.find(p => p.id === id);
    if (!plato) return;

    document.getElementById('plato-id').value = plato.id;
    document.getElementById('plato-nombre').value = plato.nombre;
    document.getElementById('plato-precio').value = plato.precio;
    document.getElementById('plato-categoria').value = plato.categoria;
    document.getElementById('plato-descripcion').value = plato.descripcion || '';
    document.getElementById('plato-disponible').checked = plato.disponible;

    document.getElementById('modal-plato-title').textContent = 'Editar Plato';
    openModal('modal-plato');
}

async function savePlato(e) {
    e.preventDefault();
    const id = document.getElementById('plato-id').value;
    const payload = {
        nombre: document.getElementById('plato-nombre').value.trim(),
        precio: parseFloat(document.getElementById('plato-precio').value),
        categoria: document.getElementById('plato-categoria').value,
        descripcion: document.getElementById('plato-descripcion').value.trim() || null,
        disponible: document.getElementById('plato-disponible').checked
    };

    const url = id ? `${API_BASE}/platos/${id}` : `${API_BASE}/platos`;
    const method = id ? 'PUT' : 'POST';

    try {
        const res = await fetch(url, {
            method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (res.ok) {
            closeModal('modal-plato');
            showAlert(id ? "Plato actualizado correctamente." : "Plato creado exitosamente.", "success");
            loadPlatos();
        } else {
            const errData = await res.json();
            showAlert(errData.message || "Error al procesar la solicitud de plato.");
        }
    } catch (err) {
        showAlert("Error de conexión con el servidor.");
    }
}

async function deletePlato(id) {
    if (!confirm("¿Está seguro de que desea eliminar/desactivar este plato?")) return;

    try {
        const res = await fetch(`${API_BASE}/platos/${id}`, { method: 'DELETE' });
        if (res.ok) {
            if (res.status === 200) {
                const data = await res.json();
                showAlert(data.message, "warning");
            } else {
                showAlert("Plato eliminado exitosamente.", "success");
            }
            loadPlatos();
        } else {
            const errData = await res.json();
            showAlert(errData.message || "No se pudo eliminar el plato.", "danger");
        }
    } catch (err) {
        showAlert("Error al intentar eliminar el plato.");
    }
}

// MESAS
async function loadMesas() {
    try {
        const res = await fetch(`${API_BASE}/mesas`);
        if (res.ok) {
            mesasState = await res.json();
            renderMesasCards(mesasState);
        }
    } catch (err) {
        showAlert("Error al obtener el listado de mesas.");
    }
}

function renderMesasCards(mesas) {
    const container = document.getElementById('mesas-cards-container');
    container.innerHTML = '';

    mesas.forEach(m => {
        const div = document.createElement('div');
        div.className = `mesa-card estado-${m.estado}`;
        div.innerHTML = `
            <h3>Mesa #${m.numero}</h3>
            <p><i class="fa-solid fa-users"></i> Capacidad: ${m.capacidad} pers.</p>
            <p><i class="fa-solid fa-location-dot"></i> ${m.ubicacion || 'General'}</p>
            <p><span class="badge ${getMesaBadgeClass(m.estado)}">${m.estado}</span></p>
            <div class="mt-3 flex gap-2 justify-center">
                <button class="btn btn-secondary btn-sm" onclick="editMesa(${m.id})"><i class="fa-solid fa-pen"></i> Editar</button>
                <button class="btn btn-danger btn-sm" onclick="deleteMesa(${m.id})"><i class="fa-solid fa-trash"></i></button>
            </div>
        `;
        container.appendChild(div);
    });
}

function openMesaModal() {
    document.getElementById('form-mesa').reset();
    document.getElementById('mesa-id').value = '';
    document.getElementById('modal-mesa-title').textContent = 'Nueva Mesa';
    openModal('modal-mesa');
}

function editMesa(id) {
    const mesa = mesasState.find(m => m.id === id);
    if (!mesa) return;

    document.getElementById('mesa-id').value = mesa.id;
    document.getElementById('mesa-numero').value = mesa.numero;
    document.getElementById('mesa-capacidad').value = mesa.capacidad;
    document.getElementById('mesa-ubicacion').value = mesa.ubicacion || '';
    document.getElementById('mesa-estado').value = mesa.estado;

    document.getElementById('modal-mesa-title').textContent = 'Editar Mesa';
    openModal('modal-mesa');
}

async function saveMesa(e) {
    e.preventDefault();
    const id = document.getElementById('mesa-id').value;
    const payload = {
        numero: parseInt(document.getElementById('mesa-numero').value),
        capacidad: parseInt(document.getElementById('mesa-capacidad').value),
        ubicacion: document.getElementById('mesa-ubicacion').value.trim(),
        estado: document.getElementById('mesa-estado').value
    };

    const url = id ? `${API_BASE}/mesas/${id}` : `${API_BASE}/mesas`;
    const method = id ? 'PUT' : 'POST';

    try {
        const res = await fetch(url, {
            method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (res.ok) {
            closeModal('modal-mesa');
            showAlert(id ? "Mesa actualizada correctamente." : "Mesa creada exitosamente.", "success");
            loadMesas();
        } else {
            const errData = await res.json();
            showAlert(errData.message || "Error al procesar la solicitud de mesa.");
        }
    } catch (err) {
        showAlert("Error de conexión con el servidor.");
    }
}

async function deleteMesa(id) {
    if (!confirm("¿Está seguro de que desea eliminar esta mesa?")) return;

    try {
        const res = await fetch(`${API_BASE}/mesas/${id}`, { method: 'DELETE' });
        if (res.ok) {
            showAlert("Mesa eliminada exitosamente.", "success");
            loadMesas();
        } else {
            const errData = await res.json();
            showAlert(errData.message || "No se pudo eliminar la mesa.", "danger");
        }
    } catch (err) {
        showAlert("Error al intentar eliminar la mesa.");
    }
}

// PEDIDOS
async function loadPedidos() {
    try {
        const res = await fetch(`${API_BASE}/pedidos`);
        if (res.ok) {
            pedidosState = await res.json();
            renderPedidosTable(pedidosState);
        }
    } catch (err) {
        showAlert("Error al obtener los pedidos.");
    }
}

function renderPedidosTable(pedidos) {
    const tbody = document.querySelector('#table-pedidos tbody');
    tbody.innerHTML = '';

    pedidos.forEach(p => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>#${p.id}</td>
            <td><strong>${p.clienteNombreCompleto}</strong> (${p.clienteCedula})</td>
            <td>${p.mesaNumero ? 'Mesa #' + p.mesaNumero : 'N/A'}</td>
            <td>${new Date(p.fechaPedido).toLocaleString()}</td>
            <td><span class="badge ${getBadgeClass(p.estado)}">${p.estado}</span></td>
            <td><strong>$${p.total.toFixed(2)}</strong></td>
            <td>
                <button class="btn btn-primary btn-sm" onclick="verDetallePedido(${p.id})"><i class="fa-solid fa-eye"></i> Detalle</button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

async function openNuevoPedidoModal() {
    orderItemsBuilder = [];
    renderPedidoItemsBuilder();

    try {
        const [cliRes, meRes, plaRes] = await Promise.all([
            fetch(`${API_BASE}/clientes`),
            fetch(`${API_BASE}/mesas`),
            fetch(`${API_BASE}/platos?disponible=true`)
        ]);

        if (cliRes.ok && meRes.ok && plaRes.ok) {
            const clientes = await cliRes.json();
            const mesas = await meRes.json();
            const platos = await plaRes.json();

            // Populate Clientes Select
            const cliSelect = document.getElementById('pedido-cliente');
            cliSelect.innerHTML = '<option value="">-- Seleccionar Cliente --</option>';
            clientes.filter(c => c.estado).forEach(c => {
                cliSelect.innerHTML += `<option value="${c.id}">${c.nombre} ${c.apellido} - ${c.cedula}</option>`;
            });

            // Populate Mesas Select
            const meSelect = document.getElementById('pedido-mesa');
            meSelect.innerHTML = '<option value="">-- Sin Mesa (Para llevar) --</option>';
            mesas.filter(m => m.estado === 'Disponible').forEach(m => {
                meSelect.innerHTML += `<option value="${m.id}">Mesa #${m.numero} (${m.ubicacion || 'General'} - ${m.capacidad} pers.)</option>`;
            });

            // Populate Platos Select
            const plaSelect = document.getElementById('select-plato-item');
            plaSelect.innerHTML = '<option value="">-- Seleccionar Plato --</option>';
            platos.forEach(p => {
                plaSelect.innerHTML += `<option value="${p.id}" data-precio="${p.precio}">${p.nombre} - $${p.precio.toFixed(2)}</option>`;
            });

            openModal('modal-pedido');
        }
    } catch (err) {
        showAlert("Error al preparar datos para el nuevo pedido.");
    }
}

function agregarItemPedido() {
    const selectPlato = document.getElementById('select-plato-item');
    const platoId = parseInt(selectPlato.value);
    const cantidad = parseInt(document.getElementById('input-cantidad-item').value);

    if (!platoId || cantidad <= 0) {
        alert("Por favor seleccione un plato y una cantidad válida.");
        return;
    }

    const selectedOption = selectPlato.options[selectPlato.selectedIndex];
    const nombre = selectedOption.text.split(' - ')[0];
    const precio = parseFloat(selectedOption.getAttribute('data-precio'));

    const existingIndex = orderItemsBuilder.findIndex(i => i.platoId === platoId);
    if (existingIndex >= 0) {
        orderItemsBuilder[existingIndex].cantidad += cantidad;
        orderItemsBuilder[existingIndex].subtotal = orderItemsBuilder[existingIndex].cantidad * precio;
    } else {
        orderItemsBuilder.push({
            platoId,
            nombre,
            precioUnitario: precio,
            cantidad,
            subtotal: precio * cantidad
        });
    }

    renderPedidoItemsBuilder();
}

function removePedidoItem(index) {
    orderItemsBuilder.splice(index, 1);
    renderPedidoItemsBuilder();
}

function renderPedidoItemsBuilder() {
    const tbody = document.querySelector('#table-pedido-items tbody');
    tbody.innerHTML = '';
    let grandTotal = 0;

    orderItemsBuilder.forEach((item, idx) => {
        grandTotal += item.subtotal;
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${item.nombre}</td>
            <td>$${item.precioUnitario.toFixed(2)}</td>
            <td>${item.cantidad}</td>
            <td>$${item.subtotal.toFixed(2)}</td>
            <td><button type="button" class="btn btn-danger btn-sm" onclick="removePedidoItem(${idx})">&times;</button></td>
        `;
        tbody.appendChild(tr);
    });

    document.getElementById('pedido-summary-subtotal').textContent = `$${grandTotal.toFixed(2)}`;
    document.getElementById('pedido-summary-total').textContent = `$${grandTotal.toFixed(2)}`;
}

async function savePedido(e) {
    e.preventDefault();

    const clienteId = parseInt(document.getElementById('pedido-cliente').value);
    const mesaIdVal = document.getElementById('pedido-mesa').value;
    const mesaId = mesaIdVal ? parseInt(mesaIdVal) : null;
    const observaciones = document.getElementById('pedido-observaciones').value.trim();

    if (!clienteId) {
        showAlert("Debe seleccionar un cliente.");
        return;
    }

    if (orderItemsBuilder.length === 0) {
        showAlert("Debe agregar al menos un plato al pedido.");
        return;
    }

    const payload = {
        clienteId,
        mesaId,
        observaciones: observaciones || null,
        estado: 'Pendiente',
        detalles: orderItemsBuilder.map(item => ({
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

        if (res.ok) {
            closeModal('modal-pedido');
            showAlert("Pedido registrado con éxito.", "success");
            loadPedidos();
        } else {
            const errData = await res.json();
            showAlert(errData.message || "Error al registrar el pedido.");
        }
    } catch (err) {
        showAlert("Error de comunicación al guardar el pedido.");
    }
}

async function verDetallePedido(id) {
    try {
        const res = await fetch(`${API_BASE}/pedidos/${id}`);
        if (res.ok) {
            const p = await res.json();
            document.getElementById('detalle-pedido-title').textContent = `Detalle del Pedido #${p.id}`;

            let detallesHtml = p.detalles.map(d => `
                <tr>
                    <td>${d.platoNombre}</td>
                    <td>$${d.precioUnitario.toFixed(2)}</td>
                    <td>${d.cantidad}</td>
                    <td>$${d.subtotal.toFixed(2)}</td>
                </tr>
            `).join('');

            document.getElementById('detalle-pedido-body').innerHTML = `
                <div class="mb-3">
                    <p><strong>Cliente:</strong> ${p.clienteNombreCompleto} (${p.clienteCedula})</p>
                    <p><strong>Mesa:</strong> ${p.mesaNumero ? 'Mesa #' + p.mesaNumero : 'N/A'}</p>
                    <p><strong>Fecha:</strong> ${new Date(p.fechaPedido).toLocaleString()}</p>
                    <p><strong>Estado Actual:</strong> <span class="badge ${getBadgeClass(p.estado)}">${p.estado}</span></p>
                    ${p.observaciones ? `<p><strong>Observaciones:</strong> ${p.observaciones}</p>` : ''}
                </div>
                <h4>Platos Solicitados</h4>
                <div class="table-responsive">
                    <table class="table">
                        <thead>
                            <tr><th>Plato</th><th>Precio Unit.</th><th>Cantidad</th><th>Subtotal</th></tr>
                        </thead>
                        <tbody>${detallesHtml}</tbody>
                    </table>
                </div>
                <div class="order-summary mt-3">
                    <div class="summary-line total-line">
                        <span>Total del Pedido:</span>
                        <span>$${p.total.toFixed(2)}</span>
                    </div>
                </div>

                <div class="mt-4 pt-3 border-top">
                    <label><strong>Cambiar Estado del Pedido:</strong></label>
                    <div class="flex gap-2 mt-2">
                        <button class="btn btn-secondary btn-sm" onclick="cambiarEstadoPedido(${p.id}, 'En preparación')">En preparación</button>
                        <button class="btn btn-primary btn-sm" onclick="cambiarEstadoPedido(${p.id}, 'Servido')">Servido</button>
                        <button class="btn btn-success btn-sm" onclick="cambiarEstadoPedido(${p.id}, 'Pagado')">Pagado</button>
                        <button class="btn btn-danger btn-sm" onclick="cambiarEstadoPedido(${p.id}, 'Cancelado')">Cancelar</button>
                    </div>
                </div>
            `;

            openModal('modal-detalle-pedido');
        }
    } catch (err) {
        showAlert("Error al cargar el detalle del pedido.");
    }
}

async function cambiarEstadoPedido(id, nuevoEstado) {
    try {
        const res = await fetch(`${API_BASE}/pedidos/${id}/estado`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ estado: nuevoEstado })
        });

        if (res.ok) {
            closeModal('modal-detalle-pedido');
            showAlert(`El pedido #${id} cambió su estado a '${nuevoEstado}'.`, "success");
            loadPedidos();
        } else {
            const errData = await res.json();
            showAlert(errData.message || "Error al actualizar el estado del pedido.");
        }
    } catch (err) {
        showAlert("Error al enviar la solicitud.");
    }
}

// REPORTES
async function loadReportesClientesSelect() {
    try {
        const res = await fetch(`${API_BASE}/clientes`);
        if (res.ok) {
            const clientes = await res.json();
            const select = document.getElementById('select-reporte-cliente');
            select.innerHTML = '<option value="">-- Seleccionar Cliente --</option>';
            clientes.forEach(c => {
                select.innerHTML += `<option value="${c.id}">${c.nombre} ${c.apellido} - ${c.cedula}</option>`;
            });
        }
    } catch (err) {
        console.error("Error al obtener clientes para el reporte.");
    }
}

async function generarReporteCliente() {
    const clienteId = document.getElementById('select-reporte-cliente').value;
    if (!clienteId) {
        showAlert("Por favor seleccione un cliente de la lista.");
        return;
    }

    try {
        const res = await fetch(`${API_BASE}/reportes/pedidos-por-cliente/${clienteId}`);
        if (res.ok) {
            const rep = await res.json();

            document.getElementById('rep-cliente-nombre').textContent = `${rep.nombreCliente} ${rep.apellidoCliente}`;
            document.getElementById('rep-cliente-cedula').textContent = rep.cedulaCliente;
            document.getElementById('rep-cant-pedidos').textContent = rep.cantidadPedidos;
            document.getElementById('rep-total-gastado').textContent = `$${rep.totalGastado.toFixed(2)}`;

            const tbody = document.querySelector('#table-reporte-pedidos tbody');
            tbody.innerHTML = '';

            rep.pedidos.forEach(p => {
                const platosResumen = p.detalles.map(d => `${d.cantidad}x ${d.platoNombre} ($${d.precioUnitario.toFixed(2)})`).join(', ');
                const tr = document.createElement('tr');
                tr.innerHTML = `
                    <td>#${p.id}</td>
                    <td>${new Date(p.fechaPedido).toLocaleString()}</td>
                    <td>${p.mesaNumero ? 'Mesa #' + p.mesaNumero : 'N/A'}</td>
                    <td><span class="badge ${getBadgeClass(p.estado)}">${p.estado}</span></td>
                    <td>${platosResumen}</td>
                    <td><strong>$${p.total.toFixed(2)}</strong></td>
                `;
                tbody.appendChild(tr);
            });

            document.getElementById('reporte-resultado-container').style.display = 'block';
        } else {
            showAlert("No se pudo obtener el reporte del cliente seleccionado.");
        }
    } catch (err) {
        showAlert("Error al cargar la información del reporte.");
    }
}

// UTILS & FILTERS
function getBadgeClass(estado) {
    switch (estado) {
        case 'Pendiente': return 'badge-warning';
        case 'En preparación': return 'badge-info';
        case 'Servido': return 'badge-primary';
        case 'Pagado': return 'badge-success';
        case 'Cancelado': return 'badge-danger';
        default: return 'badge-secondary';
    }
}

function getMesaBadgeClass(estado) {
    switch (estado) {
        case 'Disponible': return 'badge-success';
        case 'Ocupada': return 'badge-danger';
        case 'Reservada': return 'badge-warning';
        case 'Mantenimiento': return 'badge-secondary';
        default: return 'badge-secondary';
    }
}

function initSearchFilters() {
    // Search Clientes
    const searchCliente = document.getElementById('search-cliente');
    if (searchCliente) {
        searchCliente.addEventListener('input', (e) => {
            const term = e.target.value.toLowerCase();
            const filtered = clientesState.filter(c =>
                c.nombre.toLowerCase().includes(term) ||
                c.apellido.toLowerCase().includes(term) ||
                c.cedula.includes(term)
            );
            renderClientesTable(filtered);
        });
    }

    // Search Platos
    const searchPlato = document.getElementById('search-plato');
    if (searchPlato) {
        searchPlato.addEventListener('input', (e) => {
            const term = e.target.value.toLowerCase();
            const filtered = platosState.filter(p =>
                p.nombre.toLowerCase().includes(term) ||
                p.categoria.toLowerCase().includes(term)
            );
            renderPlatosTable(filtered);
        });
    }
}
