using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using Model.DTO.Referencial;
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
        public async Task<ActionResult> GetListProveedor(int page = 1, int pageSize = 10)
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
        public async Task<ActionResult<List<ProveedorListCompraDTO>>> GetListProveedorCompra()
        {
            var listaProveedorCompra = await service_.GetProveedorCompra();

            if (listaProveedorCompra == null || !listaProveedorCompra.Any())
            {
                return NotFound();
            }
            return Ok(listaProveedorCompra);
        }

        [HttpGet("recproveedor/{codproveedor}")]
        public async Task<IActionResult> GetProveedor(int codproveedor)
        {
            var proveedor = await service_.ObtenerProveedor(codproveedor);
            if (proveedor == null)
                return NotFound(new { error = "Proveedor no encontrado" });

            return Ok(proveedor);
        }

        [HttpPost("insert")]
        public async Task<IActionResult> PostInsertarProveedor([FromBody] ProveedorInsertDTO proveedor)
        {
            if (proveedor == null)
                return BadRequest("Datos del proveedor inválidos");

            try
            {
                string resultado = await service_.InsertarNuevoProveedor(proveedor);
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

        [HttpPut("actualizarproveedor")]
        public async Task<IActionResult> UpdateProveedor([FromBody] ProveedorUpdateDTO dto)
        {
            try
            {
                var mensaje = await service_.ActualizarProveedor(dto);
                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
