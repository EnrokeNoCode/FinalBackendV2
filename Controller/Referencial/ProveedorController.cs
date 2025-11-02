using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using Service.Referencial;
using Utils;

namespace Controller.Referencial
{
    [ApiController]
    [Route("api/proveedor")]
    public class ProveedorController : ControllerBase
    {
        private readonly ProveedorService service_;
        public ProveedorController(ProveedorService service)
        {
            service_ = service;
        }

        [HttpGet("listaproveedor")]
        public async Task<ActionResult> ListProveedor(int page = 1, int pageSize = 10)
        {
            try
            {
                var result = await service_.GetProveedor( page, pageSize);
                if (result.TotalItems == 0)
                {
                    return NotFound(new { Message = "No se encontraron gestiones registradas." });
                }

                return Ok(new
                {
                    items = result.Data,
                    totalItems = result.TotalItems,
                    totalPages = result.TotalPages,
                    page = result.Page,
                    pageSize = result.PageSize
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiRespuestaDTO.Error(Mensajes.RecuperarMensaje(CodigoMensajes.InternalServerError)));
            }
        }

        [HttpGet("listaproveedorcompra")]
        public async Task<ActionResult<List<ProveedorListCompraDTO>>> ListSucursalSession()
        {
            var listaProveedorCompra = await service_.GetProveedorCompra();

            if (listaProveedorCompra == null || !listaProveedorCompra.Any())
            {
                return NotFound();
            }
            return Ok(listaProveedorCompra);
        }
    }
}
