using Model.DTO;
using Model.DTO.Compras.Ajustes;
using Npgsql;
using Persistence;
using Persistence.SQL.Compra;
using System.Data;
using System.Text.Json;

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

        public async Task<PaginadoDTO<AjustesListDTO>> AjustaList(int page, int pageSize)
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


    }
}
