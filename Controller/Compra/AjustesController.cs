using Microsoft.AspNetCore.Mvc;
using Model.DTO.Compras.Ajustes;
using Model.DTO.Compras.Compra;
using Model.DTO.Compras.PedidoCompra;
using Service.Compra;

namespace Controller
{
    [ApiController]
    [Route("api/ajustes")]
    public class AjustesController : ControllerBase
    {
        private readonly AjustesService _data;

        public AjustesController(AjustesService data)
        {
            _data = data;
        }

        [HttpGet("lista/{codsucursal}")]
        public async Task<ActionResult<List<AjustesListDTO>>> GetList(int codsucursal, int page = 1, int pageSize = 10)
        {
            var ajustesList = await _data.AjusteList(page, pageSize, codsucursal);

            if (ajustesList == null)
            {
                return NotFound();
            }
            return Ok(ajustesList);
        }

        [HttpPost("insertar")]
        public async Task<IActionResult> InsertAjuste([FromBody] AjustesInsertDTO pedido)
        {
            try
            {
                int newAjuste = await _data.InsertarAjustes(pedido);
                return Ok(new { mensaje = "Insertado correctamente", id = newAjuste });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        [HttpGet("recajuste/{codajuste}")]
        public async Task<ActionResult<AjustesDTO>> Get(int codajuste)
        {
            var ajustes = await _data.AjusteVer(codajuste);

            if (ajustes == null)
                return NotFound(ajustes);

            return Ok(ajustes);
        }

        [HttpPut("actualizarajustedet")]
        public async Task<ActionResult> ActualizarPedCompraDet([FromBody] AjusteUpdateDTO ajuste)
        {
            try
            {
                string updateDatos = await _data.ActualizarAjusteDet(ajuste);

                if (updateDatos.StartsWith("OK"))
                {
                    return Ok(new { message = updateDatos});
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

        [HttpPut("cerrarajuste/{codajuste}")]
        public async Task<ActionResult> PutActualizarEstado(int codajuste)
        {
            try
            {
                string filasAfectadas = await _data.CerrarAjuste(codajuste);

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
