using Model.DTO;
using Model.DTO.Compras.PresupuestoCompra;
using Npgsql;
using Persistence;
using Persistence.SQL.Compra;
using System.Data;
using System.Text.Json;
using Utils;

namespace Service.Compra
{
    public class PresupuestoCompraService
    {
        private readonly DBConnector _cn;
        private readonly PresupuestoCompras_sql _query;

        public PresupuestoCompraService(DBConnector cn, PresupuestoCompras_sql query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<PaginadoDTO<PresupuestoCompraListDTO>> PresupuestoCompraList(int page, int pageSize, int codsucursal)
        {
            var lista = new List<PresupuestoCompraListDTO>();
            int totalItems = 0;
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();

                using (var cmdCount = new NpgsqlCommand($"SELECT COUNT(*) FROM purchase.presupuestocompra where codsucursal = {codsucursal}", npgsql))
                {
                    totalItems = Convert.ToInt32(await cmdCount.ExecuteScalarAsync());
                }

                int offset = (page - 1) * pageSize;

                string consultapresc_ = _query.SelectList(pageSize, offset);
                using (var cmdPresupuestoCompra = new NpgsqlCommand(consultapresc_, npgsql))
                {
                    cmdPresupuestoCompra.Parameters.AddWithValue("@codsucursal", codsucursal);
                    using (var readerPresCompra = await cmdPresupuestoCompra.ExecuteReaderAsync())
                    {
                        while (await readerPresCompra.ReadAsync())
                        {
                            var prescompra_ = new PresupuestoCompraListDTO
                            {
                                codpresupuestocompra = (int)readerPresCompra["codpresupuestocompra"],
                                numsucursal = (string)readerPresCompra["numsucursal"],
                                dessucursal = (string)readerPresCompra["dessucursal"],
                                numtipocomprobante = (string)readerPresCompra["numtipocomprobante"],
                                destipocomprobante = (string)readerPresCompra["destipocomprobante"],
                                numpresupuestocompra = (string)readerPresCompra["numpresupuestocompra"],
                                fechapresupuesto = (DateTime)readerPresCompra["fechapresupuesto"],
                                numestmov = (string)readerPresCompra["numestmov"],
                                desestmov = (string)readerPresCompra["desestmov"],
                                empleado = (string)readerPresCompra["empleado"],
                                nrodocprv = (string)readerPresCompra["nrodocprv"],
                                proveedor = (string)readerPresCompra["razonsocial"],
                                pedidocompra = (string)readerPresCompra["datopedido"],
                                totaliva = (decimal)readerPresCompra["totaliva"],
                                totalexento = (decimal)readerPresCompra["totalexento"],
                                totalpresupuestocompra = (decimal)readerPresCompra["totalpresupuestocompra"]
                            };
                            lista.Add(prescompra_);
                        }
                    }
                }
                await npgsql.CloseAsync();
            }
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new PaginadoDTO<PresupuestoCompraListDTO>
            {
                Data = lista,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<PresupuestoCompraDTO?> PresupuestoCompraVer(int codpresupuestocompra)
        {
            PresupuestoCompraDTO? prstCompra = null;

            using var npgsql = new NpgsqlConnection(_cn.cadenaSQL());
            await npgsql.OpenAsync();

            using (var cmd = new NpgsqlCommand(_query.Select(1), npgsql))
            {
                cmd.Parameters.AddWithValue("@codpresupuestocompra", codpresupuestocompra);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    prstCompra = reader.MapToObject<PresupuestoCompraDTO>();
                    prstCompra.detalle = new List<PresupuestoCompraDetDTO>();
                }
            }
            
            if (prstCompra == null) return null;

            // --- Detalles ---
            using (var cmdDet = new NpgsqlCommand(_query.SelectDetails(1), npgsql))
            {
                cmdDet.Parameters.AddWithValue("@codpresupuestocompra", codpresupuestocompra);

                using var readerDet = await cmdDet.ExecuteReaderAsync();
                while (await readerDet.ReadAsync())
                {
                    prstCompra.detalle!.Add(readerDet.MapToObject<PresupuestoCompraDetDTO>());
                }
            }

            return prstCompra;
        }

        public async Task<int> InsertarPresupuestoCompra(PresupuestoCompraInsertDto presupuesto)
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

                            cmd.Parameters.AddWithValue("@codtipocomprobante", presupuesto.codtipocomprobante);
                            cmd.Parameters.AddWithValue("@codterminal", presupuesto.codterminal);
                            cmd.Parameters.AddWithValue("@ultimo", presupuesto.ultimo);
                            cmd.Parameters.AddWithValue("@numpresupuestocompra", presupuesto.numpresupuestocompra);
                            cmd.Parameters.AddWithValue("@fechapresupuesto", DateTime.Parse(presupuesto.fechapresupuesto));
                            cmd.Parameters.AddWithValue("@codestmov", presupuesto.codestmov);
                            cmd.Parameters.AddWithValue("@codempleado", presupuesto.codempleado);
                            cmd.Parameters.AddWithValue("@codproveedor", presupuesto.codproveedor);
                            cmd.Parameters.AddWithValue("@codmoneda", presupuesto.codmoneda);
                            cmd.Parameters.AddWithValue("@codsucursal", presupuesto.codsucursal);
                            cmd.Parameters.AddWithValue("@totaliva", presupuesto.totaliva);
                            cmd.Parameters.AddWithValue("@totaldescuento", presupuesto.totaldescuento);
                            cmd.Parameters.AddWithValue("@totalexento", presupuesto.totalexento);
                            cmd.Parameters.AddWithValue("@totalgravada", presupuesto.totalgravada);
                            cmd.Parameters.AddWithValue("@totalpresupuestocompra", presupuesto.totalpresupuestocompra);
                            cmd.Parameters.AddWithValue("@cotizacion", presupuesto.cotizacion);
                            cmd.Parameters.AddWithValue("@observacion", presupuesto.observacion ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@contactoprv", presupuesto.contactoprv ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@condiciopago", presupuesto.condiciopago);

                            var detallesJson = JsonSerializer.Serialize(presupuesto.prescompradet);
                            cmd.Parameters.AddWithValue("@detalles", NpgsqlTypes.NpgsqlDbType.Json, detallesJson);

                            int codpresupuestocompra = (int)await cmd.ExecuteScalarAsync();

                            await transaction.CommitAsync();
                            return codpresupuestocompra;
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine("Error al insertar PresupuestoCompra: " + ex.Message);
                        throw;
                    }
                }
            }
        }

        public async Task<string> ActualizarEstadoV2(int codpresupuestocompra, int codestado)
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
                            cmd.Parameters.AddWithValue("@codpresupuestocompra", codpresupuestocompra);
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

        public async Task<string> ActualizarEstado(int codpresupuestocompra, int codestado)
        {
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var transaction = await npgsql.BeginTransactionAsync())
                {
                    try
                    {
                        string consultaEstado = _query.Select(3);

                        using (var cmdValidar = new NpgsqlCommand(consultaEstado, npgsql))
                        {
                            cmdValidar.Parameters.AddWithValue("@codpresupuestocompra", codpresupuestocompra);
                            cmdValidar.Transaction = transaction;
                            var estadoActual = await cmdValidar.ExecuteScalarAsync();

                            switch ((int)estadoActual)
                            {
                                case 2:
                                    throw new Exception("El presupuesto compra ya fue utilizado.");
                                case 3:
                                    throw new Exception("El presupuesto compra no se puede utilizar ya supero los dias.");
                                case 4:
                                    throw new Exception("El presupuesto compra ya se encuentra anulado.");
                            }
                        }
                        string actulizarestado = _query.Update(2);

                        using (var cmd = new NpgsqlCommand(actulizarestado, npgsql))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@codpresupuestocompra", codpresupuestocompra);
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

        public async Task<List<PresupuestoCompraOCListDTO>> PresupuestoCompraxPrv(int codproveedor)
        {
            var listaPresupuestos = new List<PresupuestoCompraOCListDTO>();

            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consultaprstcab = _query.Select(2);
                using (var cmdPrstC = new NpgsqlCommand(consultaprstcab, npgsql))
                {
                    cmdPrstC.Parameters.AddWithValue("@codproveedor", codproveedor);
                    using (var reader = await cmdPrstC.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var prstc = new PresupuestoCompraOCListDTO
                            {
                                codpresupuestocompra = reader.GetInt32(reader.GetOrdinal("codpresupuestocompra")),
                                codproveedor = reader.GetInt32(reader.GetOrdinal("codproveedor")),
                                fechapresupuesto = reader.GetDateTime(reader.GetOrdinal("fechapresupuesto")),
                                datopresupuesto = reader.GetString(reader.GetOrdinal("datopresupuesto")),
                                totalpresupuestocompra = reader.GetDecimal(reader.GetOrdinal("totalpresupuestocompra")),
                                presdetoc = new List<PresupuestoCompraOCListDetDTO>()
                            };

                            listaPresupuestos.Add(prstc);
                        }
                    }
                }
                string consultaDetalle_ = _query.SelectDetails(2);
                using (var cmdDetalle = new NpgsqlCommand(consultaDetalle_, npgsql))
                {
                    foreach (var presupuesto in listaPresupuestos)
                    {
                        cmdDetalle.Parameters.Clear();
                        cmdDetalle.Parameters.AddWithValue("@codpresupuestocompra", presupuesto.codpresupuestocompra);
                        
                        using (var readerDet = await cmdDetalle.ExecuteReaderAsync())
                        {
                            while (await readerDet.ReadAsync())
                            {
                                var detalle = new PresupuestoCompraOCListDetDTO
                                {
                                    codpresupuestocompra = readerDet.GetInt32(readerDet.GetOrdinal("codpresupuestocompra")),
                                    codproducto = readerDet.GetInt32(readerDet.GetOrdinal("codproducto")),
                                    producto = readerDet.GetString(readerDet.GetOrdinal("datoproducto")),
                                    codiva = readerDet.GetInt32(readerDet.GetOrdinal("codiva")),
                                    iva = readerDet.GetString(readerDet.GetOrdinal("datoiva")),
                                    cantidad = readerDet.GetDecimal(readerDet.GetOrdinal("cantidad")),
                                    preciobruto = readerDet.GetDecimal(readerDet.GetOrdinal("preciobruto")),
                                    totallinea = readerDet.GetDecimal(readerDet.GetOrdinal("totallinea")),
                                };
                                presupuesto.presdetoc?.Add(detalle);
                            }
                        }
                    }
                }
            }
            return listaPresupuestos;
        }
    }
}
