using Model.Reportes.Caja;
using Npgsql;
using Persistence;
using Persistence.SQL.Reporte;

namespace Service.Reportes;
public class CajaReporteService
{
    private readonly DBConnector _cn;
    private readonly CajaReporteSQL _query;

    public CajaReporteService(DBConnector cn, CajaReporteSQL query)
    {
        _cn = cn;
        _query = query; 
    }


    public async Task<CajaCobroDTO?> ObtenerReporteCaja(int codGestion)
    {
        using var conn = new NpgsqlConnection(_cn.cadenaSQL());
        await conn.OpenAsync();
        CajaCobroDTO? caja = null;

        using (var cmd = new NpgsqlCommand(_query.CajaInfoCobros(), conn))
        {
            cmd.Parameters.AddWithValue("@codgestion", codGestion);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                caja = new CajaCobroDTO
                {
                    sucursal = reader.GetString(0),
                    caja = reader.GetString(1),
                    cobrador = reader.GetString(2),
                    cajaformacobro = new List<CajaFormaCobroDTO>()
                };
            }
        }

        if (caja == null)
            return null;
        using (var cmd = new NpgsqlCommand(_query.CajaReporteCobros(), conn))
        {
            cmd.Parameters.AddWithValue("@codgestion", codGestion);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                caja.cajaformacobro.Add(new CajaFormaCobroDTO
                {
                    formacobro = reader.GetString(0),
                    datoformacobro = reader.GetString(1),
                    montocobro = reader.GetDecimal(2)
                });
            }
        }

        return caja;
    }


}
