using System.Data;
using System.Text.Json;
using Model.DTO;
using Model.DTO.Compras.Compra;
using Npgsql;
using Persistence;
using Persistence.SQL.Compra;
using Utils;

namespace Service.Compra
{
    public class ComprasService
    {
        private readonly DBConnector _cn;
        private readonly Compras_Sql _query;

        public ComprasService(DBConnector cn, Compras_Sql query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<PaginadoDTO<CompraListDTO>> CompraList(int page, int pageSize)
        {
            var lista = new List<CompraListDTO>();
            int totalItems = 0;
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var cmdCount = new NpgsqlCommand("SELECT COUNT(*) FROM purchase.compras", npgsql))
                {
                    totalItems = Convert.ToInt32(await cmdCount.ExecuteScalarAsync());
                }

                int offset = (page - 1) * pageSize;
                string consultaCompras = _query.SelectList(pageSize, offset);
                using (var cmdCompras = new NpgsqlCommand(consultaCompras, npgsql))
                {
                    using (var readerCompras = await cmdCompras.ExecuteReaderAsync())
                    {
                        while (await readerCompras.ReadAsync())
                        {
                            var compras_ = new CompraListDTO
                            {
                                codcompra = (int)readerCompras["codcompra"],
                                numsucursal = (string)readerCompras["numsucursal"],
                                dessucursal = (string)readerCompras["dessucursal"],
                                numtipocomprobante = (string)readerCompras["numtipocomprobante"],
                                destipocomprobante = (string)readerCompras["destipocomprobante"],
                                numcompra = (string)readerCompras["numcompra"],
                                fechacompra = (DateTime)readerCompras["fechacompra"],
                                numestmov = (string)readerCompras["numestmov"],
                                desestmov = (string)readerCompras["desestmov"],
                                empleado = (string)readerCompras["empleado"],
                                nrodocprv = (string)readerCompras["nrodocprv"],
                                proveedor = (string)readerCompras["razonsocial"],
                                ordencompra = (string)readerCompras["datoordencompra"],
                                totaliva = (decimal)readerCompras["totaliva"],
                                totalexento = (decimal)readerCompras["totalexento"],
                                totalcompra = (decimal)readerCompras["totalcompra"],
                                condicion = (string)readerCompras["condicion"]
                            };
                            lista.Add(compras_);
                        }
                    }
                }
                await npgsql.CloseAsync();
            }
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new PaginadoDTO<CompraListDTO>
            {
                Data = lista,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<ComprasDTO?> ComprasVer(int codcompra)
        {
            ComprasDTO? compras = null;

            using var npgsql = new NpgsqlConnection(_cn.cadenaSQL());
            await npgsql.OpenAsync();

            using (var cmd = new NpgsqlCommand(_query.Select(2), npgsql))
            {
                cmd.Parameters.AddWithValue("@codcompra", codcompra);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    compras = reader.MapToObject<ComprasDTO>();
                    compras.detalle = new List<ComprasDetDTO>();
                }
            }

            if (compras == null) return null;

            // --- Detalles ---
            using (var cmdDet = new NpgsqlCommand(_query.SelectDetails(1), npgsql))
            {
                cmdDet.Parameters.AddWithValue("@codcompra", codcompra);

                using var readerDet = await cmdDet.ExecuteReaderAsync();
                while (await readerDet.ReadAsync())
                {
                    compras.detalle!.Add(readerDet.MapToObject<ComprasDetDTO>());
                }
            }

            return compras;
        }

        public async Task<List<ComprasNCListDTO>> ComprasListCabecera(int codproveedor)
        {
            var comprasList = new List<ComprasNCListDTO>();

            using var npgsql = new NpgsqlConnection(_cn.cadenaSQL());
            await npgsql.OpenAsync();

            using (var cmd = new NpgsqlCommand(_query.Select(3), npgsql))
            {
                cmd.Parameters.AddWithValue("@codproveedor", codproveedor);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var compra = reader.MapToObject<ComprasNCListDTO>();
                    comprasList.Add(compra);
                }
            }

            return comprasList;
        }

