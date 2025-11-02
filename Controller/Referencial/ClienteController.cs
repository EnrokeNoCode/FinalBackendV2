using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using Service.Referencial;
using Utils;

namespace Controller.Referencial
{
    [ApiController]
    [Route("api/cliente")]
    public class ClienteController : ControllerBase
    {
        private readonly ClienteService clienteService_;
        public ClienteController(ClienteService clienteService)
        {
            clienteService_ = clienteService;
        }

        [HttpGet("listacliente")]
        public async Task<ActionResult> ListGestionesPorSucursal(int page = 1, int pageSize = 10)
        {
            try
            {
                var result = await clienteService_.GetListaCliente(page, pageSize);
                if (result.TotalItems == 0)
                {
                    return NotFound(new { Message = "No se encontraron clientes." });
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

        [HttpGet("recuperarlistavehiculo/{codcliente}")]
        public async Task<IActionResult> ListDataVehiculo(int codcliente)
        {
            try
            {
                var vehiculos = await clienteService_.GetListaClienteVehiculo(codcliente);
                if (vehiculos == null || vehiculos.Count == 0)
                {
                    return NotFound(new { message = "El cliente no tiene ningún vehículo registrado." });
                }
                return Ok(vehiculos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiRespuestaDTO.Error(Mensajes.RecuperarMensaje(CodigoMensajes.InternalServerError)));
            }
        }

        [HttpGet("listaclienteMov")]
        public async Task<ActionResult<List<ClienteListDTO>>> ListData()
        {
            try
            {
                var listaClienteMov = await clienteService_.GetListaClienteMov();

                if (listaClienteMov == null || !listaClienteMov.Any())
                {
                    return NotFound();
                }
                return Ok(listaClienteMov);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiRespuestaDTO.Error(Mensajes.RecuperarMensaje(CodigoMensajes.InternalServerError)));
            }
        }
    }
}
