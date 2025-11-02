using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using Service.Referencial;

namespace Controller.Referencial
{
    [ApiController]
    [Route("api/estadomov")]
    public class EstadoMovimientoController : ControllerBase
    {
        private readonly EstadoMovimientoService service_;
        public EstadoMovimientoController(EstadoMovimientoService service)
        {
            service_ = service;
        }

        [HttpGet("listaestadomov")]
        public async Task<ActionResult<List<EstadoMovimientoListDTO>>> ListEstadoMovimiento()
        {
            var listaDato = await service_.GetListEstadoMovimiento();

            if (listaDato == null || !listaDato.Any())
            {
                return NotFound();
            }
            return Ok(listaDato);
        }
    }
}
