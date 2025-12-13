using System.Data;
using System.Text.Json;
using Model.DTO;
using Model.DTO.Ventas.Venta;
using Model.DTO.Ventas.RemisionVenta;
using Npgsql;
using Persistence;
using Persistence.SQL.Venta;
using Utils;

namespace Service.Venta
{
    public class RemisionVentaService
    {
        private readonly DBConnector _cn;
        private readonly RemisionVenta_Sql _query;

        public RemisionVentaService(DBConnector cn, RemisionVenta_Sql query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<PaginadoDTO<RemisionVentaListDTO>> RemisionVentaList( int page, int pageSize)
        {
            var lista = new List<RemisionVentaListDTO>();
            int totalItems = 0;
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var cmdCount = new NpgsqlCommand($"SELECT COUNT(*) FROM sales.remisionventa ;", npgsql))
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
                            var compras_ = new RemisionVentaListDTO
                            {
                                codremisionventa = (int)readerVentas["codremisionventa"],
                                fecharemision = (DateTime)readerVentas["fecharemision"],
                                numremisionventa = (string)readerVentas["numremisionventa"],
                                datoventa = (string)readerVentas["datoventa"],
                                datocliente = (string)readerVentas["datocliente"],
                                datosucursal = (string)readerVentas["datosucursal"],
                                fechallegada = (DateTime)readerVentas["fechallegada"],
                                estado = (string)readerVentas["estado"],
                                totalnotaremision = 0.00m
                            };
                            lista.Add(compras_);
                        }
                    }
                }
                await npgsql.CloseAsync();
            }
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new PaginadoDTO<RemisionVentaListDTO>
            {
                Data = lista,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<int> InsertarRemisionVenta(RemisionVentaInsertDTO remisionVenta)
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

                            cmd.Parameters.AddWithValue("@codventa", remisionVenta.codventa);
                            cmd.Parameters.AddWithValue("@codsucursal", remisionVenta.codsucursal);
                            cmd.Parameters.AddWithValue("@codtipocomprobante", remisionVenta.codtipocomprobante);
                            cmd.Parameters.AddWithValue("@codestmov", remisionVenta.codestmov);
                            cmd.Parameters.AddWithValue("@numremisionventa", remisionVenta.numremisionventa);
                            cmd.Parameters.AddWithValue("@fecharemision", remisionVenta.fecharemision);
                            cmd.Parameters.AddWithValue("@fecharegistro", remisionVenta.fecharegistro);
                            cmd.Parameters.AddWithValue("@codcliente", remisionVenta.codcliente);
                            cmd.Parameters.AddWithValue("@codempleado", remisionVenta.codempleado);
                            cmd.Parameters.AddWithValue("@rucransportista", remisionVenta.ruc_ransportista);
                            cmd.Parameters.AddWithValue("@razonsocialtransportista", remisionVenta.razonsocial_transportista);
                            cmd.Parameters.AddWithValue("@chapavehiculo", remisionVenta.chapa_vehiculo);
                            cmd.Parameters.AddWithValue("@marcavehiculo", remisionVenta.marca_vehiculo);
                            cmd.Parameters.AddWithValue("@modelovehiculo", remisionVenta.modelo_vehiculo);
                            cmd.Parameters.AddWithValue("@nrodocchofer", remisionVenta.nrodoc_chofer);
                            cmd.Parameters.AddWithValue("@nombreapellidochofer", remisionVenta.nombreapellido_chofer);
                            cmd.Parameters.AddWithValue("@nrotelefonochofer", remisionVenta.telefono_chofer);
                            var detallesJson = JsonSerializer.Serialize(remisionVenta.remisionventadet);
                            cmd.Parameters.AddWithValue("@detalles", NpgsqlTypes.NpgsqlDbType.Json, detallesJson);
                            cmd.Parameters.AddWithValue("@codterminal", remisionVenta.codterminal);
                            int codcompra = (int)await cmd.ExecuteScalarAsync();
                            await transaction.CommitAsync();
                            return codcompra;
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine("Error al insertar la Remision Compra: " + ex.Message);
                        throw;
                    }
                }
            }
        }
        public async Task<RemisionVentaDTO?> RemisionVentaVer(int codremisionventa)
        {
            RemisionVentaDTO? compras = null;

            using var npgsql = new NpgsqlConnection(_cn.cadenaSQL());
            await npgsql.OpenAsync();

            using (var cmd = new NpgsqlCommand(_query.Select(1), npgsql))
            {
                cmd.Parameters.AddWithValue("@codremisionventa", codremisionventa);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    compras = reader.MapToObject<RemisionVentaDTO>();
                    compras.remisiondet = new List<RemisionVentaDetDTO>();
                }
            }

            if (compras == null) return null;

            // --- Detalles ---
            using (var cmdDet = new NpgsqlCommand(_query.SelectWithDetails(), npgsql))
            {
                cmdDet.Parameters.AddWithValue("@codremisioncompra", codremisionventa);

                using var readerDet = await cmdDet.ExecuteReaderAsync();
                while (await readerDet.ReadAsync())
                {
                    compras.remisiondet!.Add(readerDet.MapToObject<RemisionVentaDetDTO>());
                }
            }

            return compras;
        }
        public async Task<string> CancelarRemision(int codremisionventa)
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
                            cmd.Parameters.AddWithValue("@codremisionventa", codremisionventa);
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