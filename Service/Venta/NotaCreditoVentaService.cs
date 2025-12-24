using Model.DTO;
using Model.DTO.Ventas.NotaCreditoVenta;
using Npgsql;
using Persistence;
using Persistence.SQL.Venta;
using System.Data;
using System.Text.Json;
using Utils;

namespace Service.Venta
{
    public class NotaCreditoVentaService
    {
        private readonly DBConnector _cn;
        private readonly NotaCreditoVenta_Sql _query;

        public NotaCreditoVentaService(DBConnector cn, NotaCreditoVenta_Sql query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<PaginadoDTO<NotaCreditoVentaListDTO>> NotaCreditoVentaList(int page, int pageSize)
        {
            var lista = new List<NotaCreditoVentaListDTO>();
            int totalItems = 0;
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var cmdCount = new NpgsqlCommand("SELECT COUNT(*) FROM shared.notacredito where movimiento = 'VENTAS'", npgsql))
                {
                    totalItems = Convert.ToInt32(await cmdCount.ExecuteScalarAsync());
                }

                int offset = (page - 1) * pageSize;
                string consultaVentas = _query.SelectList(pageSize, offset);
                using (var cmdVentas = new NpgsqlCommand(consultaVentas, npgsql))
                {
                    using (var readerVentas = await cmdVentas.ExecuteReaderAsync())
                    {
                        while (await readerVentas.ReadAsync())
                        {
                            var ventas_ = new NotaCreditoVentaListDTO
                            {
                                codnotacredito = (int)readerVentas["codnotacredito"],
                                codventa = (int)readerVentas["codventa"],
                                fechanotacredito = (DateTime)readerVentas["fechanotacredito"],
                                nronotacredito = (string)readerVentas["nronotacredito"],
                                dessucursal = (string)readerVentas["dessucursal"],
                                datoventa = (string)readerVentas["datoventa"],
                                totaldevolucion = (decimal)readerVentas["totaldevolucion"],
                                datocliente = (string)readerVentas["datocliente"],
                                nummoneda = (string)readerVentas["nummoneda"],
                                desestmov = (string)readerVentas["desestmov"]
                            };
                            lista.Add(ventas_);
                        }
                    }
                }
                await npgsql.CloseAsync();
            }
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new PaginadoDTO<NotaCreditoVentaListDTO>
            {
                Data = lista,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<int> InsertarNotaCreditoVentas(NotaCreditoVentaInsertDTO notaCredito)
        {
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();

                using (var transaction = await npgsql.BeginTransactionAsync())
                {
                    try
                    {
                        string insertarQuery = _query.Insert();

                        using (var cmd = new NpgsqlCommand(insertarQuery, npgsql))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Transaction = transaction;

                            cmd.Parameters.AddWithValue("@codventa", notaCredito.codventa);
                            cmd.Parameters.AddWithValue("@codcliente", notaCredito.codcliente);
                            cmd.Parameters.AddWithValue("@codtipocomprobante", notaCredito.codtipocomprobante);
                            cmd.Parameters.AddWithValue("@numnotacredito", notaCredito.numnotacredito);
                            cmd.Parameters.AddWithValue("@fechanotacredito", DateTime.Parse(notaCredito.fechanotacredito));
                            cmd.Parameters.AddWithValue("@nrotimbrado", notaCredito.nrotimbrado);
                            cmd.Parameters.AddWithValue("@fechavalidez", DateOnly.Parse(notaCredito.fechavalidez));
                            cmd.Parameters.AddWithValue("@codestmov", notaCredito.codestmov);
                            cmd.Parameters.AddWithValue("@codempleado", notaCredito.codempleado);
                            cmd.Parameters.AddWithValue("@codsucursal", notaCredito.codsucursal);
                            cmd.Parameters.AddWithValue("@codmoneda", notaCredito.codmoneda);
                            cmd.Parameters.AddWithValue("@cotizacion", notaCredito.cotizacion);
                            cmd.Parameters.AddWithValue("@totaliva", notaCredito.totaliva);
                            cmd.Parameters.AddWithValue("@totaldescuento", notaCredito.totaldescuento);
                            cmd.Parameters.AddWithValue("@totalexento", notaCredito.totalexenta);
                            cmd.Parameters.AddWithValue("@totalgravada", notaCredito.totalgravada);
                            cmd.Parameters.AddWithValue("@totaldevolucion", notaCredito.totaldevolucion);
                            cmd.Parameters.AddWithValue("@codterminal", notaCredito.codterminal);
                            cmd.Parameters.AddWithValue("@ultimo", notaCredito.ultimo);
                            var detallesJson = JsonSerializer.Serialize(notaCredito.notacreditodet);
                            cmd.Parameters.AddWithValue("@detalles", NpgsqlTypes.NpgsqlDbType.Json, detallesJson);
                            int codnotacredito = (int)await cmd.ExecuteScalarAsync();
                            await transaction.CommitAsync();
                            return codnotacredito;
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine("Error al insertar la Nota Credito: " + ex.Message);
                        throw;
                    }
                }
            }
        }

        public async Task<NotaCreditoVentaDTO?> NotaCreditoVer(int codnotacredito)
        {
            NotaCreditoVentaDTO? notaCredito = null;

            using var npgsql = new NpgsqlConnection(_cn.cadenaSQL());
            await npgsql.OpenAsync();

            using (var cmd = new NpgsqlCommand(_query.Select(1), npgsql))
            {
                cmd.Parameters.AddWithValue("@codnotacredito", codnotacredito);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    notaCredito = reader.MapToObject<NotaCreditoVentaDTO>();
                    notaCredito.notacreditodet = new List<NotaCreditoVentaDetDTO>();
                }
            }

            if (notaCredito == null) return null;

            // --- Detalles ---
            using (var cmdDet = new NpgsqlCommand(_query.SelectDetail(1), npgsql))
            {
                cmdDet.Parameters.AddWithValue("@codnotacredito", codnotacredito);

                using var readerDet = await cmdDet.ExecuteReaderAsync();
                while (await readerDet.ReadAsync())
                {
                    notaCredito.notacreditodet!.Add(readerDet.MapToObject<NotaCreditoVentaDetDTO>());
                }
            }

            return notaCredito;
        }

        public async Task<string> ActualizarEstadoV2(int codnotacredito, int codestado)
        {

            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var transaction = await npgsql.BeginTransactionAsync())
                {
                    try
                    {
                        string actulizarestado = _query.Update();
                        using (var cmd = new NpgsqlCommand(actulizarestado, npgsql))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@codnotacredito", codnotacredito);
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

    }
}
