using Persistence.SQL.Referencial;
using Persistence;
using Model.DTO;
using Npgsql;
using System.Data;
using Model.DTO.Referencial;

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
                                datoseccion = (string)reader["datoseccion"],
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

        public async Task<ProductoGetDTO> ObtenerProducto(int codproducto)
        {
            using var cn = new NpgsqlConnection(_cn.cadenaSQL());
            await cn.OpenAsync();

            string selectProducto = _query.SelectProductoMod();

            using var cmd = new NpgsqlCommand(selectProducto, cn);
            cmd.Parameters.AddWithValue("@codproducto", codproducto);

            using var dr = await cmd.ExecuteReaderAsync();
            if (!await dr.ReadAsync()) return null;

            return new ProductoGetDTO
            {
                codproducto = dr.GetInt32(0),
                codigobarra = dr.GetString(1),
                desproducto = dr.GetString(2),
                codproveedor = dr.GetInt32(3),
                datoproveedor = dr.GetString(4),
                codiva = dr.GetInt32(5),
                desiva = dr.GetString(6),
                afectastock = dr.GetBoolean(7),
                activo = dr.GetBoolean(8),
                codmarca = dr.GetInt32(9),
                desmarca = dr.GetString(10),
                codfamilia = dr.GetInt32(11),
                desfamilia = dr.GetString(12),
                codrubro = dr.GetInt32(13),
                desrubro = dr.GetString(14),
                costoultimo = dr.GetDecimal(15)
            };
        }

        public async Task<string> ActualizarProducto(ProductoUpdateDTO dto)
        {
            using var cn = new NpgsqlConnection(_cn.cadenaSQL());
            await cn.OpenAsync();

            string actualizarProducto = _query.UpdateProducto();

            using var cmd = new NpgsqlCommand(actualizarProducto, cn);

            cmd.Parameters.AddWithValue("@codproducto", dto.codproducto);
            cmd.Parameters.AddWithValue("@codigobarra", dto.codigobarra);
            cmd.Parameters.AddWithValue("@desproducto", dto.desproducto);
            cmd.Parameters.AddWithValue("@codproveedor", dto.codproveedor);
            cmd.Parameters.AddWithValue("@codmarca", dto.codmarca);
            cmd.Parameters.AddWithValue("@codfamilia", dto.codfamilia);
            cmd.Parameters.AddWithValue("@codrubro", dto.codrubro);
            cmd.Parameters.AddWithValue("@codiva", dto.codiva);
            cmd.Parameters.AddWithValue("@afectastock", dto.afectastock);
            cmd.Parameters.AddWithValue("@costoultimo", dto.costoultimo);

            int filas = await cmd.ExecuteNonQueryAsync();
            if (filas == 0)
                throw new Exception("No se pudo actualizar el producto");

            return "Producto actualizado correctamente";
        }

        public async Task<string> InsertarNuevoProducto(ProductoInsertDTO producto)
        {
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var transaction = await npgsql.BeginTransactionAsync())
                {
                    try
                    {
                        string insertarProducto = _query.Insert();
                        using (var cmd = new NpgsqlCommand(insertarProducto, npgsql))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Transaction = transaction;

                            cmd.Parameters.AddWithValue("@codigobarra", producto.codigobarra);
                            cmd.Parameters.AddWithValue("@desproducto", producto.desproducto);
                            cmd.Parameters.AddWithValue("@codfamilia", producto.codfamilia);
                            cmd.Parameters.AddWithValue("@codmarca", producto.codmarca);
                            cmd.Parameters.AddWithValue("@codrubro", producto.codrubro);
                            cmd.Parameters.AddWithValue("@codunidadmedida", producto.codunidadmedida);
                            cmd.Parameters.AddWithValue("@codiva", producto.codiva);
                            cmd.Parameters.AddWithValue("@codproveedor", producto.codproveedor);
                            cmd.Parameters.AddWithValue("@costoultimo", producto.costoultimo);
                            cmd.Parameters.AddWithValue("@activo", producto.activo);
                            cmd.Parameters.AddWithValue("@afectastock", producto.afectastock);
                            string resultado = (string)await cmd.ExecuteScalarAsync();
                            await transaction.CommitAsync();
                            return resultado;
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine("Error al insertar el Producto: " + ex.Message);
                        throw;
                    }
                }
            }
        }

        public async Task<string> ActulizarEliminarRegistro(int cod)
        {

            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var transaction = await npgsql.BeginTransactionAsync())
                {
                    try
                    {
                        string actulizarestado = _query.UpdateDeleteStatus();
                        using (var cmd = new NpgsqlCommand(actulizarestado, npgsql))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@cod", cod);
                            cmd.Transaction = transaction;
                            string filasAfectadas = (string)await cmd.ExecuteScalarAsync();
                            await transaction.CommitAsync();
                            return filasAfectadas;
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine($"Error: {ex.Message}");
                        throw;
                    }
                }
            }
        }
    }
}
