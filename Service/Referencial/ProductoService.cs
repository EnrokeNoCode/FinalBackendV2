using Persistence.SQL.Referencial;
using Persistence;
using Model.DTO;
using Npgsql;

namespace Service.Referencial
{
    public class ProductoService
    {
        private readonly DBConnector _cn;
        private readonly ProductoSQL _query;

        public ProductoService(DBConnector cn, ProductoSQL query)
        {
            _cn = cn;
            _query = query;
        }

        public static decimal CalcularPrecioBruto(decimal precioVenta, int codiva)
        {
            return precioVenta + (codiva switch
            {
                1 => precioVenta * 0.05m, // 5%
                2 => precioVenta * 0.10m, // 10%
                _ => 0 // Exento
            });
        }

        public async Task<PaginadoDTO<ProductoListDTO>> GetProductoLista(int page, int pageSize)
        {
            var lista = new List<ProductoListDTO>();
            int totalItems = 0;
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var cmdCount = new NpgsqlCommand($"SELECT COUNT(*) FROM referential.producto;", npgsql))
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
                string consulta = _query.SelectProducto(pageSize, offset);
                using (var cmdLista = new NpgsqlCommand(consulta, npgsql))
                {
                    using (var reader = await cmdLista.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var listaProducto = new ProductoListDTO
                            {
                                codproducto = (int)reader["codproducto"],
                                datoproducto = (string)reader["datoproducto"],
                                datoproveedor = (string)reader["datoproveedor"],
                                datoiva = (string)reader["datoiva"],
                                afectastock = (string)reader["afectastock"],
                                estado = (string)reader["estado"],
                                datoseccion = "Se enlaza la BD recuperar los datos",
                                costoultimo = (decimal)reader["costoultimo"]
                            };
                            lista.Add(listaProducto);
                        }
                    }
                }

                await npgsql.CloseAsync();
            }
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new PaginadoDTO<ProductoListDTO>
            {
                Data = lista,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<List<ProductoListCompraDTO>> GetProductoCompra()
        {
            var lista = new List<ProductoListCompraDTO>();
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consultaLista = _query.SelectProductoCompra();
                using (var cmdLista = new NpgsqlCommand(consultaLista, npgsql))
                {
                    using (var readerLista = await cmdLista.ExecuteReaderAsync())
                    {
                        while (await readerLista.ReadAsync())
                        {
                            var listaProducto = new ProductoListCompraDTO
                            {
                                codproducto = (int)readerLista["codproducto"],
                                codigobarra = (string)readerLista["codigobarra"],
                                desproducto = (string)readerLista["desproducto"],
                                proveedor = (string)readerLista["datoproveedor"],
                                codiva = (int)readerLista["codiva"],
                                desiva = (string)readerLista["desiva"],
                                coheficiente = (decimal)readerLista["coheficiente"],
                                costoultimo = (decimal)readerLista["costoultimo"],
                            };
                            lista.Add(listaProducto);
                        }
                    }
                }

                await npgsql.CloseAsync();
            }
            return lista;
        }

        public async Task<List<ProductoVentaListDTO>> GetProductoVenta(int codsucursal)
        {
            var lista = new List<ProductoVentaListDTO>();
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consultaLista = _query.SelectProductoVenta();
                using (var cmdLista = new NpgsqlCommand(consultaLista, npgsql))
                {
                    cmdLista.Parameters.AddWithValue("codsucursal", codsucursal);
                    using (var readerLista = await cmdLista.ExecuteReaderAsync())
                    {
                        while (await readerLista.ReadAsync())
                        {
                            var listaProducto = new ProductoVentaListDTO
                            {
                                codproducto = (int)readerLista["codproducto"],
                                datoproducto = (string)readerLista["datoproducto"],
                                preciobruto = CalcularPrecioBruto((decimal)readerLista["precioventa"], (int)readerLista["codiva"]),
                                precioneto = (decimal)readerLista["precioventa"],
                                cantidad = (decimal)readerLista["cantidad"],
                                desiva = (string)readerLista["desiva"],
                                codiva = (int)readerLista["codiva"]
                            };
                            lista.Add(listaProducto);
                        }
                    }
                }

                await npgsql.CloseAsync();
            }
            return lista;
        }
    }
}
