using Microsoft.AspNetCore.Mvc;
using Model.DTO.Compras.Compra;
using Service.Compra;

namespace Controller.Compras
{
    [ApiController]
    [Route("api/compras")]
    public class ComprasController : ControllerBase
    {
        private readonly ComprasService _data;

        public ComprasController(ComprasService data)
        {
            _data = data;
        }

        [HttpGet("lista/{codsucursal}")]
        public async Task<ActionResult<List<CompraListDTO>>> GetList(int codsucursal, int page = 1, int pageSize = 10)
        {
            var comprasList = await _data.CompraList(page, pageSize, codsucursal);

            if (comprasList == null)
            {
                return NotFound();
            }
            return Ok(comprasList);
        }

        [HttpGet("reccompra/{codcompra}")]
        public async Task<ActionResult<ComprasDTO>> Get(int codcompra)
        {
            var compras = await _data.ComprasVer(codcompra);

            if (compras == null)
                return NotFound();

            return Ok(compras);
        }

        [HttpGet("reccomprascab/{codproveedor}")]
        public async Task<ActionResult<List<ComprasNCListDTO>>> GetCabeceras(int codproveedor)
        {
            var comprasCab = await _data.ComprasListCabecera(codproveedor);

            if (comprasCab == null || comprasCab.Count == 0)
                return NotFound();

            return Ok(comprasCab);
        }

        [HttpGet("reccompradetalle/{codcompra}")]
        public async Task<ActionResult<List<ComprasDetNCListDTO>>> GetDetalle(int codcompra)
        {
            var detalles = await _data.ComprasListDetalle(codcompra);

            if (detalles == null || detalles.Count == 0)
                return NotFound(detalles);

            return Ok(detalles);
        }

        [HttpGet("reccompradetalle/remision/{codcompra}")]
        public async Task<ActionResult<List<ComprasREMDetListDTO>>> GetDetalleRemision(int codcompra)
        {
            var detalles = await _data.ComprasRemListDetalle(codcompra);

            if (detalles == null || detalles.Count == 0)
                return NotFound(detalles);

            return Ok(detalles);
        }

        [HttpPost("insertar")]
        public async Task<IActionResult> InsertarCompra([FromBody] ComprasInsertDTO compra)
        {
            try
            {
                int newCompraId = await _data.InsertarCompra(compra);
                return Ok(new { mensaje = "Compra insertada correctamente", id = newCompraId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPut]
        [Route("actualizarestado/{codcompra}/{codestado}")]
        public async Task<ActionResult> PutActualizarEstado(int codcompra, int codestado)
        {
            try
            {
                string filasAfectadas = await _data.ActualizarEstadoV2(codcompra, codestado);

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