
using Microsoft.AspNetCore.Mvc;
using Model.DTO.Servicios.RegistroVehiculo;
using Service.Servicio;

namespace Controller
{
    [ApiController]
    [Route("api/registrovehiculo")]
    public class RegistroVehiculoController : ControllerBase
    {
        private readonly RegistroVehiculoService _data;

        public RegistroVehiculoController(RegistroVehiculoService data)
        {
            _data = data;
        }

        [HttpGet]
        [Route("lista")]
        public async Task<ActionResult<List<RegistroVehiculoListDTO>>> GetList()
        {
            var registroVehiculoList = await _data.RegistroVehiculoList();

            if (registroVehiculoList == null || !registroVehiculoList.Any())
            {
                return NotFound();
            }
            return Ok(registroVehiculoList);
        }

        [HttpPost("insertar")]
        public async Task<IActionResult> InsertarRegistroVehiculo([FromBody] RegistroVehiculoInsertDTO registrovehiculo)
        {
            try
            {
                int newRegistro = await _data.InsertarRegistroVehiculo(registrovehiculo);
                return Ok(new { mensaje = "Insertado correctamente", id = newRegistro });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}