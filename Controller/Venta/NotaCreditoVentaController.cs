using Microsoft.AspNetCore.Mvc;
using Model.DTO.Ventas.NotaCreditoVenta;
using Service.Venta;

namespace Controller.Venta
{
    [ApiController]
    [Route("api/notacreditoventa")]
    public class NotaCreditoVentaController : ControllerBase
    {
        private readonly NotaCreditoVentaService _data;

        public NotaCreditoVentaController(NotaCreditoVentaService data)
        {
            _data = data;
        }

        [HttpGet("lista")]
        public async Task<ActionResult<List<NotaCreditoVentaListDTO>>> GetList(int page = 1, int pageSize = 10)
        {
            var comprasList = await _data.NotaCreditoVentaList(page, pageSize);

            if (comprasList == null)
            {
                return NotFound();
            }
            return Ok(comprasList);
        }

        [HttpGet("recnotacredito/{codnotacredito}")]
        public async Task<ActionResult<NotaCreditoVentaDTO>> Get(int codnotacredito)
        {
            var notaCredito = await _data.NotaCreditoVer(codnotacredito);

            if (notaCredito == null)
                return NotFound();

            return Ok(notaCredito);
        }

        [HttpPost("insertar")]
        public async Task<IActionResult> InsertarCompra([FromBody] NotaCreditoVentaInsertDTO ncventa)
        {
            try
            {
                int newNCVentaId = await _data.InsertarNotaCreditoVentas(ncventa);
                return Ok(new { mensaje = "Nota Credito insertada correctamente", id = newNCVentaId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
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
