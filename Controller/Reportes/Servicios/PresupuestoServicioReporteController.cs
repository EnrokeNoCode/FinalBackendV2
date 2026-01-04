using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using Reports.Servicios;
using Service.Reportes.Servicios;

namespace Controller.Reportes.Servicios
{
    [ApiController]
    [Route("api/reportes/presupuestoservicio")]
    public class PresupuestoServicioReporteController : ControllerBase
    {
        private readonly PresupuestoServicioReporteService _service;

        public PresupuestoServicioReporteController(PresupuestoServicioReporteService service)
        {
            _service = service;
        }

        [HttpGet("presupuesto/pdf/{codpresupuesto}")]
        public async Task<IActionResult> GenerarPresupuestoPdf(int codpresupuesto)
        {
            try
            {
                var presupuesto = await _service.ObtenerPresupuestoServicio(codpresupuesto);

                if (presupuesto == null || presupuesto.codpresupuesto == 0)
                    return NotFound("No se encontró el presupuesto indicado.");

                var pdf = new PresupuestoServicioReport(presupuesto);
                var bytes = pdf.GeneratePdf();

                return File(bytes, "application/pdf", $"presupuesto_{presupuesto.nropresupuesto}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generando PDF: {ex.Message}");
            }
        }


    }
}
