using Microsoft.AspNetCore.Mvc;
using Model.DTO.Compras.NotaCreditoCompra;
using Service.Compra;

namespace Controller
{
    [ApiController]
    [Route("api/notacreditocompra")]
    public class NotaCreditoCompraController : ControllerBase
    {
        private readonly NotaCreditoCompraService _data;

        public NotaCreditoCompraController(NotaCreditoCompraService data)
        {
            _data = data;
        }

        [HttpGet]
        [Route("lista")]
        public async Task<ActionResult<List<NotaCreditoListDTO>>> GetList(int page = 1, int pageSize = 10)
        {
            var comprasList = await _data.NotaCreditoCompraList(page, pageSize);

            if (comprasList == null)
            {
                return NotFound();
            }
            return Ok(comprasList);
        }

        [HttpPost("insertar")]
        public async Task<IActionResult> InsertarCompra([FromBody] NotaCreditoInsertDTO nccompra)
        {
            try
            {
                int newNCCompraId = await _data.InsertarNotaCreditoCompra(nccompra);
                return Ok(new { mensaje = "Nota Credito insertada correctamente", id = newNCCompraId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("recnotacredito/{codnotacredito}")]
        public async Task<ActionResult<NotaCreditoDTO>> Get(int codnotacredito)
        {
            var notaCredito = await _data.NotaCreditoVer(codnotacredito);

            if (notaCredito == null)
                return NotFound();

            return Ok(notaCredito);
        }

    }
}
