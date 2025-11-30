using System.Data;
using System.Text.Json;
using Model.DTO;
using Model.DTO.Compras.Compra;
using Model.DTO.Compras.RemisionCompra;
using Npgsql;
using Persistence;
using Persistence.SQL.Compra;
using Utils;

namespace Service.Compra
{
    public class RemisionCompraService
    {
        private readonly DBConnector _cn;
        private readonly RemisionCompra_Sql _query;

        public RemisionCompraService(DBConnector cn, RemisionCompra_Sql query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<PaginadoDTO<RemisionCompraListDTO>> RemisionCompraList( int page, int pageSize, int codsucursal)
        {
            var lista = new List<RemisionCompraListDTO>();
            int totalItems = 0;
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var cmdCount = new NpgsqlCommand($"SELECT COUNT(*) FROM purchase.remisioncompra where codsucursal = {codsucursal}", npgsql))
                {
                    totalItems = Convert.ToInt32(await cmdCount.ExecuteScalarAsync());
                }

                int offset = (page - 1) * pageSize;
                string consultaCompras = _query.SelectList(pageSize, offset);
                using (var cmdCompras = new NpgsqlCommand(consultaCompras, npgsql))
                {
                    cmdCompras.Parameters.AddWithValue("@codsucursal", codsucursal);
                    using (var readerCompras = await cmdCompras.ExecuteReaderAsync())
                    {
                        while (await readerCompras.ReadAsync())
                        {
                            var compras_ = new RemisionCompraListDTO
                            {
                                codremisioncompra = (int)readerCompras["codremisioncompra"],
                                fecharemision = (DateTime)readerCompras["fecharemision"],
                                numremisioncompra = (string)readerCompras["numremisioncompra"],
                                datocompra = (string)readerCompras["datocompra"],
                                datoproveedor = (string)readerCompras["datoproveedor"],
                                datosucursal = (string)readerCompras["datosucursal"],
                                fechallegada = (DateTime)readerCompras["fechallegada"],
                                estado = (string)readerCompras["estado"],
                                totalnotaremision = 0.00m
                            };
                            lista.Add(compras_);
                        }
                    }
                }
                await npgsql.CloseAsync();
            }
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new PaginadoDTO<RemisionCompraListDTO>
            {
                Data = lista,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<int> InsertarRemisionCompra(RemisionCompraInsertDTO remisionCompra)
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

                            cmd.Parameters.AddWithValue("@codcompra", remisionCompra.codcompra);
                            cmd.Parameters.AddWithValue("@codsucursal", remisionCompra.codsucursal);
                            cmd.Parameters.AddWithValue("@codtipocomprobante", remisionCompra.codtipocomprobante);
                            cmd.Parameters.AddWithValue("@codestmov", remisionCompra.codestmov);
                            cmd.Parameters.AddWithValue("@numremisioncompra", remisionCompra.numremisioncompra);
                            cmd.Parameters.AddWithValue("@fecharemision", remisionCompra.fecharemision);
                            cmd.Parameters.AddWithValue("@fecharegistro", remisionCompra.fecharegistro);
                            cmd.Parameters.AddWithValue("@codproveedor", remisionCompra.codproveedor);
                            cmd.Parameters.AddWithValue("@codempleado", remisionCompra.codempleado);
                            cmd.Parameters.AddWithValue("@rucransportista", remisionCompra.ruc_ransportista);
                            cmd.Parameters.AddWithValue("@razonsocialtransportista", remisionCompra.razonsocial_transportista);
                            cmd.Parameters.AddWithValue("@chapavehiculo", remisionCompra.chapa_vehiculo);
                            cmd.Parameters.AddWithValue("@marcavehiculo", remisionCompra.marca_vehiculo);
                            cmd.Parameters.AddWithValue("@modelovehiculo", remisionCompra.modelo_vehiculo);
                            cmd.Parameters.AddWithValue("@nrodocchofer", remisionCompra.nrodoc_chofer);
                            cmd.Parameters.AddWithValue("@nombreapellidochofer", remisionCompra.nombreapellido_chofer);
                            cmd.Parameters.AddWithValue("@nrotelefonochofer", remisionCompra.telefono_chofer);
                            var detallesJson = JsonSerializer.Serialize(remisionCompra.remisioncompradet);
                            cmd.Parameters.AddWithValue("@detalles", NpgsqlTypes.NpgsqlDbType.Json, detallesJson);
                            cmd.Parameters.AddWithValue("@codterminal", remisionCompra.codterminal);
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
        public async Task<RemisionCompraDTO?> RemisionCompraVer(int codremisioncompra)
        {
            RemisionCompraDTO? compras = null;

            using var npgsql = new NpgsqlConnection(_cn.cadenaSQL());
            await npgsql.OpenAsync();

            using (var cmd = new NpgsqlCommand(_query.Select(1), npgsql))
            {
                cmd.Parameters.AddWithValue("@codremisioncompra", codremisioncompra);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    compras = reader.MapToObject<RemisionCompraDTO>();
                    compras.remisiondet = new List<RemisionCompraDetDTO>();
                }
            }

            if (compras == null) return null;

            // --- Detalles ---
            using (var cmdDet = new NpgsqlCommand(_query.SelectWithDetails(), npgsql))
            {
                cmdDet.Parameters.AddWithValue("@codremisioncompra", codremisioncompra);

                using var readerDet = await cmdDet.ExecuteReaderAsync();
                while (await readerDet.ReadAsync())
                {
                    compras.remisiondet!.Add(readerDet.MapToObject<RemisionCompraDetDTO>());
                }
            }

            return compras;
        }
        public async Task<string> CancelarRemision(int codremisioncompra)
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
                            cmd.Parameters.AddWithValue("@codremisioncompra", codremisioncompra);
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