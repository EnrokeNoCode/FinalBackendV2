using System.Data;
using System.Text.Json;
using Model.DTO;
using Model.DTO.Ventas.PresupuestoVenta;
using Npgsql;
using Persistence;
using Persistence.SQL.Venta;
using Utils;

namespace Service.Venta
{
    public class PresupuestoVentaService
    {
        private readonly DBConnector _cn;
        private readonly PresupuestoVenta_sql _query;

        public PresupuestoVentaService(DBConnector cn, PresupuestoVenta_sql query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<PaginadoDTO<PresupuestoVentaListDTO>> PresupuestoVentaLista(int page, int pageSize)
        {
            var lista = new List<PresupuestoVentaListDTO>();
            int totalItems = 0;
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var cmdCount = new NpgsqlCommand("SELECT COUNT(*) FROM sales.presupuestoventa", npgsql))
                {
                    totalItems = Convert.ToInt32(await cmdCount.ExecuteScalarAsync());
                }

                int offset = (page - 1) * pageSize;
                string consultapresupuestov_ = _query.SelectList(pageSize, offset);
                using (var cmdPresupuestoVenta = new NpgsqlCommand(consultapresupuestov_, npgsql))
                {
                    using (var readerPrstVenta = await cmdPresupuestoVenta.ExecuteReaderAsync())
                    {
                        while (await readerPrstVenta.ReadAsync())
                        {
                            var prstventa_ = new PresupuestoVentaListDTO
                            {
                                codpresupuestoventa = (int)readerPrstVenta["codpresupuestoventa"],
                                fechapresupuestoventa = (DateTime)readerPrstVenta["fechapresupuestoventa"],
                                numpresupuestoventa = (string)readerPrstVenta["numpresupuestoventa"],
                                cliente = (string)readerPrstVenta["cliente"],
                                vendedor = (string)readerPrstVenta["vendedor"],
                                desestmov = (string)readerPrstVenta["desestmov"],
                                sucursal = (string)readerPrstVenta["sucursal"],
                                datopedidoventa = (string)readerPrstVenta["datopedidoventa"],
                                condicionpago = (string)readerPrstVenta["condicionpago"],
                                totalpresupuestoventa = (decimal)readerPrstVenta["totalpresupuestoventa"]
                            };
                            lista.Add(prstventa_);
                        }
                    }
                }
                await npgsql.CloseAsync();
            }
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new PaginadoDTO<PresupuestoVentaListDTO>
            {
                Data = lista,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            }; ;
        }

        public async Task<string> ActualizarEstadoV2(int codpresupuestoventa, int codestado)
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
                            cmd.Parameters.AddWithValue("@codpresupuestoventa", codpresupuestoventa);
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
        public async Task<int> ActualizarEstado(int codpresupuestoventa, int codestado)
        {
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var transaction = await npgsql.BeginTransactionAsync())
                {
                    try
                    {
                        string consultaEstado = _query.Select(1);

                        using (var cmdValidar = new NpgsqlCommand(consultaEstado, npgsql))
                        {
                            cmdValidar.Parameters.AddWithValue("@codpresupuestoventa", codpresupuestoventa);
                            cmdValidar.Transaction = transaction;
                            var estadoActual = await cmdValidar.ExecuteScalarAsync();

                            switch ((int)estadoActual)
                            {
                                case 2:
                                    throw new Exception("El presupuesto ya fue utilizado.");
                                case 3:
                                    throw new Exception("El presupuesto no se puede utilizar ya supero los dias.");
                                case 4:
                                    throw new Exception("El presupuesto ya se encuentra anulado.");
                            }
                        }
                        string actulizarestado = _query.Update(1);

                        using (var cmd = new NpgsqlCommand(actulizarestado, npgsql))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@codpresupuestoventa", codpresupuestoventa);
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
        public async Task<int> InsertarPresupuestoVenta(PresupuestoVentaInsertDTO presupuestov)
        {
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();

                using (var transaction = await npgsql.BeginTransactionAsync())
                {
                    try
                    {
                        string insertarPresupuesto = _query.Insert();

                        using (var cmd = new NpgsqlCommand(insertarPresupuesto, npgsql))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Transaction = transaction;

                            cmd.Parameters.AddWithValue("@codtipocomprobante", presupuestov.codtipocomprobante);
                            cmd.Parameters.AddWithValue("@codsucursal", presupuestov.codsucursal);
                            cmd.Parameters.AddWithValue("@codvendedor", presupuestov.codvendedor);
                            cmd.Parameters.AddWithValue("@codcliente", presupuestov.codcliente);
                            cmd.Parameters.AddWithValue("@fechapresupuesto", presupuestov.fechapresupuestoventa);
                            cmd.Parameters.AddWithValue("@numprstventa", presupuestov.numpresupuestoventa ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@codpedidov", presupuestov.codpedidov ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@observacion", presupuestov.observacion ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@diaven", presupuestov.diaven);
                            cmd.Parameters.AddWithValue("@condicionpago", presupuestov.condicionpago);
                            cmd.Parameters.AddWithValue("@codmoneda", presupuestov.codmoneda);
                            cmd.Parameters.AddWithValue("@cotizacion", presupuestov.cotizacion1);
                            cmd.Parameters.AddWithValue("@codestmov", presupuestov.codestmov);
                            cmd.Parameters.AddWithValue("@totaliva", presupuestov.totaliva);
                            cmd.Parameters.AddWithValue("@totaldescuento", presupuestov.totaldescuento);
                            cmd.Parameters.AddWithValue("@totalexento", presupuestov.totalexento);
                            cmd.Parameters.AddWithValue("@totalgravada", presupuestov.totalgravada);
                            cmd.Parameters.AddWithValue("@totalpresupuestoventa", presupuestov.totalpresupuestoventa);
                            cmd.Parameters.AddWithValue("@codterminal", presupuestov.codterminal);
                            cmd.Parameters.AddWithValue("@ultimo", presupuestov.ultimo);
                            var detallesJson = JsonSerializer.Serialize(presupuestov.presventadet);
                            cmd.Parameters.AddWithValue("@detalles", NpgsqlTypes.NpgsqlDbType.Json, detallesJson);

                            int codpresupuestoventa = (int)await cmd.ExecuteScalarAsync();

                            await transaction.CommitAsync();
                            return codpresupuestoventa;
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine("Error al insertar el Presupuesto del Cliente: " + ex.Message);
                        throw;
                    }
                }
            }
        }

        public async Task<List<PresupuestoVentaDTO>> PresupuestoVentaxCliente(int codcliente)
        {
            var lista = new List<PresupuestoVentaDTO>();

            using var npgsql = new NpgsqlConnection(_cn.cadenaSQL());
            await npgsql.OpenAsync();

            using (var cmd = new NpgsqlCommand(_query.Select(2), npgsql))
            {
                cmd.Parameters.AddWithValue("@codcliente", codcliente);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    lista.Add(reader.MapToObject<PresupuestoVentaDTO>());
                }
            }

            return lista;
        }
    }
}