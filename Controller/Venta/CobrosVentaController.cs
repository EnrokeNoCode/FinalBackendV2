using Microsoft.AspNetCore.Mvc;
using Model.DTO.CobroVenta;
using Service.Venta;

namespace Controller.Venta
{
    [ApiController]
    [Route("api/cobrosventa")]
    public class CobrosVentaController : ControllerBase
    {
        private readonly CobrosVentaService _data;
        public CobrosVentaController(CobrosVentaService data)
        {
            _data = data;
        }
        
        [HttpPost("insertar/cobros/contado")]
        public async Task<IActionResult> InsertarCobros([FromBody] CobroVentaContadoDTO venta)
        {
            try
            {
                string newVentaId = await _data.InsertarCobros(venta);
                return Ok(new { mensaje = newVentaId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}