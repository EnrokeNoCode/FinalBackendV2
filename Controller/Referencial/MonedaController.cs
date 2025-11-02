using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using Service.Referencial;

namespace Controller.Referencial
{
    [ApiController]
    [Route("api/moneda")]
    public class MonedaController : ControllerBase
    {
        private readonly MonedaService service_;
        public MonedaController(MonedaService service)
        {
            service_ = service;
        }

        [HttpGet("listamoneda")]
        public async Task<ActionResult<List<MonedaListDTO>>> ListMoneda()
        {
            var listaDato = await service_.GetListMoneda();

            if (listaDato == null || !listaDato.Any())
            {
                return NotFound();
            }
            return Ok(listaDato);
        }
    }
}
