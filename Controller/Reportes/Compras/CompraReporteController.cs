using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using Reports.ComprasReports;
using Service.Reportes.Compras;

namespace Controller.Reportes.Compras
{
    [ApiController]
    [Route("api/reportes/compras")]
    public class CompraReporteController : ControllerBase
    {
        private readonly CompraReporteService _service;

        public CompraReporteController(CompraReporteService service)
        {
            _service = service;
        }

        [HttpGet("listado/pdf")]
        public async Task<IActionResult> ReporteCompras(
            [FromQuery] bool detalle = false,
            [FromQuery] string? nrodocprv = null,
            [FromQuery] DateTime? fechainicio = null,
            [FromQuery] DateTime? fechafin = null,
            [FromQuery] int? codsucursal = null)
        {
            try
            {
                var data = await _service.ObtenerReporteCompras(
                    incluirDetalle: detalle,
                    nroDocPrv: nrodocprv,
                    fechaInicio: fechainicio,
                    fechaFin: fechafin,
                    codSucursal: codsucursal
                );

                if (data == null || data.Count == 0)
                    return NotFound("No existen compras con los filtros indicados.");

                var pdf = new CompraReports(data, detalle);
                var bytes = pdf.GeneratePdf();

                return File(bytes, "application/pdf", "reporte_compras.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generando reporte: {ex.Message}");
            }
        }
    }
}
