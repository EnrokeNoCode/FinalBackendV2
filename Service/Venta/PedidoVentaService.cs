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
    
        public async Task<PedidoVentaDTO?> PedidoVentaConDet(int codpedidov)
        {
            PedidoVentaDTO? pedido = null;
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consultapedidocab_ = _query.Select();
                using (var cmdPedidoVenta = new NpgsqlCommand(consultapedidocab_, npgsql))
                {
                    cmdPedidoVenta.Parameters.AddWithValue("@codpedidov", codpedidov);
                    using (var reader = await cmdPedidoVenta.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            pedido = new PedidoVentaDTO
                            {
                                codpedidov = reader.GetInt32(reader.GetOrdinal("codpedidov")),
                                fechapedventa = reader.GetDateTime(reader.GetOrdinal("fechapedidov")),
                                numtipocomprobante = reader.GetString(reader.GetOrdinal("numtipocomprobante")),
                                numpedventa = reader.GetString(reader.GetOrdinal("numpedventa")),
                                nrodoc = reader.GetString(reader.GetOrdinal("nrodoc")),
                                cliente = reader.GetString(reader.GetOrdinal("cliente")),
                                vendedor = reader.GetString(reader.GetOrdinal("vendedor")),
                                desestmov = reader.GetString(reader.GetOrdinal("desestmov")),
                                nummoneda = reader.GetString(reader.GetOrdinal("nummoneda")),
                                dessucursal = reader.GetString(reader.GetOrdinal("dessucursal")),
                                totalpedidov = reader.GetDecimal(reader.GetOrdinal("totalpedidov")),
                                pedventadet = new List<PedidoVentaDetDTO>()
                            };
                        }
                    }
                }

                if (pedido == null)
                    return null;

                string consultaDetalle_ = _query.SelectWithDetails();
                using (var cmdDetalle = new NpgsqlCommand(consultaDetalle_, npgsql))
                {
                    cmdDetalle.Parameters.AddWithValue("@codpedidov", codpedidov);
                    using (var readerDet = await cmdDetalle.ExecuteReaderAsync())
                    {
                        while (await readerDet.ReadAsync())
                        {
                            var detalle = new PedidoVentaDetDTO
                            {
                                codpedidov = readerDet.GetInt32(readerDet.GetOrdinal("codpedidov")),
                                codproducto = readerDet.GetInt32(readerDet.GetOrdinal("codproducto")),
                                datoproducto = readerDet.GetString(readerDet.GetOrdinal("datoproducto")),
                                cantidad = readerDet.GetDecimal(readerDet.GetOrdinal("cantidad")),
                                precioventa = readerDet.GetDecimal(readerDet.GetOrdinal("precioventa")),
                                codiva = readerDet.GetInt32(readerDet.GetOrdinal("codiva")),
                                desiva = readerDet.GetString(readerDet.GetOrdinal("desiva"))
                            };
                            pedido.pedventadet?.Add(detalle);
                        }
                    }
                }
            }

            return pedido;
        }

        public async Task<string> ActualizarPedidoVentaDet(PedidoVentaUpdateDTO pedido)
        {
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();

                using (var transaction = await npgsql.BeginTransactionAsync())
                {
                    try
                    {
                        string actualizaDet = _query.Update(2);
                        using (var cmd = new NpgsqlCommand(actualizaDet, npgsql))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Transaction = transaction;

                            cmd.Parameters.AddWithValue("@codpedidov", pedido.codpedidov);
                            cmd.Parameters.AddWithValue("@totalpedidov", pedido.totalpedidov);

                            var detallesJson = JsonSerializer.Serialize(pedido.pedventadet);
                            cmd.Parameters.AddWithValue("@detalles", NpgsqlTypes.NpgsqlDbType.Json, detallesJson);

                            string updateMsg = (string)await cmd.ExecuteScalarAsync();
                            await transaction.CommitAsync();
                            return updateMsg;
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine("Error al actualizar PedidoVenta: " + ex.Message);
                        throw;
                    }
                }

            }
        }
    
    }
}