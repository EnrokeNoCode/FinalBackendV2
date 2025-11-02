using Persistence.SQL.Referencial;
using Persistence;
using Model.DTO;
using Npgsql;

namespace Service.Referencial
{
    public class MotivoAjusteService
    {
        private readonly DBConnector _cn;
        private readonly MotivoAjusteSQL _query;

        public MotivoAjusteService(DBConnector cn, MotivoAjusteSQL query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<List<MotivoAjusteListDTO>> GetListMotivoAjuste()
        {
            var lista = new List<MotivoAjusteListDTO>();
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consultaLista = _query.SelectMotivoAjuste();
                using (var cmdLista = new NpgsqlCommand(consultaLista, npgsql))
                {
                    using (var readerLista = await cmdLista.ExecuteReaderAsync())
                    {
                        while (await readerLista.ReadAsync())
                        {
                            var listaMotivoAjuste = new MotivoAjusteListDTO
                            {
                                codmotivo = (int)readerLista["codmotivo"],
                                desmotivo = (string)readerLista["desmotivo"],
                            };
                            lista.Add(listaMotivoAjuste);
                        }
                    }
                }

                await npgsql.CloseAsync();
            }
            return lista;
        }
    }
}
