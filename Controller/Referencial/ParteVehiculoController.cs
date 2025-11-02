using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using Service.Referencial;

namespace Controller.Referencial
{
    [ApiController]
    [Route("api/partesvehiculo")]
    public class ParteVehiculoController : ControllerBase
    {
        private readonly PartesVehiculoService service_;
        public ParteVehiculoController(PartesVehiculoService service)
        {
            service_ = service;
        }

        [HttpGet("listapartesvehiculo")]
        public async Task<ActionResult<List<PartesVehiculoService>>> ListPartesVehiculo()
        {
            var listaDato = await service_.GetListPartesVehiculo();

            if (listaDato == null || !listaDato.Any())
            {
                return NotFound();
            }
            return Ok(listaDato);
        }
    }
}
