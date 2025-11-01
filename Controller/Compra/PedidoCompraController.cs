using Microsoft.AspNetCore.Mvc;
using Model.DTO.Compras.PedidoCompra;
using Service.Compra;

namespace Controller
{

    [ApiController]
    [Route("api/pedidocompra")]

    public class PedidoCompraController : ControllerBase
    {
        private readonly PedidoCompraService _data;
        public PedidoCompraController(PedidoCompraService data)
        {
            _data = data;
        }

        [HttpGet]
        [Route("lista")]
        public async Task<ActionResult<List<PedidoCompraListDTO>>> GetList(int page = 1, int pageSize = 10)
        {
            var resultado = await _data.PedidoCompraLista(page, pageSize);

            if (resultado == null || resultado.Data == null || !resultado.Data.Any())
            {
                return NotFound();
            }

            return Ok(resultado);
        }

        [HttpGet("recpedcompra/{codpedcompra}")]
        public async Task<ActionResult<PedidoCompraDTO>> Get(int codpedcompra)
        {
            var pedido = await _data.PedidoCompraConDet(codpedcompra);

            if (pedido == null)
                return NotFound();

            return Ok(pedido);
        }

        [HttpPut]
        [Route("anularpedcompra/{codpedcompra}/{codestado}")]
        public async Task<ActionResult> PutActualizarEstado(int codpedcompra, int codestado)
        {
             try
            {
                string filasAfectadas = await _data.ActualizarEstadoV2(codpedcompra, codestado);

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

        [HttpPut]
        [Route("actualizarpedidodet")]
        public async Task<ActionResult> ActualizarPedCompraDet([FromBody] PedidoCompraUpdateDTO pedido)
        {
            try
            {
                string updateDatos = await _data.ActualizarPedidoCompraDet(pedido);

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


        [HttpPost("insertar")]
        public async Task<IActionResult> InsertarPedidoCompra([FromBody] PedidoCompraInsertDTO pedido)
        {
            try
            {
                int newPedidoCompra = await _data.InsertarPedidoCompra(pedido);
                return Ok(new { mensaje = "Insertado correctamente", id = newPedidoCompra });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }


    }
}