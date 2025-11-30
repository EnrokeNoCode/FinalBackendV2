using Model.DTO;
using Model.DTO.Compras.Ajustes;
using Model.DTO.Compras.Compra;
using Model.DTO.Compras.PedidoCompra;
using Npgsql;
using Persistence;
using Persistence.SQL.Compra;
using System.Data;
using System.Text.Json;
using Utils;

namespace Service.Compra
{
    public class AjustesService
    {
        private readonly DBConnector _cn;
        private readonly Ajustes_Sql _query;

        public AjustesService(DBConnector cn, Ajustes_Sql query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<PaginadoDTO<AjustesListDTO>> AjusteList(int page, int pageSize, int codsucursal)
        {
            var lista = new List<AjustesListDTO>();
            int totalItems = 0;
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var cmdCount = new NpgsqlCommand("SELECT COUNT(*) FROM shared.ajustes ", npgsql))
                {
                    totalItems = Convert.ToInt32(await cmdCount.ExecuteScalarAsync());
                }

                int offset = (page - 1) * pageSize;
                string consultaAjustes = _query.SelectList(pageSize, offset);
                using (var cmdAjustes = new NpgsqlCommand(consultaAjustes, npgsql))
                {
                    cmdAjustes.Parameters.AddWithValue("@codsucursal", codsucursal);
                    using (var readerAjustes = await cmdAjustes.ExecuteReaderAsync())
                    {
                        while (await readerAjustes.ReadAsync())
                        {
                            var compras_ = new AjustesListDTO
                            {
                                codajuste = (int)readerAjustes["codajuste"],
                                datoajuste = (string)readerAjustes["datoajuste"],
                                fechaajuste = (DateTime)readerAjustes["fechaajuste"],
                                dessucursal = (string)readerAjustes["dessucursal"],
                                datomotivo = (string)readerAjustes["datomotivo"],
                                datoempleado = (string)readerAjustes["datoempleado"],
                                estado = (string)readerAjustes["estado"]
                            };
                            lista.Add(compras_);
                        }
                    }
                }
                await npgsql.CloseAsync();
            }
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new PaginadoDTO<AjustesListDTO>
            {
                Data = lista,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<int> InsertarAjustes(AjustesInsertDTO ajuste)
        {
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();

                using (var transaction = await npgsql.BeginTransactionAsync())
                {
                    try
                    {
                        string insertarAjuste = _query.Insert();

                        using (var cmd = new NpgsqlCommand(insertarAjuste, npgsql))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Transaction = transaction;

                            cmd.Parameters.AddWithValue("@codtipocomprobante", ajuste.codtipocomprobante);
                            cmd.Parameters.AddWithValue("@codsucursal", ajuste.codsucursal);
                            cmd.Parameters.AddWithValue("@numajuste", ajuste.numajuste);
                            cmd.Parameters.AddWithValue("@fechaajuste", DateTime.Parse(ajuste.fechaajuste));          
                            cmd.Parameters.AddWithValue("@codmotivo", ajuste.codmotivo);
                            cmd.Parameters.AddWithValue("@codempleado", ajuste.codempleado);
                            cmd.Parameters.AddWithValue("@condicion", ajuste.condicion);
                            cmd.Parameters.AddWithValue("@codterminal", ajuste.codterminal);
                            cmd.Parameters.AddWithValue("@ultimo", ajuste.ultimo);

                            var detallesJson = JsonSerializer.Serialize(ajuste.ajustedet);
                            cmd.Parameters.AddWithValue("@detalles", NpgsqlTypes.NpgsqlDbType.Json, detallesJson);

                            int codpedcompra = (int)await cmd.ExecuteScalarAsync();

                            await transaction.CommitAsync();
                            return codpedcompra;
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine("Error al insertar el Ajuste: " + ex.Message);
                        throw;
                    }
                }
            }
        }
        public async Task<AjustesDTO?> AjusteVer(int codajuste)
        {
            AjustesDTO? ajuste = null;

            using var npgsql = new NpgsqlConnection(_cn.cadenaSQL());
            await npgsql.OpenAsync();

            using (var cmd = new NpgsqlCommand(_query.Select(), npgsql))
            {
                cmd.Parameters.AddWithValue("@codajuste", codajuste);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    ajuste = reader.MapToObject<AjustesDTO>();
                    ajuste.ajustesdet = new List<AjustesDetDTO>();
                }
            }

            if (ajuste == null) return null;

            // --- Detalles ---
            using (var cmdDet = new NpgsqlCommand(_query.SelectWithDetail(), npgsql))
            {
                cmdDet.Parameters.AddWithValue("@codajuste", codajuste);

                using var readerDet = await cmdDet.ExecuteReaderAsync();
                while (await readerDet.ReadAsync())
                {
                    ajuste.ajustesdet!.Add(readerDet.MapToObject<AjustesDetDTO>());
                }
            }

            return ajuste;
        }

        public async Task<string> ActualizarAjusteDet(AjusteUpdateDTO ajuste)
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

                            cmd.Parameters.AddWithValue("@codajuste", ajuste.codajuste);
                            cmd.Parameters.AddWithValue("@codsucursal", ajuste.codsucursal);

                            var detallesJson = JsonSerializer.Serialize(ajuste.ajustesdet);
                            cmd.Parameters.AddWithValue("@detalles", NpgsqlTypes.NpgsqlDbType.Json, detallesJson);

                            string updateMsg = (string)await cmd.ExecuteScalarAsync();
                            await transaction.CommitAsync();
                            return updateMsg;
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine("Error al actualizar: " + ex.Message);
                        throw;
                    }
                }

            }
        }

        public async Task<string> CerrarAjuste(int codajuste)
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
                            cmd.Parameters.AddWithValue("@codajuste", codajuste);
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
