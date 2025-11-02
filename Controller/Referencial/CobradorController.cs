using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using Service.Referencial;

namespace Controller.Referencial
{
    [ApiController]
    [Route("api/cobrador")]
    public class CobradorController : ControllerBase
    {
        private readonly CobradorService service_;
        public CobradorController(CobradorService service)
        {
            service_ = service;
        }

        [HttpGet("listacobrador")]
        public async Task<ActionResult<List<CobradorListDTO>>> ListCobrador()
        {
            var listaCobrador = await service_.GetCobradorList();

            if (listaCobrador == null || !listaCobrador.Any())
            {
                return NotFound();
            }
            return Ok(listaCobrador);
        }
    }
}
