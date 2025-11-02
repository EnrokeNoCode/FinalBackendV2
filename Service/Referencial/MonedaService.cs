using Persistence.SQL.Referencial;
using Persistence;
using Model.DTO;
using Npgsql;

namespace Service.Referencial
{
    public class MonedaService
    {
        private readonly DBConnector _cn;
        private readonly MonedaSQL _query;

        public MonedaService(DBConnector cn, MonedaSQL query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<List<MonedaListDTO>> GetListMoneda()
        {
            var lista = new List<MonedaListDTO>();
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consultaLista = _query.SelectMoneda();
                using (var cmdLista = new NpgsqlCommand(consultaLista, npgsql))
                {
                    using (var readerLista = await cmdLista.ExecuteReaderAsync())
                    {
                        while (await readerLista.ReadAsync())
                        {
                            var listaMoneda = new MonedaListDTO
                            {
                                codmoneda = (int)readerLista["codmoneda"],
                                nummoneda = (string)readerLista["nummoneda"],
                                desmoneda = (string)readerLista["desmoneda"]
                            };
                            lista.Add(listaMoneda);
                        }
                    }
                }

                await npgsql.CloseAsync();
            }
            return lista;
        }
    }
}
