using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using Model.DTO.Referencial;
using Service.Referencial;
using Utils;

namespace Controller.Referencial
{
    [ApiController]
    [Route("api/cliente")]
    public class ClienteController : ControllerBase
    {
        private readonly ClienteService service_;
        public ClienteController(ClienteService service)
        {
            service_ = service;
        }

        [HttpGet("listacliente")]
        public async Task<ActionResult> ListGestionesPorSucursal(int page = 1, int pageSize = 10)
        {
            try
            {
                var result = await service_.GetListaCliente(page, pageSize);
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
                var vehiculos = await service_.GetListaClienteVehiculo(codcliente);
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
                var listaClienteMov = await service_.GetListaClienteMov();

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

        [HttpGet("reccliente/{codcliente}")]
        public async Task<IActionResult> GetCliente(int codcliente)
        {
            var cliente = await service_.ObtenerCliente(codcliente);
            if (cliente == null)
                return NotFound(new { error = "Cliente no encontrado" });

            return Ok(cliente);
        }

        [HttpPost("insert")]
        public async Task<IActionResult> InsertarCliente([FromBody] ClienteInsertDTO cliente)
        {
            if (cliente == null)
                return BadRequest("Datos del cliente inválidos");

            try
            {
                string resultado = await service_.InsertarNuevoCliente(cliente);
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

        [HttpPut("actualizarcliente")]
        public async Task<IActionResult> UpdateCliente([FromBody] ClienteUpdateDTO dto)
        {
            try
            {
                var mensaje = await service_.ActualizarCliente(dto);
                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
