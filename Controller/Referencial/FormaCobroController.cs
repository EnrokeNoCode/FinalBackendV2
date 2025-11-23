using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using Service.Referencial;

namespace Controller.Referencial
{
    
    [ApiController]
    [Route("api/formacobro")]
    public class FormaCobroController : ControllerBase
    {
        private readonly FormaCobroService service_;
        public FormaCobroController(FormaCobroService service)
        {
            service_ = service;
        }

        [HttpGet("listaformacobro")]
        public async Task<ActionResult<List<FormaCobroDTO>>> ListFormaCobro()
        {
            var listaDatos = await service_.GetFormaCobro();

            if (listaDatos == null || !listaDatos.Any())
            {
                return NotFound();
            }
            return Ok(listaDatos);
        }
    }
}
