using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Model.DTO;
using Service.Referencial;
using Utils;

namespace Controller.Referencial
{
    [ApiController]
    [Route("api/producto")]
    public class ProductoController : ControllerBase
    {
        private readonly ProductoService service_;
        public ProductoController(ProductoService service)
        {
            service_ = service;
        }

        [HttpGet("listaproducto")]
        public async Task<ActionResult<List<ProductoListDTO>>> ListProducto(int page = 1, int pageSize = 10)
        {
            try
            {
                var result = await service_.GetProductoLista(page, pageSize);
                if (result.TotalItems == 0)
                {
                    return NotFound(new { Message = "No se encontraron productos." });
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

        [HttpGet("listaproductocompra")]
        public async Task<ActionResult<List<ProductoListCompraDTO>>> ListProductoCompra()
        {
            var listaProductoCompra = await service_.GetProductoCompra();

            if (listaProductoCompra == null || !listaProductoCompra.Any())
            {
                return NotFound();
            }
            return Ok(listaProductoCompra);
        }

        [HttpGet("listaproductoventa/{codsucursal}")]
        public async Task<ActionResult<List<ProductoVentaListDTO>>> ListaProductoVenta(int codsucursal)
        {
            var listaProductoVenta = await service_.GetProductoVenta(codsucursal);

            if (listaProductoVenta == null || !listaProductoVenta.Any())
            {
                return NotFound();
            }
            return Ok(listaProductoVenta);
        }
    }
}
