using Microsoft.AspNetCore.Mvc;
using Reports.CajaReports;
using QuestPDF.Fluent;
using Service.Reportes.Referenciales;
using Reports.Referenciales;
using Model.Reportes.Caja;

namespace Controllers.Reportes.Caja
{
    [ApiController]
    [Route("api/reportes/caja")]
    public class CajaReporteController : ControllerBase
    {
        private readonly CajaReporteService _service;

        public CajaReporteController(CajaReporteService service)
        {
            _service = service;
        }

        [HttpGet("cobros/pdf/{codGestion}")]
        public async Task<IActionResult> ReporteCobros(int codGestion)
        {
            if (codGestion <= 0)
                return BadRequest("El código de gestión no es válido.");

            var reporte = await _service.ObtenerReporteCaja(codGestion);

            if (reporte == null || reporte.cajaformacobro.Count == 0)
                return NotFound("No existen cobros para la gestión indicada.");

            var pdf = new CajaReports(reporte);
            var bytes = pdf.GeneratePdf();

            return File(bytes, "application/pdf", $"cobros_caja_{codGestion}.pdf");
        }

        [HttpGet("cajadetallecobros/pdf/{codGestion}")]
        public async Task<IActionResult> ReporteCajaDetalleCobros(int codGestion)
        {
            if (codGestion <= 0)
                return BadRequest(new { message = "El código de gestión no es válido." });

            var reporte = await _service.ObtenerReporteCajaGestionCobroDetalle(codGestion);

            if (reporte == null || !reporte.Any())
                return NotFound(new { message = "No existen cobros para la gestión indicada." });

            var pdf = new CajaGestionCobrosDetalleReport(reporte);
            var bytes = pdf.GeneratePdf();

            return File(
                bytes,
                "application/pdf",
                $"cobros_caja_{codGestion}.pdf"
            );
        }


    }
}
