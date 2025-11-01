using Persistence;
using Npgsql; 
using System.Data;
using System.Text.Json;
using Model.DTO;
using Persistence.SQL.Compra;
using Model.DTO.Compras.PedidoCompra;

namespace Service.Compra
{
    public class PedidoCompraService
    {

        private readonly DBConnector _cn;
        private readonly PedidosCompras_sql _query;

        public PedidoCompraService(DBConnector cn, PedidosCompras_sql query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<PaginadoDTO<PedidoCompraListDTO>> PedidoCompraLista(int page, int pageSize)
        {
            var lista = new List<PedidoCompraListDTO>();
            int totalItems = 0;

            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();

                // ➕ Consulta para obtener el total de registros sin paginación
                using (var cmdCount = new NpgsqlCommand("SELECT COUNT(*) FROM purchase.pedidocompra", npgsql))
                {
                    totalItems = Convert.ToInt32(await cmdCount.ExecuteScalarAsync());
                }

                int offset = (page - 1) * pageSize;
                string consultapedidoc_ = _query.SelectList(pageSize, offset);

                using (var cmdPedidoCompra = new NpgsqlCommand(consultapedidoc_, npgsql))
                {
                    using (var readerPedCompra = await cmdPedidoCompra.ExecuteReaderAsync())
                    {
                        while (await readerPedCompra.ReadAsync())
                        {
                            var pedcompra_ = new PedidoCompraListDTO
                            {
                                codpedcompra = (int)readerPedCompra["codpedcompra"],
                                numsucursal = (string)readerPedCompra["numsucursal"],
                                dessucursal = (string)readerPedCompra["dessucursal"],
                                numtipocomprobante = (string)readerPedCompra["numtipocomprobante"],
                                destipocomprobante = (string)readerPedCompra["destipocomprobante"],
                                numpedcompra = (string)readerPedCompra["numpedcompra"],
                                fechapedcompra = (DateTime)readerPedCompra["fechapedcompra"],
                                numestmov = (string)readerPedCompra["numestmov"],
                                desestmov = (string)readerPedCompra["desestmov"],
                                empleado = (string)readerPedCompra["empleado"]
                            };
                            lista.Add(pedcompra_);
                        }
                    }
                }

                await npgsql.CloseAsync();
            }

            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new PaginadoDTO<PedidoCompraListDTO>
            {
                Data = lista,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<PedidoCompraDTO?> PedidoCompraConDet(int idpedcompra)
        {
            PedidoCompraDTO? pedido = null;
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consultapedidocab_ = _query.Select();
                using (var cmdPedidoCompra = new NpgsqlCommand(consultapedidocab_, npgsql))
                {
                    cmdPedidoCompra.Parameters.AddWithValue("@codpedcompra", idpedcompra);
                    using (var reader = await cmdPedidoCompra.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            pedido = new PedidoCompraDTO
                            {
                                codpedcompra = reader.GetInt32(reader.GetOrdinal("codpedcompra")),
                                fechapedcompra = reader.GetDateTime(reader.GetOrdinal("fechapedcompra")),
                                numtipocomprobante = reader.GetString(reader.GetOrdinal("numtipocomprobante")),
                                destipocomprobante = reader.GetString(reader.GetOrdinal("destipocomprobante")),
                                numpedcompra = reader.GetString(reader.GetOrdinal("numpedcompra")),
                                empleado = reader.GetString(reader.GetOrdinal("empleado")),
                                numestmov = reader.GetString(reader.GetOrdinal("numestmov")),
                                desestmov = reader.GetString(reader.GetOrdinal("desestmov")),
                                numsucursal = reader.GetString(reader.GetOrdinal("numsucursal")),
                                dessucursal = reader.GetString(reader.GetOrdinal("dessucursal")),
                                pedcompradet = new List<PedidoCompraDetDTO>()
                            };
                        }
                    }
                }

                if (pedido == null)
                    return null;

                string consultaDetalle_ = _query.SelectWithDetails();
                using (var cmdDetalle = new NpgsqlCommand(consultaDetalle_, npgsql))
                {
                    cmdDetalle.Parameters.AddWithValue("@codpedcompra", idpedcompra);
                    using (var readerDet = await cmdDetalle.ExecuteReaderAsync())
                    {
                        while (await readerDet.ReadAsync())
                        {
                            var detalle = new PedidoCompraDetDTO
                            {
                                codpedcompra = readerDet.GetInt32(readerDet.GetOrdinal("codpedcompra")),
                                codproducto = readerDet.GetInt32(readerDet.GetOrdinal("codproducto")),
                                codigobarra = readerDet.GetString(readerDet.GetOrdinal("codigobarra")),
                                desproducto = readerDet.GetString(readerDet.GetOrdinal("desproducto")),
                                cantidad = readerDet.GetDecimal(readerDet.GetOrdinal("cantidad")),
                                costoultimo = readerDet.GetDecimal(readerDet.GetOrdinal("costoultimo")),
                            };
                            pedido.pedcompradet?.Add(detalle);
                        }
                    }
                }
            }

            return pedido;
        }
        public async Task<string> ActualizarEstadoV2(int codpedcompra, int codestado)
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
                            cmd.Parameters.AddWithValue("@codpedcompra", codpedcompra);
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
        public async Task<int> ActualizarEstado(int codpedcompra, int codestado)
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
                            cmdValidar.Parameters.AddWithValue("@codpedcompra", codpedcompra);
                            cmdValidar.Transaction = transaction;
                            var estadoActual = await cmdValidar.ExecuteScalarAsync();

