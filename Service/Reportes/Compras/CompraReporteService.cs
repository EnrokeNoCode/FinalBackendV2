using Model.Reportes.Compras;
using Npgsql;
using Persistence;
using Persistence.SQL.Reporte.Compras;

namespace Service.Reportes.Compras
{
    public class CompraReporteService
    {
        private readonly DBConnector _cn;
        private readonly CompraReporteSQL _query;

        public CompraReporteService(DBConnector cn, CompraReporteSQL query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<List<CompraListadoReporteDTO>> ObtenerReporteCompras(
            bool incluirDetalle,
            string? nroDocPrv,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int? codSucursal)
        {
            var lista = new List<CompraListadoReporteDTO>();

            try
            {
                using var conn = new NpgsqlConnection(_cn.cadenaSQL());
                await conn.OpenAsync();
                bool filtrarProveedor = !string.IsNullOrWhiteSpace(nroDocPrv);
                bool filtrarFecha = fechaInicio.HasValue && fechaFin.HasValue;
                bool filtrarSucursal = codSucursal.HasValue;

                using (var cmd = new NpgsqlCommand(
                    _query.SelectCompraListado(filtrarProveedor, filtrarFecha, filtrarSucursal), conn))
                {
                    if (filtrarProveedor)
                        cmd.Parameters.AddWithValue("@nrodocprv", nroDocPrv!);

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
                        lista.Add(new CompraListadoReporteDTO
                        {
                            codcompra = reader.GetInt32(0),
                            fechacompra = reader.GetString(1),
                            numtipocomprobante = reader.GetString(2),
                            numcompra = reader.GetString(3),
                            nrodocprv = reader.GetString(4),
                            razonsocial = reader.GetString(5),
                            desestmov = reader.GetString(6),
                            nummoneda = reader.GetString(7),
                            cotizacion = reader.GetDecimal(8),
                            condicionpago = reader.GetString(9),
                            totaliva = reader.GetDecimal(10),
                            totalexento = reader.GetDecimal(11),
                            totalcompra = reader.GetDecimal(12),
                            detalle = incluirDetalle ? new List<CompraDetListadoReporteDTO>() : null
                        });
                    }
                }
                if (incluirDetalle && lista.Count > 0)
                {
                    foreach (var compra in lista)
                    {
                        using var cmdDet = new NpgsqlCommand(_query.SelectCompraDetListado(), conn);
                        cmdDet.Parameters.AddWithValue("@codcompra", compra.codcompra);

                        using var readerDet = await cmdDet.ExecuteReaderAsync();
                        while (await readerDet.ReadAsync())
                        {
                            compra.detalle!.Add(new CompraDetListadoReporteDTO
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
                throw new Exception("Error al obtener reporte de compras: " + ex.Message, ex);
            }

            return lista;
        }
    }
}
