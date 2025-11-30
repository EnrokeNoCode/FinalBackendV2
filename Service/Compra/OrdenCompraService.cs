using Model.DTO;
using Model.DTO.Compras.OrdenCompra;
using Npgsql;
using Persistence;
using Persistence.SQL.Compra;
using System.Data;
using System.Text.Json;
using Utils;

namespace Service.Compra
{
    public class OrdenCompraService
    {
        private readonly DBConnector _cn;
        private readonly OrdenCompra_Sql _query;

        public OrdenCompraService(DBConnector cn, OrdenCompra_Sql query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<PaginadoDTO<OrdenCompraListDTO>> OrdenCompraList(int page, int pageSize, int codsucursal)
        {
            var lista = new List<OrdenCompraListDTO>();
            int totalItems = 0;
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var cmdCount = new NpgsqlCommand($"SELECT COUNT(*) FROM purchase.ordencompra where codsucursal = {codsucursal}", npgsql))
                {
                    totalItems = Convert.ToInt32(await cmdCount.ExecuteScalarAsync());
                }

                int offset = (page - 1) * pageSize;
                string consultaOrdenCompra = _query.SelectList(pageSize, offset);
                using (var cmdOrdenCompra = new NpgsqlCommand(consultaOrdenCompra, npgsql))
                {
                    cmdOrdenCompra.Parameters.AddWithValue("@codsucursal", codsucursal);
                    using (var readerOrdenCompra = await cmdOrdenCompra.ExecuteReaderAsync())
                    {
                        while (await readerOrdenCompra.ReadAsync())
                        {
                            var ordenCompra_ = new OrdenCompraListDTO
                            {
                                codordenc = (int)readerOrdenCompra["codordenc"],
                                numsucursal = (string)readerOrdenCompra["numsucursal"],
                                dessucursal = (string)readerOrdenCompra["dessucursal"],
                                numtipocomprobante = (string)readerOrdenCompra["numtipocomprobante"],
                                destipocomprobante = (string)readerOrdenCompra["destipocomprobante"],
                                numordencompra = (string)readerOrdenCompra["numordencompra"],
                                fechaorden = (DateTime)readerOrdenCompra["fechaorden"],
                                numestmov = (string)readerOrdenCompra["numestmov"],
                                desestmov = (string)readerOrdenCompra["desestmov"],
                                empleado = (string)readerOrdenCompra["empleado"],
                                nrodocprv = (string)readerOrdenCompra["nrodocprv"],
                                proveedor = (string)readerOrdenCompra["razonsocial"],
                                presupuestocompra = (string)readerOrdenCompra["datopresupuesto"],
                                totaliva = (decimal)readerOrdenCompra["totaliva"],
                                totalexento = (decimal)readerOrdenCompra["totalexento"],
                                totalordencompra = (decimal)readerOrdenCompra["totalordencompra"],
                                condicion = (string)readerOrdenCompra["condicion"]
                            };
                            lista.Add(ordenCompra_);
                        }
                    }
                }
                await npgsql.CloseAsync();
            }
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new PaginadoDTO<OrdenCompraListDTO>
            {
                Data = lista,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<OrdenCompraDTO?> OrdenCompraVer(int codordenc)
        {
            OrdenCompraDTO? ordenCompra = null;

            using var npgsql = new NpgsqlConnection(_cn.cadenaSQL());
            await npgsql.OpenAsync();

            using (var cmd = new NpgsqlCommand(_query.Select(2), npgsql))
            {
                cmd.Parameters.AddWithValue("@codordenc", codordenc);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    ordenCompra = reader.MapToObject<OrdenCompraDTO>();
                    ordenCompra.detalle = new List<OrdenCompraDetDTO>();
                }
            }

            if (ordenCompra == null) return null;

            // --- Detalles ---
            using (var cmdDet = new NpgsqlCommand(_query.SelectDetails(1), npgsql))
            {
                cmdDet.Parameters.AddWithValue("@codordenc", codordenc);

                using var readerDet = await cmdDet.ExecuteReaderAsync();
                while (await readerDet.ReadAsync())
                {
                    ordenCompra.detalle!.Add(readerDet.MapToObject<OrdenCompraDetDTO>());
                }
            }

            return ordenCompra;
        }

        public async Task<int> InsertarOrdenCompra(OrdenCompraInsertDTO ordenCompra)
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

