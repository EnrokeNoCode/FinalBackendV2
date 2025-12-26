using Persistence.SQL.Reporte.Referenciales;
using Npgsql;
using Persistence;
using Model.Reportes.Producto;

namespace Service.Reportes.Referenciales
{
    public class ProductoReporteService
    {
        private readonly DBConnector _cn;
        private readonly ProductoReporteSQL _query;

        public ProductoReporteService(DBConnector cn, ProductoReporteSQL query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<List<ProductoListadoDTO>> ObtenerListadoProductos()
        {
            var lista = new List<ProductoListadoDTO>();

            using var conn = new NpgsqlConnection(_cn.cadenaSQL());
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(_query.ProductoListado(), conn);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new ProductoListadoDTO
                {
                    
                    datoproducto = reader.GetString(0),
                    datoproveedor = reader.GetString(1),
                    datoiva = reader.GetString(2),
                    afectastock = reader.GetString(3),
                    estado = reader.GetString(4),
                    costoultimo = reader.GetDecimal(5),
                    datoseccion = reader.GetString(6)
                });
            }

            return lista;
        }
    }
}
