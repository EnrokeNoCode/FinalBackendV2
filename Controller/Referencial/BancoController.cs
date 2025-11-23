using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using Service.Referencial;

namespace Controller.Referencial
{
    [ApiController]
    [Route("api/banco")]
    public class BancoController : ControllerBase
    {
        private readonly BancoService service_;
        public BancoController(BancoService service)
        {
            service_ = service;
        }

        [HttpGet("lista")]
        public async Task<ActionResult<List<BancoDTO>>> ListBanco()
        {
            var listaDatos = await service_.GetListBanco();

            if (listaDatos == null || !listaDatos.Any())
            {
                return NotFound();
            }
            return Ok(listaDatos);
        }
    }
}
