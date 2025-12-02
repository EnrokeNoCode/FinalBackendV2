using Microsoft.AspNetCore.Mvc;
using Model.DTO.Compras.NotaCreditoCompra;
using Service.Compra;

namespace Controller.Compras
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

        [HttpGet("lista/{codsucursal}")]
        public async Task<ActionResult<List<NotaCreditoListDTO>>> GetList(int codsucursal, int page = 1, int pageSize = 10)
        {
            var comprasList = await _data.NotaCreditoCompraList(page, pageSize, codsucursal);

            if (comprasList == null)
            {
                return NotFound();
            }
            return Ok(comprasList);
        }

        [HttpPost("insertar")]
        public async Task<IActionResult> InsertarNotaCreditoCompra([FromBody] NotaCreditoInsertDTO nccompra)
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

        [HttpPut("anular/{codnotacredito}/{codestado}")]
        public async Task<ActionResult> PutActualizarEstado(int codnotacredito, int codestado)
        {
            try
            {
                string filasAfectadas = await _data.ActualizarEstadoV2(codnotacredito, codestado);

                if (filasAfectadas.StartsWith("OK"))
                {
                    return Ok(new { message = filasAfectadas });
                }
                else if (filasAfectadas.StartsWith("ERROR"))
                {
                    return BadRequest(new { message = filasAfectadas });
                }
                else
                {
                    return StatusCode(500, new { message = "Respuesta inesperada: " + filasAfectadas });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }

        }

    }
}
