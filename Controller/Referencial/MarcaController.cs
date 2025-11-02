using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using Service.Referencial;

namespace Controller.Referencial
{
    [ApiController]
    [Route("api/marca")]
    public class MarcaController : ControllerBase
    {
        private readonly MarcaService service_;
        public MarcaController(MarcaService service)
        {
            service_ = service;
        }

        [HttpGet("listamarca/{marca}")]
        public async Task<ActionResult<List<MarcaListDTO>>> ListMarca(bool marca)
        {
            var listaDatos = await service_.GetListMarca(marca);

            if (listaDatos == null || !listaDatos.Any())
            {
                return NotFound();
            }
            return Ok(listaDatos);
        }
    }
}
