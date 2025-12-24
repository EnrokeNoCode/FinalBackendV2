using Persistence.SQL.Referencial;
using Persistence;
using Model.DTO;
using Npgsql;

namespace Service.Referencial
{
    public class TipoComprobanteService
    {
        private readonly DBConnector _cn;
        private readonly TipoComprobanteSQL _query;

        public TipoComprobanteService(DBConnector cn, TipoComprobanteSQL query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<List<TipoComprobanteListDTO>> GetListTipoComprobanteMov(string tipomov, bool marca)
        {
            var lista = new List<TipoComprobanteListDTO>();
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consultaLista = _query.SelectTipoComprobanteMov();
                using (var cmdLista = new NpgsqlCommand(consultaLista, npgsql))
                {
                    cmdLista.Parameters.AddWithValue("@tipomov", tipomov);
                    cmdLista.Parameters.AddWithValue("@marca", marca);
                    using (var readerLista = await cmdLista.ExecuteReaderAsync())
                    {
                        try
                        {
                            while (await readerLista.ReadAsync())
                            {
                                var listaComprobante = new TipoComprobanteListDTO
                                {
                                    codtipocomprobante = (int)readerLista["codtipocomprobante"],
                                    numtipocomprobante = (string)readerLista["numtipocomprobante"],
                                    destipocomprobante = (string)readerLista["destipocomprobante"]
                                };
                                lista.Add(listaComprobante);
                            }
                        }
                        catch(Exception ex)
                        {
                            
                        }
                    }
                }
                await npgsql.CloseAsync();
            }
            return lista;
        }
    }
}
