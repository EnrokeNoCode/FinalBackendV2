using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using Service.Referencial;
using Utils;

namespace Controller.Referencial
{
    [ApiController]
    [Route("api/tipocompro")]
    public class TipoComprobanteController : ControllerBase
    {
        private readonly TipoComprobanteService terminalService_;
        public TipoComprobanteController(TipoComprobanteService terminalService)
        {
            terminalService_ = terminalService;
        }

        [HttpGet("listatipocompro/{tipomov}/{marca}")]
        public async Task<ActionResult<List<ComprobanteTerminalListDTO>>> ListTipoComprobanteMov(string tipomov, bool marca)
        {
            try
            {
                var listaDatos = await terminalService_.GetListTipoComprobanteMov(tipomov, marca);

                if (listaDatos == null || !listaDatos.Any())
                {
                    return NotFound();
                }
                return Ok(listaDatos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiRespuestaDTO.Error(Mensajes.RecuperarMensaje(CodigoMensajes.InternalServerError)));
            }
        }
    }
}
