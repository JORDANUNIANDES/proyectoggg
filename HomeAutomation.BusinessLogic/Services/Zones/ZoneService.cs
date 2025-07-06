using HomeAutomation.DataAccess.Entities;
using HomeAutomation.DataAccess.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq; // Para .Any()
using System.Threading.Tasks;

namespace HomeAutomation.BusinessLogic.Services.Zones
{
    public class ZoneService : IZoneService
    {
        private readonly IZoneRepository _zoneRepository;
        private readonly IDeviceRepository _deviceRepository; // Para verificar si una zona tiene dispositivos
        private readonly ILogger<ZoneService> _logger;

        public ZoneService(
            IZoneRepository zoneRepository,
            IDeviceRepository deviceRepository,
            ILogger<ZoneService> logger)
        {
            _zoneRepository = zoneRepository ?? throw new ArgumentNullException(nameof(zoneRepository));
            _deviceRepository = deviceRepository ?? throw new ArgumentNullException(nameof(deviceRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Zone> GetZoneByIdAsync(int zoneId)
        {
            _logger.LogInformation("Obteniendo zona por ID: {ZoneId}", zoneId);
            return await _zoneRepository.GetByIdAsync(zoneId);
        }

        public async Task<List<Zone>> GetAllZonesAsync()
        {
            _logger.LogInformation("Obteniendo todas las zonas.");
            return await _zoneRepository.GetAllAsync();
        }

        public async Task<Zone> AddZoneAsync(Zone zone)
        {
            if (zone == null) throw new ArgumentNullException(nameof(zone));
            if (string.IsNullOrWhiteSpace(zone.Name)) throw new ArgumentException("El nombre de la zona no puede estar vacío.", nameof(zone.Name));

            // Validar unicidad del nombre de la zona (si es un requisito)
            var existingZone = await _zoneRepository.GetByNameAsync(zone.Name);
            if (existingZone != null)
            {
                _logger.LogWarning("Intento de agregar zona con nombre duplicado: {ZoneName}", zone.Name);
                throw new InvalidOperationException($"Ya existe una zona con el nombre '{zone.Name}'.");
            }

            _logger.LogInformation("Agregando nueva zona: {ZoneName}", zone.Name);
            await _zoneRepository.AddAsync(zone);
            return zone; // zone ahora tendrá el ZoneId
        }

        public async Task UpdateZoneAsync(Zone zone)
        {
            if (zone == null) throw new ArgumentNullException(nameof(zone));
            if (string.IsNullOrWhiteSpace(zone.Name)) throw new ArgumentException("El nombre de la zona no puede estar vacío.", nameof(zone.Name));

            var existingZoneWithSameName = await _zoneRepository.GetByNameAsync(zone.Name);
            if (existingZoneWithSameName != null && existingZoneWithSameName.ZoneId != zone.ZoneId)
            {
                _logger.LogWarning("Intento de actualizar zona ID {ZoneId} con nombre duplicado: {ZoneName}", zone.ZoneId, zone.Name);
                throw new InvalidOperationException($"Ya existe otra zona con el nombre '{zone.Name}'.");
            }

            _logger.LogInformation("Actualizando zona ID: {ZoneId}", zone.ZoneId);
            await _zoneRepository.UpdateAsync(zone);
        }

        public async Task<bool> DeleteZoneAsync(int zoneId)
        {
            _logger.LogInformation("Intentando eliminar zona ID: {ZoneId}", zoneId);
            // Verificar si la zona tiene dispositivos asignados
            var devicesInZone = await _deviceRepository.GetDevicesByZoneIdAsync(zoneId);
            if (devicesInZone != null && devicesInZone.Any())
            {
                _logger.LogWarning("No se puede eliminar la zona ID: {ZoneId} porque contiene dispositivos.", zoneId);
                // Podrías lanzar una excepción específica o devolver un mensaje.
                // throw new InvalidOperationException("No se puede eliminar la zona porque contiene dispositivos. Desasígnelos primero.");
                return false; // Indicar que no se eliminó
            }

            await _zoneRepository.DeleteAsync(zoneId);
            _logger.LogInformation("Zona ID: {ZoneId} eliminada.", zoneId);
            return true; // Indicar que se eliminó
        }
    }
}
