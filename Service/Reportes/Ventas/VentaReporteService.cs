using Model.Reportes.Ventas;
using Npgsql;
using Persistence;
using Persistence.SQL.Reporte.Ventas;

namespace Service.Reportes.Ventas
{
    public class VentaReporteService
    {
        private readonly DBConnector _cn;
        private readonly VentaReporteSQL _query;

        public VentaReporteService(DBConnector cn, VentaReporteSQL query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<List<VentaListadoReporteDTO>> ObtenerReporteVenta(
            bool incluirDetalle,
            string? nroDocC,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int? codSucursal)
        {
            var lista = new List<VentaListadoReporteDTO>();

            try
            {
                using var conn = new NpgsqlConnection(_cn.cadenaSQL());
                await conn.OpenAsync();
                bool filtrarCliente = !string.IsNullOrWhiteSpace(nroDocC);
                bool filtrarFecha = fechaInicio.HasValue && fechaFin.HasValue;
                bool filtrarSucursal = codSucursal.HasValue;

                using (var cmd = new NpgsqlCommand(
                    _query.SelectVentaListado(filtrarCliente, filtrarFecha, filtrarSucursal), conn))
                {
                    if (filtrarCliente)
                        cmd.Parameters.AddWithValue("@nrodoc", nroDocC!);

                    if (filtrarFecha)
                    {
                        var fin = fechaFin!.Value.Date.AddDays(1).AddSeconds(-1);
                        cmd.Parameters.AddWithValue("@fechainicio", fechaInicio!.Value);
                        cmd.Parameters.AddWithValue("@fechafin", fin);
                    }

                    if (filtrarSucursal)
                        cmd.Parameters.AddWithValue("@codsucursal", codSucursal!.Value);

                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        lista.Add(new VentaListadoReporteDTO
                        {
                            codventa = reader.GetInt32(0),
                            fechaventa = reader.GetString(1),
                            numtipocomprobante = reader.GetString(2),
                            numventa = reader.GetString(3),
                            nrodoc = reader.GetString(4),
                            cliente = reader.GetString(5),
                            desestmov = reader.GetString(6),
                            nummoneda = reader.GetString(7),
                            cotizacion = reader.GetDecimal(8),
                            condicionpago = reader.GetString(9),
                            totaliva = reader.GetDecimal(10),
                            totalexento = reader.GetDecimal(11),
                            totalventa = reader.GetDecimal(12),
                            detalle = incluirDetalle ? new List<VentaDetListadoReporteDTO>() : null
                        });
                    }
                }
                if (incluirDetalle && lista.Count > 0)
                {
                    foreach (var venta in lista)
                    {
                        using var cmdDet = new NpgsqlCommand(_query.SelectVentaDetListado(), conn);
                        cmdDet.Parameters.AddWithValue("@codventa", venta.codventa);

                        using var readerDet = await cmdDet.ExecuteReaderAsync();
                        while (await readerDet.ReadAsync())
                        {
                            venta.detalle!.Add(new VentaDetListadoReporteDTO
                            {
                                codigobarra = readerDet.GetString(0),
                                desproducto = readerDet.GetString(1),
                                desiva = readerDet.GetString(2),
                                cantidad = readerDet.GetDecimal(3),
                                preciobruto = readerDet.GetDecimal(4),
                                precioneto = readerDet.GetDecimal(5),
                                totallinea = readerDet.GetDecimal(6)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener reporte de ventas: " + ex.Message, ex);
            }

            return lista;
        }
    }
}
