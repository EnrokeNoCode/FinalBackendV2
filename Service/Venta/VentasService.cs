using Model.DTO;
using Model.DTO.Ventas.Venta;
using Model.DTO.Ventas.Ventas;
using Npgsql;
using Persistence;
using Persistence.SQL.Venta;
using System.Data;
using System.Text.Json;
using Utils;

namespace Service.Venta
{
    public class VentasService
    {
        private readonly DBConnector _cn;
        private readonly Ventas_sql _query;

        public VentasService(DBConnector cn, Ventas_sql query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<PaginadoDTO<VentasListDTO>> VentasList(int page, int pageSize)
        {
            var lista = new List<VentasListDTO>();
            int totalItems = 0;
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var cmdCount = new NpgsqlCommand("SELECT COUNT(*) FROM sales.ventas", npgsql))
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
                            var ventas_ = new VentasListDTO
                            {
                                codventa = (int)readerVentas["codventa"],
                                datosventa = (string)readerVentas["datosventa"],
                                fechaventa = (DateTime)readerVentas["fechaventa"],
                                datocliente = (string)readerVentas["datocliente"],
                                desestmov = (string)readerVentas["desestmov"],
                                datosucursal = (string)readerVentas["datosucursal"],
                                presupuestoventa = (string)readerVentas["datopresupuesto"],
                                datovendedor = (string)readerVentas["datovendedor"],
                                moneda = (string)readerVentas["moneda"],
                                totalventa = (decimal)readerVentas["totalventa"],
                                condicion = (string)readerVentas["condicion"]
                            };
                            lista.Add(ventas_);
                        }
                    }
                }
                await npgsql.CloseAsync();
            }
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new PaginadoDTO<VentasListDTO>
            {
                Data = lista,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<List<VentaListCobroContadoDTO>> VentaContado(int codcliente)
        {
            var lista = new List<VentaListCobroContadoDTO>();

            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consulta = _query.Select(4);

                using (var cmd = new NpgsqlCommand(consulta, npgsql))
                {
                    cmd.Parameters.AddWithValue("@codcliente", codcliente);

                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            lista.Add(new VentaListCobroContadoDTO
                            {
                                codventa = dr.GetInt32(dr.GetOrdinal("codventa")),
                                codmoneda = dr.GetInt32(dr.GetOrdinal("codmoneda")),
                                numventa = dr["numventa"]?.ToString(),
                                totalventa = Convert.ToDecimal(dr["totalventa"]),
                                nummoneda = dr["nummoneda"]?.ToString(),
                                cotizacion = Convert.ToDecimal(dr["cotizacion"])
                            });
                        }
                    }
                }
            }

            return lista;
        }

        public async Task<int> InsertarVentas(VentasInsertDTO ventas)
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

                            cmd.Parameters.AddWithValue("@codtipocomprobante", ventas.codtipocomprobante);
                            cmd.Parameters.AddWithValue("@numventa", ventas.numventa);
                            cmd.Parameters.AddWithValue("@fechaventa", DateTime.Parse(ventas.fechaventa));
                            cmd.Parameters.AddWithValue("@codcliente", ventas.codcliente);
                            cmd.Parameters.AddWithValue("@codterminal", ventas.codterminal);
                            cmd.Parameters.AddWithValue("@ultimo", ventas.ultimo);
                            cmd.Parameters.AddWithValue("@finvalideztimbrado", ventas.finvalideztimbrado);
                            cmd.Parameters.AddWithValue("@nrotimbrado", ventas.nrotimbrado);
                            cmd.Parameters.AddWithValue("@codsucursal", ventas.codsucursal);
                            cmd.Parameters.AddWithValue("@codvendedor", ventas.codvendedor);
                            cmd.Parameters.AddWithValue("@codestmov", ventas.codestmov);
                            cmd.Parameters.AddWithValue("@condicionpago", ventas.condicionpago);
                            cmd.Parameters.AddWithValue("@codmoneda", ventas.codmoneda);
                            cmd.Parameters.AddWithValue("@observacion", ventas.observacion ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@cotizacion", ventas.cotizacion);
                            cmd.Parameters.AddWithValue("@totaliva", ventas.totaliva);
                            cmd.Parameters.AddWithValue("@totaldescuento", ventas.totaldescuento);
                            cmd.Parameters.AddWithValue("@totalexento", ventas.totalexento);
                            cmd.Parameters.AddWithValue("@totalgravada", ventas.totalgravada);
                            cmd.Parameters.AddWithValue("@totalventa", ventas.totalventa);
                            cmd.Parameters.AddWithValue("@codpresupuestoventa", ventas.codpresupuestoventa ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@cant_cuotas", ventas.cant_cuotas);
                            var detallesJson = JsonSerializer.Serialize(ventas.ventasdet);
                            cmd.Parameters.AddWithValue("@detalles", NpgsqlTypes.NpgsqlDbType.Json, detallesJson);
                            int codcompra = (int)await cmd.ExecuteScalarAsync();
                            await transaction.CommitAsync();
                            return codcompra;
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine("Error al insertar la Venta: " + ex.Message);
                        throw;
                    }
                }
            }
        }

        public async Task<List<VentasNCListDTO>> VentasListCabecera(int codcliente)
        {
            var ventasList = new List<VentasNCListDTO>();

            using var npgsql = new NpgsqlConnection(_cn.cadenaSQL());
            await npgsql.OpenAsync();

            using (var cmd = new NpgsqlCommand(_query.Select(3), npgsql))
            {
                cmd.Parameters.AddWithValue("@codcliente", codcliente);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var venta = reader.MapToObject<VentasNCListDTO>();
                    ventasList.Add(venta);
                }
            }

            return ventasList;
        }

        public async Task<List<VentasDetNCListDTO>> VentasListDetalle(int codventa)
        {
            var detalles = new List<VentasDetNCListDTO>();

            using var npgsql = new NpgsqlConnection(_cn.cadenaSQL());
            await npgsql.OpenAsync();

            using (var cmd = new NpgsqlCommand(_query.SelectDetails(2), npgsql))
            {
                cmd.Parameters.AddWithValue("@codventa", codventa);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    detalles.Add(reader.MapToObject<VentasDetNCListDTO>());
                }
            }

            return detalles;
        }

        //--> Para la Remision calcular disponible en base a los detalles
        public async Task<List<VentasREMDetListDTO>> VentasRemListDetalle(int codventa)
        {
            var detalles = new List<VentasREMDetListDTO>();

            using var npgsql = new NpgsqlConnection(_cn.cadenaSQL());
            await npgsql.OpenAsync();

            using (var cmd = new NpgsqlCommand(_query.SelectDetails(3), npgsql))
            {
                cmd.Parameters.AddWithValue("@codventa", codventa);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    detalles.Add(reader.MapToObject<VentasREMDetListDTO>());
                }
            }

            return detalles;
        }

        public async Task<string> ActualizarEstadoV2(int codventa, int codestado)
        {

            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var transaction = await npgsql.BeginTransactionAsync())
                {
                    try
                    {
                        string actulizarestado = _query.Update(1);
                        using (var cmd = new NpgsqlCommand(actulizarestado, npgsql))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@codventa", codventa);
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

        public async Task<VentasDTO?> ComprasVer(int codventa)
        {
            VentasDTO? ventas = null;

            using var npgsql = new NpgsqlConnection(_cn.cadenaSQL());
            await npgsql.OpenAsync();

            using (var cmd = new NpgsqlCommand(_query.Select(2), npgsql))
            {
                cmd.Parameters.AddWithValue("@codventa", codventa);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    ventas = reader.MapToObject<VentasDTO>();
                    ventas.detalle = new List<VentasDetDTO>();
                }
            }

            if (ventas == null) return null;

            // --- Detalles ---
            using (var cmdDet = new NpgsqlCommand(_query.SelectDetails(1), npgsql))
            {
                cmdDet.Parameters.AddWithValue("@codventa", codventa);

                using var readerDet = await cmdDet.ExecuteReaderAsync();
                while (await readerDet.ReadAsync())
                {
                    ventas.detalle!.Add(readerDet.MapToObject<VentasDetDTO>());
                }
            }

            return ventas;
        }
    }
}