                            switch ((int)estadoActual)
                            {
                                case 2:
                                    throw new Exception("El pedido de compra ya fue utilizado.");
                                case 3:
                                    throw new Exception("El pedido de compra no se puede utilizar ya supero los dias.");
                                case 4:
                                    throw new Exception("El pedido de compra ya se encuentra anulado.");
                            }
                        }
                        string actulizarestado = _query.Update(1);

                        using (var cmd = new NpgsqlCommand(actulizarestado, npgsql))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@codpedcompra", codpedcompra);
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
        public async Task<int> InsertarPedidoCompra(PedidoCompraInsertDTO pedido)
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
                            cmd.Parameters.AddWithValue("@numpedcompra", pedido.numpedcompra);
                            cmd.Parameters.AddWithValue("@fechapedcompra", DateTime.Parse(pedido.fechapedcompra));
                            cmd.Parameters.AddWithValue("@codestmov", pedido.codestmov);
                            cmd.Parameters.AddWithValue("@codempleado", pedido.codempleado);
                            cmd.Parameters.AddWithValue("@codsucursal", pedido.codsucursal);
                            cmd.Parameters.AddWithValue("@codterminal", pedido.codterminal);
                            cmd.Parameters.AddWithValue("@ultimo", pedido.ultimo);

                            var detallesJson = JsonSerializer.Serialize(pedido.pedcompradet);
                            cmd.Parameters.AddWithValue("@detalles", NpgsqlTypes.NpgsqlDbType.Json, detallesJson);

                            int codpedcompra = (int)await cmd.ExecuteScalarAsync();

                            await transaction.CommitAsync();
                            return codpedcompra;
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine("Error al insertar PedidoCompra: " + ex.Message);
                        throw;
                    }
                }
            }
        }

        public async Task<string> ActualizarPedidoCompraDet(PedidoCompraUpdateDTO pedido)
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

                            cmd.Parameters.AddWithValue("@codpedcompra", pedido.codpedcompra);

                            var detallesJson = JsonSerializer.Serialize(pedido.pedcompradet);
                            cmd.Parameters.AddWithValue("@detalles", NpgsqlTypes.NpgsqlDbType.Json, detallesJson);

                            string updateMsg = (string)await cmd.ExecuteScalarAsync();
                            await transaction.CommitAsync();
                            return updateMsg;
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine("Error al actualizar PedidoCompra: " + ex.Message);
                        throw;
                    }
                }

            }
        }
    }
}