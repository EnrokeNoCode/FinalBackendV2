using Persistence.SQL.Referencial;
using Persistence;
using Model.DTO;
using Npgsql;

namespace Service.Referencial
{
    public class MarcaService
    {
        private readonly DBConnector _cn;
        private readonly MarcaSQL _query;

        public MarcaService(DBConnector cn, MarcaSQL query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<List<MarcaListDTO>> GetListMarca(bool marca)
        {
            var lista = new List<MarcaListDTO>();
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consultaLista = _query.SelectMarca();
                using (var cmdLista = new NpgsqlCommand(consultaLista, npgsql))
                {
                    cmdLista.Parameters.AddWithValue("@marca", marca);
                    using (var readerLista = await cmdLista.ExecuteReaderAsync())
                    {
                        while (await readerLista.ReadAsync())
                        {
                            var listaMarca = new MarcaListDTO
                            {
                                codmarca = (int)readerLista["codmarca"],
                                nummarca = (string)readerLista["nummarca"],
                                desmarca = (string)readerLista["desmarca"]
                            };
                            lista.Add(listaMarca);
                        }
                    }
                }

                await npgsql.CloseAsync();
            }
            return lista;
        }
    }
}
