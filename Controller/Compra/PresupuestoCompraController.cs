using Microsoft.AspNetCore.Mvc;
using Model.DTO.Compras.PresupuestoCompra;
using Service.Compra;

namespace Controller
{
    [ApiController]
    [Route("api/presupuestocompra")]
    public class PresupuestoCompraController : ControllerBase
    {
        private readonly PresupuestoCompraService _data;
        public PresupuestoCompraController(PresupuestoCompraService data)
        {
            _data = data;
        }

        [HttpGet]
        [Route("lista")]
        public async Task<ActionResult<PresupuestoCompraListDTO>> GetList(int page = 1, int pageSize = 10)
        {
            var presupuestocompra = await _data.PresupuestoCompraList(page, pageSize);

            if (presupuestocompra == null)
            {
                return NotFound();
            }
            return Ok(presupuestocompra);
        }

        [HttpGet("recprstcompra/{codprstcompra}")]
        public async Task<ActionResult<PresupuestoCompraDTO>> Get(int codprstcompra)
        {
            var presupuesto = await _data.PresupuestoCompraVer(codprstcompra);

            if (presupuesto == null)
                return NotFound();

            return Ok(presupuesto);
        }

        [HttpGet("recprstxproveedor/{codproveedor}")]
        public async Task<ActionResult<PresupuestoCompraOCListDTO>> GetPrstPorPrv(int codproveedor)
        {
            var presupuesto = await _data.PresupuestoCompraxPrv(codproveedor);

            if (presupuesto == null)
                return NotFound();

            return Ok(presupuesto);
        }

        [HttpPost("insertar")]
        public async Task<IActionResult> InsertarPresupuestoCompra([FromBody] PresupuestoCompraInsertDto prstcompra)
        {
            try
            {
                int newPresupuestoCompra = await _data.InsertarPresupuestoCompra(prstcompra);
                return Ok(new { mensaje = "Insertado correctamente", id = newPresupuestoCompra });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPut]
        [Route("anularpresupuestocompra/{codpresupuestocompra}/{codestado}")]
        public async Task<ActionResult> PutActualizarEstado(int codpresupuestocompra, int codestado)
        {
            try
            {
                string filasAfectadas = await _data.ActualizarEstadoV2(codpresupuestocompra, codestado);
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
