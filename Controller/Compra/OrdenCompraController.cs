using Microsoft.AspNetCore.Mvc;
using Model.DTO.Compras.OrdenCompra;
using Service.Compra;

namespace Controller
{
    [ApiController]
    [Route("api/ordencompra")] 
    public class OrdenCompraController : ControllerBase
    {
        private readonly OrdenCompraService _data;

        public OrdenCompraController(OrdenCompraService data)
        {
            _data = data;
        }

        [HttpGet]
        [Route("lista")]
        public async Task<ActionResult<List<OrdenCompraListDTO>>> GetList(int page = 1, int pageSize = 10)  
        {
            var ordenCompraList = await _data.OrdenCompraList(page, pageSize); 

            if (ordenCompraList == null)
            {
                return NotFound();
            }
            return Ok(ordenCompraList);
        }

        [HttpGet("recordencompra/{codordenc}")]
        public async Task<ActionResult<OrdenCompraDTO>> Get(int codordenc)
        {
            var ordenCompra = await _data.OrdenCompraVer(codordenc);

            if (ordenCompra == null)
                return NotFound();

            return Ok(ordenCompra);
        }

        [HttpGet("recordenxproveedor/{codproveedor}")]
        public async Task<ActionResult<OrdenCompraComListDTO>> GetOrdenCPorPrv(int codproveedor)
        {
            var ordenCompra = await _data.OrdenCompraxPrv(codproveedor);

            if (ordenCompra == null)
                return NotFound();

            return Ok(ordenCompra);
        }

        [HttpPost("insertar")]
        public async Task<IActionResult> InsertarOrdenCompra([FromBody] OrdenCompraInsertDTO ordenCompra)
        {
            try
            {
                int newOrdenCompraId = await _data.InsertarOrdenCompra(ordenCompra);
                return Ok(new { mensaje = "Orden de compra insertada correctamente", id = newOrdenCompraId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPut]
        [Route("actualizarestado/{codordenc}/{codestado}")]
        public async Task<ActionResult> PutActualizarEstado(int codordenc, int codestado)
        {
            try
            {
                string filasAfectadas = await _data.ActualizarEstadoV2(codordenc, codestado);

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
    }
}
