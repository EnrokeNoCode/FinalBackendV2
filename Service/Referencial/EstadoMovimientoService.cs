using Persistence.SQL.Referencial;
using Persistence;
using Model.DTO;
using Npgsql;

namespace Service.Referencial
{
    public class EstadoMovimientoService
    {
        private readonly DBConnector _cn;
        private readonly EstadoMovimientoSQL _query;

        public EstadoMovimientoService(DBConnector cn, EstadoMovimientoSQL query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<List<EstadoMovimientoListDTO>> GetListEstadoMovimiento()
        {
            var lista = new List<EstadoMovimientoListDTO>();
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consultaLista = _query.SelectEstadoMovimiento();
                using (var cmdLista = new NpgsqlCommand(consultaLista, npgsql))
                {
                    using (var readerLista = await cmdLista.ExecuteReaderAsync())
                    {
                        while (await readerLista.ReadAsync())
                        {
                            var listaEstadoMov = new EstadoMovimientoListDTO
                            {
                                codestmov = (int)readerLista["codestmov"],
                                numestmov = (string)readerLista["numestmov"],
                                desestmov = (string)readerLista["desestmov"]
                            };
                            lista.Add(listaEstadoMov);
                        }
                    }
                }

                await npgsql.CloseAsync();
            }
            return lista;
        }
    }
}
