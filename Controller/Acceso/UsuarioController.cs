using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using Service.Acceso;

namespace Controller.Acceso
{
    [ApiController]
    [Route("api/usuario")]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService _usuarioService;
        public UsuarioController(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpPost("acceso")]
        public async Task<IActionResult> Access([FromBody] UsuarioAccesoDTO request)
        {
            var userData = await _usuarioService.AccessUser(request.nomusuario, request.passusuario);

            if (userData == null)
                return Unauthorized(new { message = "Credenciales inválidas" });

            return Ok(userData);
        }

        [HttpPost("terminal")]
        public async Task<IActionResult> Post([FromForm] TerminalAccesoDTO terminalAcceso)
        {
            var terminalData = await _usuarioService.TerminalData(terminalAcceso.codsucursal, terminalAcceso.pcasociado);
            if (terminalData == null) { return Unauthorized(new { message = "La terminal no esta configurada" }); }

            return Ok(terminalData);
        }
    }
}