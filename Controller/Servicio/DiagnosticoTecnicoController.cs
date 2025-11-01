using Microsoft.AspNetCore.Mvc;
using Model.DTO.Servicios.DiagnosticoTecnico;
using Service.Servicio;

namespace Controller
{
    [ApiController]
    [Route("api/diagnosticotecnico")]
    public class DiagnosticoTecnicoController : ControllerBase
    {
        private readonly DiagnosticoTecnicoService _data;

        public DiagnosticoTecnicoController(DiagnosticoTecnicoService data)
        {
            _data = data;
        }

        [HttpGet]
        [Route("lista")]
        public async Task<ActionResult<List<DiagnosticoTecnicoListDTO>>> GetList()
        {
            var diagnosticoTecnicoList = await _data.DiagnosticoTecnicoList();

            if (diagnosticoTecnicoList == null || !diagnosticoTecnicoList.Any())
            {
                return NotFound();
            }
            return Ok(diagnosticoTecnicoList);
        }

        [HttpGet("recdiagnosticotecnico/{coddiagnostico}")]
        public async Task<ActionResult<DiagnosticoTecnicoListWithDetDTO>> Get(int coddiagnostico)
        {
            var pedido = await _data.DiagnosticoTecnicoConDet(coddiagnostico);

            if (pedido == null)
                return NotFound();

            return Ok(pedido);
        }

        [HttpPut]
        [Route("actualizarestado/{coddiagnostico}/{codestado}")]
        public async Task<ActionResult> PutActualizarEstado(int coddiagnostico, int codestado)
        {
            try
            {
                int filasAfectadas = await _data.ActualizarEstado(coddiagnostico, codestado);

                if (filasAfectadas > 0)
                {
                    return Ok(new { message = "Estado del diagnostico se a actualizado correctamente." });
                }
                else
                {
                    return NotFound(new { message = "No se encontró el diagnostico con el código especificado." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }

        }

        [HttpPost("insertar")]
        public async Task<IActionResult> InsertarDiagnosticoTecnico([FromBody] DiagnosticoTecnicoInsertDTO diagtec)
        {
            try
            {
                int newDiagnosticoTecnico = await _data.InsertarDiagnosticoTecnico(diagtec);
                return Ok(new { mensaje = "Insertado correctamente", id = newDiagnosticoTecnico });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}