using Persistence.SQL.Referencial;
using Persistence;
using Model.DTO;
using Npgsql;

namespace Service.Referencial
{
    public class TemporalService
    {
        private readonly DBConnector _cn;
        private readonly TemporalSQL _query;

        public TemporalService(DBConnector cn, TemporalSQL query)
        {
            _cn = cn;
            _query = query;
        }
        public async Task<List<TemporalDTO>> GetListTemporal(string tablename)
        {
            var lista = new List<TemporalDTO>();
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consultaLista = _query.SelectTemporal(tablename);
                using (var cmdLista = new NpgsqlCommand(consultaLista, npgsql))
                {
                    using (var readerLista = await cmdLista.ExecuteReaderAsync())
                    {
                        while (await readerLista.ReadAsync())
                        {
                            var listaTemporal = new TemporalDTO
                            {
                                Codigo = (int)readerLista["Codigo"],
                                Numero = (string)readerLista["Numero"],
                                Descripcion = (string)readerLista["Descripcion"]
                            };
                            lista.Add(listaTemporal);
                        }
                    }
                }

                await npgsql.CloseAsync();
            }
            return lista;
        }
    }
}
