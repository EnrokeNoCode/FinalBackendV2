using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using Service.Referencial;

namespace Controller.Referencial
{
    [ApiController]
    [Route("api/tipotarjeta")]
    public class TipoTarjetaController : ControllerBase
    {
        private readonly TipoTarjetaService service_;
        public TipoTarjetaController(TipoTarjetaService service)
        {
            service_ = service;
        }

        [HttpGet("lista")]
        public async Task<ActionResult<List<TipoTarjetaDTO>>> ListTarjeta()
        {
            var listaDatos = await service_.GetListTipoTarjeta();

            if (listaDatos == null || !listaDatos.Any())
            {
                return NotFound();
            }
            return Ok(listaDatos);
        }
    }
}
