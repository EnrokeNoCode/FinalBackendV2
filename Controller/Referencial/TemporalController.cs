using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using Service.Referencial;

namespace Controller.Referencial
{
    [ApiController]
    [Route("api/temporal")]
    public class TemporalController : ControllerBase
    {
        private readonly TemporalService service_;
        public TemporalController(TemporalService service)
        {
            service_ = service;
        }

        [HttpGet("tipoidentificacion")]
        public async Task<ActionResult<List<TemporalDTO>>> ListTipoIdentificacion()
        {
            var listaDatos = await service_.GetListTemporal("tipo_identificacion");

            if (listaDatos == null || !listaDatos.Any())
            {
                return NotFound();
            }
            return Ok(listaDatos);
        }

        [HttpGet("ciudad")]
        public async Task<ActionResult<List<TemporalDTO>>> ListCiudad()
        {
            var listaDatos = await service_.GetListTemporal("ciudad");

            if (listaDatos == null || !listaDatos.Any())
            {
                return NotFound();
            }
            return Ok(listaDatos);
        }

        [HttpGet("tipolistaprecio")]
        public async Task<ActionResult<List<TemporalDTO>>> ListTipoPrecio()
        {
            var listaDatos = await service_.GetListTemporal("tipolistaprecio");

            if (listaDatos == null || !listaDatos.Any())
            {
                return NotFound();
            }
            return Ok(listaDatos);
        }

        [HttpGet("familia")]
        public async Task<ActionResult<List<TemporalDTO>>> ListFamilia()
        {
            var listaDatos = await service_.GetListTemporal("familia");

            if (listaDatos == null || !listaDatos.Any())
            {
                return NotFound();
            }
            return Ok(listaDatos);
        }

        [HttpGet("rubro")]
        public async Task<ActionResult<List<TemporalDTO>>> ListRubro()
        {
            var listaDatos = await service_.GetListTemporal("rubro");

            if (listaDatos == null || !listaDatos.Any())
            {
                return NotFound();
            }
            return Ok(listaDatos);
        }

        [HttpGet("unidadmedida")]
        public async Task<ActionResult<List<TemporalDTO>>> ListUnidadMedida()
        {
            var listaDatos = await service_.GetListTemporal("unidadmedida");

            if (listaDatos == null || !listaDatos.Any())
            {
                return NotFound();
            }
            return Ok(listaDatos);
        }

        [HttpGet("tipoiva")]
        public async Task<ActionResult<List<TemporalDTO>>> ListTipoIva()
        {
            var listaDatos = await service_.GetListTemporal("tipoiva");

            if (listaDatos == null || !listaDatos.Any())
            {
                return NotFound();
            }
            return Ok(listaDatos);
        }

        [HttpGet("tiposervicio")]
        public async Task<ActionResult<List<TemporalDTO>>> ListTipoServicio()
        {
            var listaDatos = await service_.GetListTemporal("tiposervicio");

            if (listaDatos == null || !listaDatos.Any())
            {
                return NotFound();
            }
            return Ok(listaDatos);
        }
    }
}
