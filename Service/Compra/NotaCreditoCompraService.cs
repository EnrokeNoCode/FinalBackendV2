using Model.DTO;
using Model.DTO.Compras.NotaCreditoCompra;
using Npgsql;
using Persistence;
using Persistence.SQL.Compra;
using System.Data;
using System.Text.Json;
using Utils;

namespace Service.Compra
{
    public class NotaCreditoCompraService
    {
        private readonly DBConnector _cn;
        private readonly NotaCreditoCompra_Sql _query;

        public NotaCreditoCompraService(DBConnector cn, NotaCreditoCompra_Sql query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<PaginadoDTO<NotaCreditoListDTO>> NotaCreditoCompraList(int page, int pageSize)
        {
            var lista = new List<NotaCreditoListDTO>();
            int totalItems = 0;
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var cmdCount = new NpgsqlCommand("SELECT COUNT(*) FROM shared.notacredito where movimiento = 'COMPRAS'", npgsql))
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
                            var compras_ = new NotaCreditoListDTO
                            {
                                codnotacredito = (int)readerCompras["codnotacredito"],
                                codcompra = (int)readerCompras["codcompra"],
                                fechanotacredito = (DateTime)readerCompras["fechanotacredito"],
                                nronotacredito = (string)readerCompras["nronotacredito"],
                                dessucursal = (string)readerCompras["dessucursal"],
                                datocompra = (string)readerCompras["datocompra"],
                                totaldevolucion = (decimal)readerCompras["totaldevolucion"],
                                datoproveedor = (string)readerCompras["datoproveedor"],
                                nummoneda = (string)readerCompras["nummoneda"]
                            };
                            lista.Add(compras_);
                        }
                    }
                }
                await npgsql.CloseAsync();
            }
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new PaginadoDTO<NotaCreditoListDTO>
            {
                Data = lista,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<int> InsertarNotaCreditoCompra(NotaCreditoInsertDTO notaCredito)
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

                            cmd.Parameters.AddWithValue("@codcompra", notaCredito.codcompra);
                            cmd.Parameters.AddWithValue("@codproveedor", notaCredito.codproveedor);
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
                            int codcompra = (int)await cmd.ExecuteScalarAsync();
                            await transaction.CommitAsync();
                            return codcompra;
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

        public async Task<NotaCreditoDTO?> NotaCreditoVer(int codnotacredito)
        {
            NotaCreditoDTO? notaCredito = null;

            using var npgsql = new NpgsqlConnection(_cn.cadenaSQL());
            await npgsql.OpenAsync();

            using (var cmd = new NpgsqlCommand(_query.Select(1), npgsql))
            {
                cmd.Parameters.AddWithValue("@codnotacredito", codnotacredito);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    notaCredito = reader.MapToObject<NotaCreditoDTO>();
                    notaCredito.notacreditodet = new List<NotaCreditoDetDTO>();
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
                    notaCredito.notacreditodet!.Add(readerDet.MapToObject<NotaCreditoDetDTO>());
                }
            }

            return notaCredito;
        }

    }
}
