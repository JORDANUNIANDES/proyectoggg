using HomeAutomation.BusinessLogic.SignalR;
using HomeAutomation.DataAccess.Entities;
using HomeAutomation.DataAccess.Repositories;
using Microsoft.Extensions.Logging; // Para logging
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeAutomation.BusinessLogic.Services.Devices
{
    public class DeviceService : IDeviceService
    {
        private readonly IDeviceRepository _deviceRepository;
        private readonly IZoneRepository _zoneRepository;
        private readonly SignalRClient _signalRClient;
        private readonly ILogger<DeviceService> _logger;

        public DeviceService(
            IDeviceRepository deviceRepository,
            IZoneRepository zoneRepository,
            SignalRClient signalRClient,
            ILogger<DeviceService> logger)
        {
            _deviceRepository = deviceRepository ?? throw new ArgumentNullException(nameof(deviceRepository));
            _zoneRepository = zoneRepository ?? throw new ArgumentNullException(nameof(zoneRepository));
            _signalRClient = signalRClient ?? throw new ArgumentNullException(nameof(signalRClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Device> GetDeviceByIdAsync(int deviceId)
        {
            _logger.LogInformation("Obteniendo dispositivo por ID: {DeviceId}", deviceId);
            return await _deviceRepository.GetByIdAsync(deviceId);
        }

        public async Task<List<Device>> GetAllDevicesAsync()
        {
            _logger.LogInformation("Obteniendo todos los dispositivos.");
            // En una aplicación real, aquí se aplicarían filtros basados en el usuario/rol.
            return await _deviceRepository.GetAllAsync();
        }
        public async Task<List<Device>> GetDevicesByUserIdAsync(int userId)
        {
            _logger.LogInformation("Obteniendo dispositivos para el usuario ID: {UserId}", userId);
            return await _deviceRepository.GetDevicesByUserIdAsync(userId);
        }

        public async Task<Device> AddDeviceAsync(Device device, int userId)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (string.IsNullOrWhiteSpace(device.Name)) throw new ArgumentException("El nombre del dispositivo no puede estar vacío.", nameof(device.Name));

            device.UserId = userId; // Asignar el dueño
            device.Status = DeviceStatus.Unavailable; // Estado inicial por defecto

            _logger.LogInformation("Agregando nuevo dispositivo: {DeviceName} para el usuario ID: {UserId}", device.Name, userId);
            await _deviceRepository.AddAsync(device);
            return device; // device ahora tendrá el DeviceId asignado por la BD
        }

        public async Task UpdateDeviceAsync(Device device)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));
            // Aquí se podrían añadir validaciones de permisos si el usuario que edita debe ser el dueño o admin.
            _logger.LogInformation("Actualizando dispositivo ID: {DeviceId}", device.DeviceId);
            await _deviceRepository.UpdateAsync(device);
        }

        public async Task DeleteDeviceAsync(int deviceId)
        {
            // Aquí se podrían añadir validaciones de permisos.
            _logger.LogInformation("Eliminando dispositivo ID: {DeviceId}", deviceId);
            await _deviceRepository.DeleteAsync(deviceId);
        }

        public async Task AssignDeviceToZoneAsync(int deviceId, int? zoneId)
        {
            var device = await _deviceRepository.GetByIdAsync(deviceId);
            if (device == null)
            {
                _logger.LogWarning("Intento de asignar zona a dispositivo no existente ID: {DeviceId}", deviceId);
                throw new KeyNotFoundException("Dispositivo no encontrado.");
            }

            if (zoneId.HasValue && zoneId.Value != 0)
            {
                var zone = await _zoneRepository.GetByIdAsync(zoneId.Value);
                if (zone == null)
                {
                    _logger.LogWarning("Intento de asignar dispositivo ID: {DeviceId} a zona no existente ID: {ZoneId}", deviceId, zoneId.Value);
                    throw new KeyNotFoundException("Zona no encontrada.");
                }
                device.ZoneId = zoneId.Value;
                _logger.LogInformation("Asignando dispositivo ID: {DeviceId} a Zona ID: {ZoneId}", deviceId, zoneId.Value);
            }
            else
            {
                device.ZoneId = null; // Desasignar de zona
                _logger.LogInformation("Desasignando dispositivo ID: {DeviceId} de cualquier zona.", deviceId);
            }
            await _deviceRepository.UpdateAsync(device);
        }

        public async Task SendCommandAsync(int deviceId, string command, string value)
        {
            var device = await _deviceRepository.GetByIdAsync(deviceId);
            if (device == null)
            {
                _logger.LogWarning("Intento de enviar comando a dispositivo no existente ID: {DeviceId}", deviceId);
                throw new KeyNotFoundException($"Dispositivo con ID {deviceId} no encontrado.");
            }

            _logger.LogInformation("Enviando comando '{Command}' con valor '{Value}' al dispositivo ID: {DeviceId} ({DeviceName}) vía SignalR.", command, value, deviceId, device.Name);
            await _signalRClient.SendCommandToServerAsync(device.DeviceId.ToString(), command, value);
        }

        public async Task HandleDeviceStatusUpdateAsync(string deviceIdStr, string command, string newStatusStr)
        {
            _logger.LogInformation("Manejando actualización de estado desde SignalR para DeviceId: {DeviceId}, Comando: {Command}, Nuevo Estado: {NewStatus}", deviceIdStr, command, newStatusStr);
            if (int.TryParse(deviceIdStr, out int deviceId))
            {
                var device = await _deviceRepository.GetByIdAsync(deviceId);
                if (device != null)
                {
                    if (Enum.TryParse<DeviceStatus>(newStatusStr, true, out DeviceStatus statusEnum))
                    {
                        if (device.Status != statusEnum) // Solo actualizar si el estado cambió
                        {
                            device.Status = statusEnum;
                            await _deviceRepository.UpdateAsync(device);
                            _logger.LogInformation("Estado del dispositivo {DeviceId} actualizado a {NewStatus} en la BD.", deviceId, newStatusStr);
                        }
                        else
                        {
                            _logger.LogInformation("Estado del dispositivo {DeviceId} ya era {CurrentStatus}. No se requiere actualización en BD.", deviceId, device.Status);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No se pudo parsear el estado '{NewStatus}' para el dispositivo {DeviceId} recibido de SignalR.", newStatusStr, deviceId);
                    }
                }
                else
                {
                     _logger.LogWarning("Se recibió actualización de estado para dispositivo no existente ID: {DeviceId} desde SignalR.", deviceId);
                }
            }
            else
            {
                _logger.LogWarning("No se pudo parsear DeviceId '{DeviceIdStr}' recibido de SignalR.", deviceIdStr);
            }
        }
    }
}
