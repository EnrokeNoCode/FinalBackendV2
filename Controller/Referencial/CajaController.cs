using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Model.DTO;
using Service.Referencial;
using Utils;

namespace Controller.Referencial
{
    [ApiController]
    [Route("api/caja")] 
    public class CajaController : ControllerBase
    {
        private readonly CajaService cajaServices_;
        public CajaController(CajaService cajaServices)
        {
            cajaServices_ = cajaServices;
        }

        [HttpGet("listagestion/{codsucursal}")]
        public async Task<ActionResult> ListGestionesPorSucursal(int codsucursal, int page = 1, int pageSize = 10)
        {
            try
            {
                var result = await cajaServices_.GetCajaGestionSuc(codsucursal, page, pageSize);
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
            catch (Exception)
            {
                return StatusCode(500, ApiRespuestaDTO.Error(Mensajes.RecuperarMensaje(CodigoMensajes.InternalServerError)));
            }
        }

    }
}