using Microsoft.AspNetCore.Mvc;
using Model.DTO.Ventas.PresupuestoVenta;
using Service.Venta;

namespace Controller.Venta
{
    [ApiController]
    [Route("api/presupuestoventa")]

    public class PresupuestoVentaController : ControllerBase
    {
        private readonly PresupuestoVentaService _data;
        public PresupuestoVentaController(PresupuestoVentaService data)
        {
            _data = data;
        }

        [HttpGet("lista")]
        public async Task<ActionResult<List<PresupuestoVentaListDTO>>> GetListaPresupuestoVenta(int page = 1, int pageSize = 10)
        {
            var prstVenta = await _data.PresupuestoVentaLista(page, pageSize);

            if (prstVenta == null)
            {
                return NotFound();
            }
            return Ok(prstVenta);
        }

        [HttpGet("listapresupuestoxcliente/{codcliente}")]
        public async Task<ActionResult<List<PresupuestoVentaListPorClienteDTO>>> GetPresupuestoVentaPorCliente(int codcliente)
        {
            try
            {
                var presupuestos = await _data.PresupuestoVentaxCliente(codcliente);
                if (presupuestos == null)
                    presupuestos = new List<PresupuestoVentaListPorClienteDTO>();

                return Ok(presupuestos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al consultar presupuestos: " + ex.Message });
            }
        }

        [HttpGet("recprstventa/{codprstventa}")]
        public async Task<ActionResult<PresupuestoVentaDTO>> Get(int codprstventa)
        {
            var presupuesto = await _data.PresupuestoVentaVer(codprstventa);

            if (presupuesto == null)
                return NotFound();

            return Ok(presupuesto);
        }

        [HttpPut("anularpresupuestoventa/{codpresupuestoventa}/{codestado}")]
        public async Task<ActionResult> PutActualizarEstado(int codpresupuestoventa, int codestado)
        {
            try
            {
                string filasAfectadas = await _data.ActualizarEstadoV2(codpresupuestoventa, codestado);

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
        public async Task<IActionResult> InsertarPresupuestoVenta([FromBody] PresupuestoVentaInsertDTO presupuesto)
        {
            try
            {
                int newPresupuestoVenta = await _data.InsertarPresupuestoVenta(presupuesto);
                return Ok(new { mensaje = "Insertado correctamente", id = newPresupuestoVenta });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
     
}