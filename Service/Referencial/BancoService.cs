using Persistence.SQL.Referencial;
using Persistence;
using Model.DTO;
using Npgsql;

namespace Service.Referencial
{
    public class BancoService
    {
        private readonly DBConnector _cn;
        private readonly BancoSQL _query;

        public BancoService(DBConnector cn, BancoSQL query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<List<BancoDTO>> GetListBanco()
        {
            var lista = new List<BancoDTO>();
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consultaLista = _query.Select();
                using (var cmdLista = new NpgsqlCommand(consultaLista, npgsql))
                {
                    using (var readerLista = await cmdLista.ExecuteReaderAsync())
                    {
                        while (await readerLista.ReadAsync())
                        {
                            var listaBanco = new BancoDTO
                            {
                                codbanco = (int)readerLista["codbanco"],
                                numbanco = (string)readerLista["numbanco"],
                                desbanco = (string)readerLista["desbanco"]
                            };
                            lista.Add(listaBanco);
                        }
                    }
                }

                await npgsql.CloseAsync();
            }
            return lista;
        }
    }
}
