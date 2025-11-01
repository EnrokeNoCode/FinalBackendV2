using Microsoft.AspNetCore.Mvc;
using Model.DTO.Compras.Ajustes;
using Service.Compra;

namespace Controller
{
    [ApiController]
    [Route("api/ajustes")]
    public class AjustesController : ControllerBase
    {
        private readonly AjustesService _data;

        public AjustesController(AjustesService data)
        {
            _data = data;
        }

        [HttpGet]
        [Route("lista")]
        public async Task<ActionResult<List<AjustesListDTO>>> GetList(int page = 1, int pageSize = 10)
        {
            var ajustesList = await _data.AjustaList(page, pageSize);

            if (ajustesList == null)
            {
                return NotFound();
            }
            return Ok(ajustesList);
        }

        [HttpPost("insertar")]
        public async Task<IActionResult> InsertAjuste([FromBody] AjustesInsertDTO pedido)
        {
            try
            {
                int newAjuste = await _data.InsertarAjustes(pedido);
                return Ok(new { mensaje = "Insertado correctamente", id = newAjuste });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
