using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using Service.Referencial;

namespace Controller.Referencial
{
    [ApiController]
    [Route("api/sucursal")]
    public class SucursalController : ControllerBase
    {
        private readonly SucursalService service_;
        public SucursalController(SucursalService service)
        {
            service_ = service;
        }

        [HttpGet("listasucursal")]
        public async Task<ActionResult<List<SucursalListDTO>>> ListSucursalSession()
        {
            var listaSucursalSession = await service_.GetListSucursalSesion();

            if (listaSucursalSession == null || !listaSucursalSession.Any())
            {
                return NotFound();
            }
            return Ok(listaSucursalSession);
        }
    }
}