                            cmd.Parameters.AddWithValue("@codtipocomprobante", ordenCompra.codtipocomprobante);
                            cmd.Parameters.AddWithValue("@codterminal", ordenCompra.codterminal);
                            cmd.Parameters.AddWithValue("@ultimo", ordenCompra.ultimo);
                            cmd.Parameters.AddWithValue("@numordencompra", ordenCompra.numordencompra);
                            cmd.Parameters.AddWithValue("@fechaorden", DateTime.Parse(ordenCompra.fechaorden));
                            cmd.Parameters.AddWithValue("@codestmov", ordenCompra.codestmov);
                            cmd.Parameters.AddWithValue("@codempleado", ordenCompra.codempleado);
                            cmd.Parameters.AddWithValue("@codproveedor", ordenCompra.codproveedor);
                            cmd.Parameters.AddWithValue("@codmoneda", ordenCompra.codmoneda);
                            cmd.Parameters.AddWithValue("@codsucursal", ordenCompra.codsucursal);
                            cmd.Parameters.AddWithValue("@totaliva", ordenCompra.totaliva);
                            cmd.Parameters.AddWithValue("@totaldescuento", ordenCompra.totaldescuento);
                            cmd.Parameters.AddWithValue("@totalexento", ordenCompra.totalexento);
                            cmd.Parameters.AddWithValue("@totalgravada", ordenCompra.totalgravada);
                            cmd.Parameters.AddWithValue("@totalordencompra", ordenCompra.totalordencompra);
                            cmd.Parameters.AddWithValue("@cotizacion", ordenCompra.cotizacion);
                            cmd.Parameters.AddWithValue("@observacion", ordenCompra.observacion ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@condiciopago", ordenCompra.condiciopago);
                            cmd.Parameters.AddWithValue("@codpresupuestocompra", ordenCompra.codpresupuestocompra ?? (object)DBNull.Value);

                            var detallesJson = JsonSerializer.Serialize(ordenCompra.ordcompradet);
                            cmd.Parameters.AddWithValue("@detalles", NpgsqlTypes.NpgsqlDbType.Json, detallesJson);

                            int codordencompra = (int)await cmd.ExecuteScalarAsync();

                            await transaction.CommitAsync();
                            return codordencompra;
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine("Error al insertar OrdenCompra: " + ex.Message);
                        throw;
                    }
                }
            }
        }

        public async Task<string> ActualizarEstadoV2(int codordenc, int codestado)
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
                            cmd.Parameters.AddWithValue("@codordenc", codordenc);
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

        public async Task<string> ActualizarEstado(int codordenc, int codestado)
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
                            cmdValidar.Parameters.AddWithValue("@codordenc", codordenc);
                            cmdValidar.Transaction = transaction;
                            var estadoActual = await cmdValidar.ExecuteScalarAsync();

                            switch ((int)estadoActual)
                            {
                                case 2:
                                    throw new Exception("La orden de compra ya fue utilizada.");                                 
                                case 3:
                                    throw new Exception("La orden de compra no se puede utilizar, ya superó los días.");
                                case 4:
                                    throw new Exception("La orden de compra ya se encuentra anulada.");
                            }
                        }
                        string actulizarestado = _query.Update(2);

                        using (var cmd = new NpgsqlCommand(actulizarestado, npgsql))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@codordenc", codordenc);
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

        public async Task<List<OrdenCompraComListDTO>> OrdenCompraxPrv(int codproveedor)
        {
            var listaOrdenCompra = new List<OrdenCompraComListDTO>();

            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consultaordencab = _query.Select(3);
                using (var cmdOrdenC = new NpgsqlCommand(consultaordencab, npgsql))
                {
                    cmdOrdenC.Parameters.AddWithValue("@codproveedor", codproveedor);
                    using (var reader = await cmdOrdenC.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var prstc = new OrdenCompraComListDTO
                            {
                                codordenc = reader.GetInt32(reader.GetOrdinal("codordenc")),
                                codproveedor = reader.GetInt32(reader.GetOrdinal("codproveedor")),
                                fechaorden = reader.GetDateTime(reader.GetOrdinal("fechaorden")),
                                datoordencompra = reader.GetString(reader.GetOrdinal("datoordencompra")),
                                totalordencompra = reader.GetDecimal(reader.GetOrdinal("totalordencompra")),
                                ordedetcom = new List<OrdenCompraComListDetDTO>()
                            };

                            listaOrdenCompra.Add(prstc);
                        }
                    }
                }
                string consultaDetalle_ = _query.SelectDetails(2);
                using (var cmdDetalle = new NpgsqlCommand(consultaDetalle_, npgsql))
                {
                    foreach (var ordencompra in listaOrdenCompra)
                    {
                        cmdDetalle.Parameters.Clear();
                        cmdDetalle.Parameters.AddWithValue("@codordenc", ordencompra.codordenc);

                        using (var readerDet = await cmdDetalle.ExecuteReaderAsync())
                        {
                            while (await readerDet.ReadAsync())
                            {
                                var detalle = new OrdenCompraComListDetDTO
                                {
                                    codordenc = readerDet.GetInt32(readerDet.GetOrdinal("codordenc")),
                                    codproducto = readerDet.GetInt32(readerDet.GetOrdinal("codproducto")),
                                    producto = readerDet.GetString(readerDet.GetOrdinal("datoproducto")),
                                    codiva = readerDet.GetInt32(readerDet.GetOrdinal("codiva")),
                                    iva = readerDet.GetString(readerDet.GetOrdinal("datoiva")),
                                    cantidad = readerDet.GetDecimal(readerDet.GetOrdinal("cantidad")),
                                    preciobruto = readerDet.GetDecimal(readerDet.GetOrdinal("preciobruto")),
                                    totallinea = readerDet.GetDecimal(readerDet.GetOrdinal("totallinea")),
                                };
                                ordencompra.ordedetcom?.Add(detalle);
                            }
                        }
                    }
                }
            }
            return listaOrdenCompra;
        }

    }
}
