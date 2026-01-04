using Model.Reportes.Servicios;
using Model.Reportes.Ventas;
using Npgsql;
using Persistence;
using Persistence.SQL.Reporte.Servicios;

namespace Service.Reportes.Servicios
{
    public class PresupuestoServicioReporteService
    {
        private readonly DBConnector _cn;
        private readonly PresupuestoServicioReporteSQL _query;

        public PresupuestoServicioReporteService(DBConnector cn, PresupuestoServicioReporteSQL query)
        {
            _cn = cn;
            _query = query;
        }
        public async Task<PresupuestoServicioReporteDTO> ObtenerPresupuestoServicio(int codpresupuesto)
        {
            var presupuesto = new PresupuestoServicioReporteDTO();

            try
            {
                using var conn = new NpgsqlConnection(_cn.cadenaSQL());
                await conn.OpenAsync();
                using (var cmdCab = new NpgsqlCommand(_query.SelectPresupuestoCab(), conn))
                {
                    cmdCab.Parameters.AddWithValue("@codpresupuesto", codpresupuesto);

                    using var reader = await cmdCab.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        presupuesto.codpresupuesto = reader.GetInt32(0);
                        presupuesto.fechapresupuesto = reader.GetString(1);
                        presupuesto.nropresupuesto = reader.GetString(2);
                        presupuesto.cliente = reader.GetString(3);
                        presupuesto.vehiculo = reader.GetString(4);
                        presupuesto.diagnostico = reader.GetString(5);
                        presupuesto.totalpresupuesto = reader.GetDecimal(6);
                        presupuesto.detalleservicio = new List<PresupuestoServicioDetServicioReporteDTO>();
                        presupuesto.detallerepuesto = new List<PresupuestoServicioDetRepuestoReporteDTO>();
                    }
                }
                if (presupuesto.codpresupuesto > 0)
                {
                    using (var cmdDetServ = new NpgsqlCommand(_query.SelectPresupuestoDetServicio(), conn))
                    {
                        cmdDetServ.Parameters.AddWithValue("@codpresupuesto", codpresupuesto);
                        using var readerDetServ = await cmdDetServ.ExecuteReaderAsync();
                        while (await readerDetServ.ReadAsync())
                        {
                            presupuesto.detalleservicio!.Add(new PresupuestoServicioDetServicioReporteDTO
                            {
                                codigobarra = readerDetServ.GetString(0),
                                desproducto = readerDetServ.GetString(1),
                                cantidad = readerDetServ.GetDecimal(2),
                                precioneto = readerDetServ.GetDecimal(3),
                                preciobruto = readerDetServ.GetDecimal(4),
                                totallinea = readerDetServ.GetDecimal(5)
                            });
                        }
                    }
                    using (var cmdDetRep = new NpgsqlCommand(_query.SelectPresupuestoDetRepuesto(), conn))
                    {
                        cmdDetRep.Parameters.AddWithValue("@codpresupuesto", codpresupuesto);
                        using var readerDetRep = await cmdDetRep.ExecuteReaderAsync();
                        while (await readerDetRep.ReadAsync())
                        {
                            presupuesto.detallerepuesto!.Add(new PresupuestoServicioDetRepuestoReporteDTO
                            {
                                codigotiposervicio = readerDetRep.GetString(0),
                                destiposervicio = readerDetRep.GetString(1),
                                observacion = readerDetRep.GetString(2),
                                precioneto = readerDetRep.GetDecimal(3),
                                preciobruto = readerDetRep.GetDecimal(4)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener presupuesto de servicio: " + ex.Message, ex);
            }
            return presupuesto;
        }

    }
}
