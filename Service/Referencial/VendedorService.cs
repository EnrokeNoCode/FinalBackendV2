using Persistence.SQL.Referencial;
using Persistence;
using Model.DTO;
using Npgsql;

namespace Service.Referencial
{
    public class VendedorService
    {
        private readonly DBConnector _cn;
        private readonly VendedorSQL _query;

        public VendedorService(DBConnector cn, VendedorSQL query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<List<VendedorListDTO>> GetListVendedor()
        {
            var lista = new List<VendedorListDTO>();
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consultaLista = _query.SelectListVendedor();
                using (var cmdLista = new NpgsqlCommand(consultaLista, npgsql))
                {
                    using (var readerLista = await cmdLista.ExecuteReaderAsync())
                    {
                        while (await readerLista.ReadAsync())
                        {
                            var listaVendedor = new VendedorListDTO
                            {
                                codvendedor = (int)readerLista["codvendedor"],
                                datovendedor = (string)readerLista["datovendedor"]
                            };
                            lista.Add(listaVendedor);
                        }
                    }
                }

                await npgsql.CloseAsync();
            }
            return lista;
        }
    }
}
