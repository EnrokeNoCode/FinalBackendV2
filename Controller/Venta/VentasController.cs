using Microsoft.AspNetCore.Mvc;
using Model.DTO.Ventas.Venta;
using Model.DTO.Ventas.Ventas;
using Service.Venta;

namespace Controller.Venta
{

    [ApiController]
    [Route("api/ventas")]
    public class VentasController : ControllerBase
    {
        private readonly VentasService _data;
        public VentasController(VentasService data)
        {
            _data = data;
        }

        [HttpGet("lista")]
        public async Task<ActionResult<List<VentasListDTO>>> GetList(int page = 1, int pageSize = 10)
        {
            var ventasList = await _data.VentasList(page, pageSize);

            if (ventasList == null)
            {
                return NotFound();
            }
            return Ok(ventasList);
        }

        [HttpGet("ventacontado/{codcliente}")]
        public async Task<ActionResult> GetVentaContado(int codcliente)
        {
            var lista = await _data.VentaContado(codcliente);

            if (lista == null || lista.Count == 0)
            {
                return NotFound();
            }

            return Ok(lista);
        }

        [HttpPost("insertar")]
        public async Task<IActionResult> InsertVentas([FromBody] VentasInsertDTO venta)
        {
            try
            {
                int newVentaId = await _data.InsertarVentas(venta);
                return Ok(new { mensaje = "Venta insertada correctamente", id = newVentaId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPut("anularventa/{codventa}/{codestado}")]
        public async Task<ActionResult> PutActualizarEstado(int codventa, int codestado)
        {
            try
            {
                string filasAfectadas = await _data.ActualizarEstadoV2(codventa, codestado);

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

        [HttpGet("recventacab/{codcliente}")]
        public async Task<ActionResult<List<VentasNCListDTO>>> GetCabeceras(int codcliente)
        {
            var ventasCab = await _data.VentasListCabecera(codcliente);

            if (ventasCab == null || ventasCab.Count == 0)
                return NotFound();

            return Ok(ventasCab);
        }

        [HttpGet("recventadetalle/{codventa}")]
        public async Task<ActionResult<List<VentasDetNCListDTO>>> GetDetalle(int codventa)
        {
            var detalles = await _data.VentasListDetalle(codventa);

            if (detalles == null || detalles.Count == 0)
                return NotFound();

            return Ok(detalles);
        }

        [HttpGet("recventadetalle/remision/{codventa}")]
        public async Task<ActionResult<List<VentasREMDetListDTO>>> GetDetalleRemision(int codventa)
        {
            var detalles = await _data.VentasRemListDetalle(codventa);

            if (detalles == null || detalles.Count == 0)
                return NotFound(detalles);

            return Ok(detalles);
        }

        [HttpGet("recventa/{codventa}")]
        public async Task<ActionResult<VentasDTO>> Get(int codventa)
        {
            var ventas = await _data.ComprasVer(codventa);

            if (ventas == null)
                return NotFound();

            return Ok(ventas);
        }
    }
}
