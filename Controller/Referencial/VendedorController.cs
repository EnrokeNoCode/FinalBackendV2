using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using Service.Referencial;

namespace Controller.Referencial
{
    [ApiController]
    [Route("api/vendedor")]
    public class VendedorController : ControllerBase
    {
        private readonly VendedorService service_;
        public VendedorController(VendedorService service)
        {
            service_ = service;
        }

        [HttpGet("listavendedormov")]
        public async Task<ActionResult<List<VendedorService>>> ListVendedor()
        {
            var listaVendedor = await service_.GetListVendedor();

            if (listaVendedor == null || !listaVendedor.Any())
            {
                return NotFound();
            }
            return Ok(listaVendedor);
        }
    }
}
