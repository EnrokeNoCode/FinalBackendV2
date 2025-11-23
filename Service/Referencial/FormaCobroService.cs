using Persistence.SQL.Referencial;
using Persistence;
using Persistence.SQL;
using Model.DTO;
using Npgsql;

namespace Service.Referencial
{
    public class FormaCobroService
    {
        private readonly DBConnector _cn;
        private readonly FormaCobroSQL _query;

        public FormaCobroService(DBConnector cn, FormaCobroSQL query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<List<FormaCobroDTO>> GetFormaCobro()
        {
            var lista = new List<FormaCobroDTO>();
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consultaLista = _query.Select(1);
                using (var cmdLista = new NpgsqlCommand(consultaLista, npgsql))
                {
                    using (var readerLista = await cmdLista.ExecuteReaderAsync())
                    {
                        while (await readerLista.ReadAsync())
                        {
                            var listaFormaCobro = new FormaCobroDTO
                            {
                                codformacobro = (int)readerLista["codformacobro"],
                                numformacobro = (string)readerLista["numformacobro"],
                                desformacobro = (string)readerLista["desformacobro"],
                                tipo = (string)readerLista["tipo"]
                            };
                            lista.Add(listaFormaCobro);
                        }
                    }
                }

                await npgsql.CloseAsync();
            }
            return lista;
        }
    }
}