        public async Task<List<ComprasDetNCListDTO>> ComprasListDetalle(int codcompra)
        {
            var detalles = new List<ComprasDetNCListDTO>();

            using var npgsql = new NpgsqlConnection(_cn.cadenaSQL());
            await npgsql.OpenAsync();

            using (var cmd = new NpgsqlCommand(_query.SelectDetails(2), npgsql))
            {
                cmd.Parameters.AddWithValue("@codcompra", codcompra);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    detalles.Add(reader.MapToObject<ComprasDetNCListDTO>());
                }
            }

            return detalles;
        }

        public async Task<int> InsertarCompra(ComprasInsertDTO compra)
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

                            cmd.Parameters.AddWithValue("@codtipocomprobante", compra.codtipocomprobante);
                            cmd.Parameters.AddWithValue("@numcompra", compra.numcompra);
                            cmd.Parameters.AddWithValue("@fechacompra", DateTime.Parse(compra.fechacompra));
                            cmd.Parameters.AddWithValue("@codproveedor", compra.codproveedor);
                            cmd.Parameters.AddWithValue("@codterminal", compra.codterminal);
                            cmd.Parameters.AddWithValue("@ultimo", compra.ultimo);
                            cmd.Parameters.AddWithValue("@finvalideztimbrado", compra.finvalideztimbrado);
                            cmd.Parameters.AddWithValue("@nrotimbrado", compra.nrotimbrado);
                            cmd.Parameters.AddWithValue("@codsucursal", compra.codsucursal);
                            cmd.Parameters.AddWithValue("@codempleado", compra.codempleado);
                            cmd.Parameters.AddWithValue("@codestmov", compra.codestmov);
                            cmd.Parameters.AddWithValue("@condicionpago", compra.condicionpago);
                            cmd.Parameters.AddWithValue("@codmoneda", compra.codmoneda);
                            cmd.Parameters.AddWithValue("@observacion", compra.observacion ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@cotizacion", compra.cotizacion);
                            cmd.Parameters.AddWithValue("@totaliva", compra.totaliva);
                            cmd.Parameters.AddWithValue("@totaldescuento", compra.totaldescuento);
                            cmd.Parameters.AddWithValue("@totalexento", compra.totalexento);
                            cmd.Parameters.AddWithValue("@totalgravada", compra.totalgravada);
                            cmd.Parameters.AddWithValue("@totalcompra", compra.totalcompra);
                            cmd.Parameters.AddWithValue("@codordenc", compra.codordenc ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@cant_cuotas", compra.cant_cuotas);
                            var detallesJson = JsonSerializer.Serialize(compra.comprasdet);
                            cmd.Parameters.AddWithValue("@detalles", NpgsqlTypes.NpgsqlDbType.Json, detallesJson);
                            int codcompra = (int)await cmd.ExecuteScalarAsync();
                            await transaction.CommitAsync();
                            return codcompra;
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine("Error al insertar la Compra: " + ex.Message);
                        throw;
                    }
                }
            }
        }

        public async Task<string> ActualizarEstadoV2(int codcompra, int codestado)
        {

            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var transaction = await npgsql.BeginTransactionAsync())
                {
                    try
                    {
                        string actulizarestado = _query.Update(2);
                        using (var cmd = new NpgsqlCommand(actulizarestado, npgsql))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@codcompra", codcompra);
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

        public async Task<string> ActualizarEstado(int codcompra, int codestado)
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
                            cmdValidar.Parameters.AddWithValue("@codcompra", codcompra);
                            cmdValidar.Transaction = transaction;
                            var estadoActual = await cmdValidar.ExecuteScalarAsync();

                            switch ((int)estadoActual)
                            {
                                case 2:
                                    throw new Exception("La compra ya fue pagada.");                                 
                                case 3:
                                    throw new Exception("La compra no se puede utilizar, ya superó los días.");
                                case 4:
                                    throw new Exception("La compra ya se encuentra anulada.");
                            }
                        }
                        string actulizarestado = _query.Update(2);

                        using (var cmd = new NpgsqlCommand(actulizarestado, npgsql))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@codcompra", codcompra);
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