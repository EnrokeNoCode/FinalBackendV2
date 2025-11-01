using System.Data;
using System.Text.Json;
using Model.DTO;
using Model.DTO.Ventas.PedidoVenta;
using Npgsql;
using Persistence;
using Persistence.SQL.Venta;

namespace Service.Venta
{
    public class PedidoVentaService
    {
        private readonly DBConnector _cn;
        private readonly PedidoVenta_sql _query;

        public PedidoVentaService(DBConnector cn, PedidoVenta_sql query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<PaginadoDTO<PedidoVentaListDTO>> PedidoVentaLista(int page, int pageSize)
        {
            var lista = new List<PedidoVentaListDTO>();
            int totalItems = 0;
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var cmdCount = new NpgsqlCommand("SELECT COUNT(*) FROM sales.pedidoventa", npgsql))
                {
                    totalItems = Convert.ToInt32(await cmdCount.ExecuteScalarAsync());
                }

                int offset = (page - 1) * pageSize;
                string consultapedidov_ = _query.SelectList(pageSize, offset);
                using (var cmdPedidoVenta = new NpgsqlCommand(consultapedidov_, npgsql))
                {
                    using (var readerPedVenta = await cmdPedidoVenta.ExecuteReaderAsync())
                    {
                        while (await readerPedVenta.ReadAsync())
                        {
                            var pedventa_ = new PedidoVentaListDTO
                            {
                                codpedidov = (int)readerPedVenta["codpedidov"],
                                fechapedidov = (DateTime)readerPedVenta["fechapedidov"],
                                numpedventa = (string)readerPedVenta["numpedventa"],
                                cliente = (string)readerPedVenta["cliente"],
                                vendedor = (string)readerPedVenta["vendedor"],
                                desestmov = (string)readerPedVenta["desestmov"],
                                sucursal = (string)readerPedVenta["sucursal"],
                                totalpedidov = (decimal)readerPedVenta["totalpedidov"]
                            };
                            lista.Add(pedventa_);
                        }
                    }
                }
                await npgsql.CloseAsync();
            }
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new PaginadoDTO<PedidoVentaListDTO>
            {
                Data = lista,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<List<PedidoVentaListDTO>> PedidoVentaLista(int codcliente)
        {
            var lista = new List<PedidoVentaListDTO>();

            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consultaPedidoVenta = _query.SelectListPrst();
                using (var cmdPedidoVenta = new NpgsqlCommand(consultaPedidoVenta, npgsql))
                {
                    cmdPedidoVenta.Parameters.AddWithValue("@codcliente", codcliente);

                    using (var readerPedVenta = await cmdPedidoVenta.ExecuteReaderAsync())
                    {
                        while (await readerPedVenta.ReadAsync())
                        {
                            var pedVenta = new PedidoVentaListDTO
                            {
                                codpedidov = readerPedVenta.GetInt32(readerPedVenta.GetOrdinal("codpedidov")),
                                fechapedidov = readerPedVenta.GetDateTime(readerPedVenta.GetOrdinal("fechapedidov")),
                                numpedventa = readerPedVenta.GetString(readerPedVenta.GetOrdinal("numpedventa")),
                                cliente = readerPedVenta.GetString(readerPedVenta.GetOrdinal("cliente")),
                                vendedor = readerPedVenta.GetString(readerPedVenta.GetOrdinal("vendedor")),
                                desestmov = readerPedVenta.GetString(readerPedVenta.GetOrdinal("desestmov")),
                                sucursal = readerPedVenta.GetString(readerPedVenta.GetOrdinal("sucursal")),
                                totalpedidov = readerPedVenta.GetDecimal(readerPedVenta.GetOrdinal("totalpedidov"))
                            };

                            lista.Add(pedVenta);
                        }
                    }
                }

                await npgsql.CloseAsync();
            }

            return lista;
        }

        public async Task<string> ActualizarEstadoV2(int codpedidov, int codestado)
        {

            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var transaction = await npgsql.BeginTransactionAsync())
                {
                    try
                    {
                        string actulizarestado = _query.Update(3);
                        using (var cmd = new NpgsqlCommand(actulizarestado, npgsql))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@codpedidov", codpedidov);
                            cmd.Parameters.AddWithValue("@codestado", codestado);
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

        public async Task<int> ActualizarEstado(int codpedventa, int codestado)
        {
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var transaction = await npgsql.BeginTransactionAsync())
                {
                    try
                    {
                        string consultaEstado = _query.SelectStatus();

                        using (var cmdValidar = new NpgsqlCommand(consultaEstado, npgsql))
                        {
                            cmdValidar.Parameters.AddWithValue("@codpedventa", codpedventa);
                            cmdValidar.Transaction = transaction;
                            var estadoActual = await cmdValidar.ExecuteScalarAsync();

                            switch ((int)estadoActual)
                            {
                                case 2:
                                    throw new Exception("El pedido de venta ya fue utilizado.");
                                case 3:
                                    throw new Exception("El pedido de venta no se puede utilizar ya supero los dias.");
                                case 4:
                                    throw new Exception("El pedido de centa ya se encuentra anulado.");
                            }
                        }
                        string actulizarestado = _query.Update(1);

                        using (var cmd = new NpgsqlCommand(actulizarestado, npgsql))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@codpedventa", codpedventa);
                            cmd.Parameters.AddWithValue("@codestado", codestado);
                            cmd.Transaction = transaction;
                            int filasAfectadas = await cmd.ExecuteNonQueryAsync();
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
        
        public async Task<int> InsertarPedidoVenta(PedidoVentaInsertDTO pedido)
        {
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();

                using (var transaction = await npgsql.BeginTransactionAsync())
                {
                    try
                    {
                        string insertarPedido = _query.Insert();

                        using (var cmd = new NpgsqlCommand(insertarPedido, npgsql))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Transaction = transaction;

                            cmd.Parameters.AddWithValue("@codtipocomprobante", pedido.codtipocomprobante);
                            cmd.Parameters.AddWithValue("@codsucursal", pedido.codsucursal);
                            cmd.Parameters.AddWithValue("@codestmov", pedido.codestmov);
                            cmd.Parameters.AddWithValue("@fechapedidov", pedido.fechapedidov);
                            cmd.Parameters.AddWithValue("@numpedventa", pedido.numpedventa);
                            cmd.Parameters.AddWithValue("@codvendedor", pedido.codvendedor);
                            cmd.Parameters.AddWithValue("@codcliente", pedido.codcliente);
                            cmd.Parameters.AddWithValue("@codmoneda", pedido.codmoneda);
                            cmd.Parameters.AddWithValue("@totalpedidov", pedido.totalpedidov);
                            cmd.Parameters.AddWithValue("@cotizacion1", pedido.cotizacion1);
                            cmd.Parameters.AddWithValue("@ultimo", pedido.ultimo);
                            cmd.Parameters.AddWithValue("@codterminal", pedido.codterminal);
                            var detallesJson = JsonSerializer.Serialize(pedido.pedventadet);
                            cmd.Parameters.AddWithValue("@detalles", NpgsqlTypes.NpgsqlDbType.Json, detallesJson);

                            int codpedcompra = (int)await cmd.ExecuteScalarAsync();

                            await transaction.CommitAsync();
                            return codpedcompra;
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine("Error al insertar Pedido Venta: " + ex.Message);
                        throw;
                    }
                }
            }
        }
    }
}