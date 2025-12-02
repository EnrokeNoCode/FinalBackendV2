using Microsoft.AspNetCore.Mvc;
using Model.DTO.Compras.Compra;
using Model.DTO.Compras.RemisionCompra;
using Service.Compra;

namespace Controller.Compras
{
    [ApiController]
    [Route("api/remisioncompra")]
    public class RemisionCompraController : ControllerBase
    {
        private readonly RemisionCompraService _data;

        public RemisionCompraController(RemisionCompraService data)
        {
            _data = data;
        
        }

        [HttpGet]
        [Route("lista/{codsucursal}")]
        public async Task<ActionResult<List<RemisionCompraListDTO>>> GetList(int codsucursal,int page = 1, int pageSize = 10)
        {
            var comprasList = await _data.RemisionCompraList(page, pageSize, codsucursal);

            if (comprasList == null)
            {
                return NotFound();
            }
            return Ok(comprasList);
        }

        [HttpPost("insertar")]
        public async Task<IActionResult> InsertarRemisonCompra([FromBody] RemisionCompraInsertDTO nccompra)
        {
            try
            {
                int newNCCompraId = await _data.InsertarRemisionCompra(nccompra);
                return Ok(new { mensaje = "Remision Compra insertada correctamente", id = newNCCompraId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("recremisioncompra/{codremisioncompra}")]
        public async Task<ActionResult<RemisionCompraDTO>> Get(int codremisioncompra)
        {
            var compras = await _data.RemisionCompraVer(codremisioncompra);

            if (compras == null)
                return NotFound();

            return Ok(compras);
        }

        [HttpPut("cancelar/{codremisioncompra}")]
        public async Task<ActionResult> CancelarRemision(int codremisioncompra)
        {
            try
            {
                string filasAfectadas = await _data.CancelarRemision(codremisioncompra);

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