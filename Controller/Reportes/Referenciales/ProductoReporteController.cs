using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using Reports.CajaReports;
using Reports.Referenciales;
using Service.Reportes.Referenciales;

namespace Controller.Reportes.Referenciales
{
    [ApiController]
    [Route("api/reportes/producto")]
    public class ProductoReporteController : ControllerBase
    {
        private readonly ProductoReporteService _service;

        public ProductoReporteController(ProductoReporteService service)
        {
            _service = service;
        }

        [HttpGet("listado/pdf")]
        public async Task<IActionResult> ReporteProductoListado()
        {

            var reporte = await _service.ObtenerListadoProductos();

            if (reporte == null)
                return NotFound("");

            var pdf = new ProductoReports(reporte);
            var bytes = pdf.GeneratePdf();

            return File(bytes, "application/pdf", "reporte_listadogeneral_producto.pdf");
        }
    }
}
