using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Model.DTO;
using Model.DTO.Referencial;
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

        [HttpPost("insert")]
        public async Task<IActionResult> InsertarProducto([FromBody] ProductoInsertDTO producto)
        {
            if (producto == null)
                return BadRequest("Datos del producto inválidos");

            try
            {
                string resultado = await service_.InsertarNuevoProducto(producto);
                if (resultado.StartsWith("OK"))
                    return Ok(new { mensaje = resultado });
                else
                    return BadRequest(new { error = resultado });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = "Error al procesar la solicitud: " + ex.Message });
            }
        }

        [HttpPut("actualizarregistro/{cod}")]
        public async Task<ActionResult> PutActualizarRegistro(int cod)
        {
            try
            {
                string filasAfectadas = await service_.ActulizarEliminarRegistro(cod);

                if (filasAfectadas.StartsWith("OK"))
                {
                    return Ok(new { message = filasAfectadas });
                }
                else if (filasAfectadas.StartsWith("ERROR"))
                {
                    return BadRequest(new { message = filasAfectadas });
                }
                else
                {
                    return StatusCode(500, new { message = "Respuesta inesperada: " + filasAfectadas });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
        [HttpGet("recproducto/{codproducto}")]
        public async Task<IActionResult> GetProducto(int codproducto)
        {
            var producto = await service_.ObtenerProducto(codproducto);
            if (producto == null)
                return NotFound(new { error = "Producto no encontrado" });

            return Ok(producto);
        }

        [HttpPut("actualizarproducto")]
        public async Task<IActionResult> UpdateProducto([FromBody] ProductoUpdateDTO dto)
        {
            try
            {
                var mensaje = await service_.ActualizarProducto(dto);
                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
