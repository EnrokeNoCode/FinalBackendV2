using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using Service.Referencial;

namespace Controller.Referencial
{
    [ApiController]
    [Route("api/motivoajuste")]
    public class MotivoAjusteController : ControllerBase
    {
        private readonly MotivoAjusteService service_;
        public MotivoAjusteController(MotivoAjusteService service)
        {
            service_ = service;
        }

        [HttpGet("listamotivoajuste")]
        public async Task<ActionResult<List<MotivoAjusteService>>> ListMotivoAjuste()
        {
            var listaDato = await service_.GetListMotivoAjuste();

            if (listaDato == null || !listaDato.Any())
            {
                return NotFound();
            }
            return Ok(listaDato);
        }
    }
}
