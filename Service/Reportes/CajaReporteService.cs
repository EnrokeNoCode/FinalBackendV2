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


    public async Task<List<CajaFormaCobroDTO>> ObtenerCobrosPorGestion(int codGestion)
    {
        var lista = new List<CajaFormaCobroDTO>();

        using var conn = new NpgsqlConnection(_cn.cadenaSQL());
        await conn.OpenAsync();

        string consultaReporte = _query.CajaReporteCobros();

        using var cmd = new NpgsqlCommand(consultaReporte, conn);
        cmd.Parameters.AddWithValue("@codgestion", codGestion);

        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            lista.Add(new CajaFormaCobroDTO
            {
                formacobro = reader.GetString(0),
                datoformacobro = reader.GetString(1),
                montocobro = reader.GetDecimal(2)
            });
        }

        return lista;
    }

}
