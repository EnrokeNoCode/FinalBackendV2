using Microsoft.AspNetCore.Mvc;
using Model.DTO.Ventas.PedidoVenta;
using Service.Venta;

namespace Controller.Venta
{
    [ApiController]
    [Route("api/pedidoventa")]

    public class PedidoVentaController : ControllerBase
    {
        private readonly PedidoVentaService _data;
        public PedidoVentaController(PedidoVentaService data)
        {
            _data = data;
        }

        [HttpGet("lista")]
        public async Task<ActionResult<List<PedidoVentaListDTO>>> GetList(int page = 1, int pageSize = 10)
        {
            var pedidoventa = await _data.PedidoVentaLista(page, pageSize);

            if (pedidoventa == null)
            {
                return NotFound();
            }
            return Ok(pedidoventa);
        }

        [HttpGet("lista/{codcliente}")]
        public async Task<ActionResult<PedidoVentaListDTO>> GetList(int codcliente)
        {
            var pedidoventa = await _data.PedidoVentaLista(codcliente);

            if (pedidoventa == null)
            {
                return NotFound();
            }
            return Ok(pedidoventa);
        }

        [HttpGet("recpedventa/{codpedidov}")]
        public async Task<ActionResult<PedidoVentaDTO>> Get(int codpedidov)
        {
            var pedido = await _data.PedidoVentaConDet(codpedidov);

            if (pedido == null)
                return NotFound(pedido);

            return Ok(pedido);
        }

        [HttpPut("anularpedidoventa/{codpedidov}/{codestado}")]
        public async Task<ActionResult> PutActualizarEstado(int codpedidov, int codestado)
        {
            try
            {
                string filasAfectadas = await _data.ActualizarEstadoV2(codpedidov, codestado);

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

        [HttpPost("insertar")]
        public async Task<IActionResult> InsertarPedidoCompra([FromBody] PedidoVentaInsertDTO pedido)
        {
            try
            {
                int newPedidoVenta = await _data.InsertarPedidoVenta(pedido);
                return Ok(new { mensaje = "Insertado correctamente", id = newPedidoVenta });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPut("actualizarpedidodet")]
        public async Task<ActionResult> ActualizarPedVentaDet([FromBody] PedidoVentaUpdateDTO pedido)
        {
            try
            {
                string updateDatos = await _data.ActualizarPedidoVentaDet(pedido);
                if (updateDatos.StartsWith("OK"))
                {
                    return Ok(new { message = updateDatos });
                }
                else if (updateDatos.StartsWith("ERROR"))
                {
                    return BadRequest(new { message = updateDatos });
                }
                else
                {
                    return StatusCode(500, new { message = "Respuesta inesperada: " + updateDatos });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno: " + ex.Message });
            }
        }
    }
}