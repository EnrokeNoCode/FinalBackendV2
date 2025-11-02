using Persistence.SQL.Referencial;
using Persistence;
using Model.DTO;
using Npgsql;

namespace Service.Referencial
{
    public class ProveedorService
    {
        private readonly DBConnector _cn;
        private readonly ProveedorSQL _query;

        public ProveedorService(DBConnector cn, ProveedorSQL query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<List<ProveedorListCompraDTO>> GetProveedorCompra()
        {
            var lista = new List<ProveedorListCompraDTO>();
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consultaLista = _query.SelectProveedorCompra();
                using (var cmdLista = new NpgsqlCommand(consultaLista, npgsql))
                {
                    using (var readerLista = await cmdLista.ExecuteReaderAsync())
                    {
                        while (await readerLista.ReadAsync())
                        {
                            var listaProveedorCompra = new ProveedorListCompraDTO
                            {
                                codproveedor = (int)readerLista["codproveedor"],
                                nrodocprv = (string)readerLista["nrodocprv"],
                                razonsocial = (string)readerLista["razonsocial"],
                                nrotimbrado = (string)readerLista["nrotimbrado"],
                                fechaventimbrado = Convert.ToString((DateTime)readerLista["fechaventimbrado"])
                            };
                            lista.Add(listaProveedorCompra);
                        }
                    }
                }
                await npgsql.CloseAsync();
            }
            return lista;
        }

        public async Task<PaginadoDTO<ProveedorListDTO>> GetProveedor( int page, int pageSize)
        {
            var lista = new List<ProveedorListDTO>();
            int totalItems = 0;
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var cmdCount = new NpgsqlCommand($"SELECT COUNT(*) FROM referential.proveedor;", npgsql))
                {
                    try
                    {
                        totalItems = Convert.ToInt32(await cmdCount.ExecuteScalarAsync());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                int offset = (page - 1) * pageSize;
                string consulta = _query.SelectProveedor(pageSize, offset);
                using (var cmd = new NpgsqlCommand(consulta, npgsql))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            try
                            {
                                var listaProveedor = new ProveedorListDTO
                                {
                                    codproveedor = (int)reader["codproveedor"],
                                    proveedor = (string)reader["proveedor"],
                                    activo = (string)reader["activo"],
                                    datofacturacion = (string)reader["datofacturacion"],
                                    fecha = (DateTime)reader["fecha"],
                                    datocontacto = (string)reader["datocontacto"]
                                };
                                lista.Add(listaProveedor);
                            }
                            catch (Exception ex) { 
                            }
                            
                        }
                    }
                }
                await npgsql.CloseAsync();
            }
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new PaginadoDTO<ProveedorListDTO>
            {
                Data = lista,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }
    }
}
