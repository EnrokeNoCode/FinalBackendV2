using Persistence.SQL.Referencial;
using Persistence;
using Model.DTO;
using Npgsql;

namespace Service.Referencial
{
    public class CobradorService
    {
        private readonly DBConnector _cn;
        private readonly CobradorSQL _query;

        public CobradorService(DBConnector cn, CobradorSQL query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<List<CobradorListDTO>> GetCobradorList()
        {
            var lista = new List<CobradorListDTO>();
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consultaLista = _query.SelectCobradorCaja();
                using (var cmdLista = new NpgsqlCommand(consultaLista, npgsql))
                {
                    using (var readerLista = await cmdLista.ExecuteReaderAsync())
                    {
                        while (await readerLista.ReadAsync())
                        {
                            var listaCobrador = new CobradorListDTO
                            {
                                codcobrador = (int)readerLista["codcobrador"],
                                datocobrador = (string)readerLista["datocobrador"]
                            };
                            lista.Add(listaCobrador);
                        }
                    }
                }

                await npgsql.CloseAsync();
            }
            return lista;
        }
    }
}
