using Microsoft.AspNetCore.Mvc;
using Model.DTO.Servicios.PresupuestoServicio;
using Service.Servicio;

namespace Controller.Servicio
{
    [ApiController]
    [Route("api/presupuestoservico")]
    public class PresupuestoServicioController : ControllerBase
    {
        private readonly PresupuestoServicioService _data;
        public PresupuestoServicioController(PresupuestoServicioService data)
        {
            _data = data;
        }

        [HttpGet("lista")]
        public async Task<ActionResult<List<PresupuestoServicioListDTO>>> GetListaPresupuestoServicio(int page = 1, int pageSize = 10)
        {
            var prstServicio = await _data.PresupuestoServicioLista(page, pageSize);

            if (prstServicio == null)
            {
                return NotFound();
            }
            return Ok(prstServicio);
        }

        [HttpPost("insertar")]
        public async Task<IActionResult> PostNuevoPresupuestoServicio([FromBody] PresupuestoServicioInsertDTO dto)
        {
            if (dto == null)
                return BadRequest("Datos inválidos");
            try
            {
                int codpresupuesto = await _data.InsertarPresupuestoServicio(dto);
                return Ok(new
                {
                    message = "Presupuesto de servicio insertado correctamente",
                    codpresupuesto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error al insertar presupuesto",
                    error = ex.Message
                });
            }
        }

        [HttpPut("actualizarestado/{codpresupuestoservicio}/{mov}")]
        public async Task<ActionResult> PutActualizarEstado(int codpresupuestoservicio, string mov)
        {
            try
            {
                string filasAfectadas = await _data.ActualizarEstado(codpresupuestoservicio, mov);

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
