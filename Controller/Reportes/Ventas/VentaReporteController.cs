using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using Reports.Ventas;
using Service.Reportes.Ventas;

namespace Controller.Reportes.Ventas
{
    [ApiController]
    [Route("api/reportes/ventas")]
    public class VentaReporteController : ControllerBase
    {
        private readonly VentaReporteService _service;

        public VentaReporteController(VentaReporteService service)
        {
            _service = service;
        }

        [HttpGet("listado/pdf")]
        public async Task<IActionResult> ReporteCompras(
            [FromQuery] bool detalle = false,
            [FromQuery] string? nrodoc = null,
            [FromQuery] DateTime? fechainicio = null,
            [FromQuery] DateTime? fechafin = null,
            [FromQuery] int? codsucursal = null)
        {
            try
            {
                var data = await _service.ObtenerReporteVenta(
                    incluirDetalle: detalle,
                    nroDocC: nrodoc,
                    fechaInicio: fechainicio,
                    fechaFin: fechafin,
                    codSucursal: codsucursal
                );

                if (data == null || data.Count == 0)
                    return NotFound("No existen ventas con los filtros indicados.");

                var pdf = new VentaReports(data, detalle);
                var bytes = pdf.GeneratePdf();

                return File(bytes, "application/pdf", "reporte_ventas.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generando reporte: {ex.Message}");
            }
        }
    }
}
