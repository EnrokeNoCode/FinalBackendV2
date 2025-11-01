using Persistence.SQL.Referencial;
using Persistence;
using Model.DTO;
using Npgsql;

namespace Service.Referencial
{
    public class SucursalService
    {
        private readonly DBConnector _cn;
        private readonly SucursalSQL _query;

        public SucursalService(DBConnector cn, SucursalSQL query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<List<SucursalListDTO>> GetListSucursalSesion()
        {
            var lista = new List<SucursalListDTO>();
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consultaLista = _query.SelectListSession();
                using (var cmdLista = new NpgsqlCommand(consultaLista, npgsql))
                {
                    using (var readerLista = await cmdLista.ExecuteReaderAsync())
                    {
                        while (await readerLista.ReadAsync())
                        {
                            var listaSucursal = new SucursalListDTO
                            {
                                codsucursal = (int)readerLista["codsucursal"],
                                numsucursal = (string)readerLista["numsucursal"],
                                dessucursal = (string)readerLista["dessucursal"]
                            };
                            lista.Add(listaSucursal);
                        }
                    }
                }

                await npgsql.CloseAsync();
            }
            return lista;
        }
    }
}
