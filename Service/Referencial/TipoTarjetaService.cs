using Persistence.SQL.Referencial;
using Persistence;
using Model.DTO;
using Npgsql;

namespace Service.Referencial
{
    public class TipoTarjetaService
    {
        private readonly DBConnector _cn;
        private readonly TipoTarjetaSQL _query;

        public TipoTarjetaService(DBConnector cn, TipoTarjetaSQL query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<List<TipoTarjetaDTO>> GetListTipoTarjeta()
        {
            var lista = new List<TipoTarjetaDTO>();
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
                            var listaTarjeta = new TipoTarjetaDTO
                            {
                                codtipotar = (int)readerLista["codtipotar"],
                                numtipotar = (string)readerLista["numtipotar"],
                                destipotar = (string)readerLista["destipotar"]
                            };
                            lista.Add(listaTarjeta);
                        }
                    }
                }

                await npgsql.CloseAsync();
            }
            return lista;
        }
    }
}
