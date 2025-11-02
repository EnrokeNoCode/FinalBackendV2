using Persistence.SQL.Referencial;
using Persistence;
using Model.DTO;
using Npgsql;

namespace Service.Referencial
{
    public class PartesVehiculoService
    {
        private readonly DBConnector _cn;
        private readonly PartesVehiculoSQL _query;

        public PartesVehiculoService(DBConnector cn, PartesVehiculoSQL query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<List<PartesVehiculoListDTO>> GetListPartesVehiculo()
        {
            var lista = new List<PartesVehiculoListDTO>();
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consultaLista = _query.SelectPartesVehiculo();
                using (var cmdLista = new NpgsqlCommand(consultaLista, npgsql))
                {
                    using (var readerLista = await cmdLista.ExecuteReaderAsync())
                    {
                        while (await readerLista.ReadAsync())
                        {
                            var listaSucursal = new PartesVehiculoListDTO
                            {
                                codparte = (int)readerLista["codparte"],
                                numparte = (string)readerLista["numparte"],
                                desparte = (string)readerLista["desparte"]
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
