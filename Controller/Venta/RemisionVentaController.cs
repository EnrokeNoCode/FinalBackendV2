using Microsoft.AspNetCore.Mvc;
using Model.DTO.Ventas.Venta;
using Model.DTO.Ventas.RemisionVenta;
using Service.Venta;

namespace Controller.Venta
{
    [ApiController]
    [Route("api/remisionventa")]
    public class RemisionVentaController : ControllerBase
    {
        private readonly RemisionVentaService _data;

        public RemisionVentaController(RemisionVentaService data)
        {
            _data = data;
        
        }

        [HttpGet]
        [Route("lista")]
        public async Task<ActionResult<List<RemisionVentaListDTO>>> GetList(int page = 1, int pageSize = 10)
        {
            var comprasList = await _data.RemisionVentaList(page, pageSize);

            if (comprasList == null)
            {
                return NotFound();
            }
            return Ok(comprasList);
        }

        [HttpGet("recremisionventa/{codremisionventa}")]
        public async Task<ActionResult<RemisionVentaDTO>> Get(int codremisionventa)
        {
            var compras = await _data.RemisionVentaVer(codremisionventa);

            if (compras == null)
                return NotFound();

            return Ok(compras);
        }

        [HttpPost("insertar")]
        public async Task<IActionResult> InsertarRemisonVenta([FromBody] RemisionVentaInsertDTO nrventa)
        {
            try
            {
                int newNRVentaId = await _data.InsertarRemisionVenta(nrventa);
                return Ok(new { mensaje = "Remision Venta insertada correctamente", id = newNRVentaId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPut("cancelar/{codremisionventa}")]
        public async Task<ActionResult> CancelarRemision(int codremisionventa)
        {
            try
            {
                string filasAfectadas = await _data.CancelarRemision(codremisionventa);

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