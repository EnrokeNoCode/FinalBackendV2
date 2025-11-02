using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using Service.Referencial;
using Utils;

namespace Controller.Referencial
{
    [ApiController]
    [Route("api/terminal")]
    public class TerminalController : ControllerBase
    {
        private readonly TerminalService terminalService_;
        public TerminalController(TerminalService terminalService)
        {
            terminalService_ = terminalService;
        }
        [HttpGet("listacomproterminal/{codterminal}/{codtipocomprobante}")]
        public async Task<IActionResult> ListComprobanteTerminal(int codterminal, int codtipocomprobante)
        {
            try
            {
                var comprobante = await terminalService_.GetComprobanteTerminal(codterminal, codtipocomprobante);

                if (comprobante == null)
                {
                    return NotFound(new { message = "Comprobante no habilitado en la terminal" });
                }

                return Ok(comprobante);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiRespuestaDTO.Error(Mensajes.RecuperarMensaje(CodigoMensajes.InternalServerError)));
            }
        }
    }
}
