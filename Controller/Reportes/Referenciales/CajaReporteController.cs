using Microsoft.AspNetCore.Mvc;
using Service.Reportes;
using Reports.CajaReports;
using QuestPDF.Fluent;

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

        [HttpGet("cobros/pdf/{codGestion:int}")]
        public async Task<IActionResult> ReporteCobros(int codGestion)
        {
            if (codGestion <= 0)
                return BadRequest("El código de gestión no es válido.");

            var data = await _service.ObtenerCobrosPorGestion(codGestion);

            if (data == null || data.Count == 0)
                return NotFound("No existen cobros para la gestión indicada.");

            var pdf = new CajaReports(data, codGestion);
            var bytes = pdf.GeneratePdf();

            return File(
                bytes,
                "application/pdf",
                $"cobros_caja_{codGestion}.pdf"
            );
        }
    }
}
