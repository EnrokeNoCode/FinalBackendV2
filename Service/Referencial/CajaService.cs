using Model.DTO;
using Npgsql;
using Persistence;
using Persistence.SQL.Referencial;
using System.Data;
using System.Text.Json;

namespace Service.Referencial
{
    public class CajaService
    {
        private readonly DBConnector _cn;
        private readonly CajaSQL _query;

        public CajaService(DBConnector cn, CajaSQL query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<PaginadoDTO<CajaGestionListDTO>> GetCajaGestionSuc(int codsucursal, int page, int pageSize)
        {
            var lista = new List<CajaGestionListDTO>();
            int totalItems = 0;
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var cmdCount = new NpgsqlCommand($"SELECT COUNT(*) FROM referential.caja WHERE codsucursal = @codsucursal;", npgsql))
                {
                    try
                    {
                        totalItems = Convert.ToInt32(await cmdCount.ExecuteScalarAsync());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                int offset = (page - 1) * pageSize;
                string consulta = _query.SelectGestion(pageSize, offset);
                using (var cmd = new NpgsqlCommand(consulta, npgsql))
                {
                    cmd.Parameters.AddWithValue("codsucursal", codsucursal);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var cajaGestion = new CajaGestionListDTO
                            {
                                codcobrador = (int)reader["codcobrador"],
                                codcaja = (int)reader["codcaja"],
                                datoscaja = (string)reader["numcaja"] + " - " + (string)reader["descaja"],
                                estado = (string)reader["estado"],
                                codgestion = (int)reader["codgestion"],
                                cobrador = (string)reader["cobrador"]
                            };
                            lista.Add(cajaGestion);
                        }
                    }
                }
                await npgsql.CloseAsync();
            }
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new PaginadoDTO<CajaGestionListDTO>
            {
                Data = lista,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<CajaGestionCobroDTO> GetGestionCobro(int codgestion)
        {
            CajaGestionCobroDTO gestionCobro = null;
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consulta = _query.SelectGestionCobranza();
                using (var cmd = new NpgsqlCommand(consulta, npgsql))
                {
                    cmd.Parameters.AddWithValue("@codgestion", codgestion);
                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        if (await dr.ReadAsync())
                        {
                            gestionCobro = new CajaGestionCobroDTO
                            {
                                codgestion = dr.GetInt32(dr.GetOrdinal("codgestion")),
                                codcaja = dr.GetInt32(dr.GetOrdinal("codcaja")),
                                numcaja = dr["numcaja"]?.ToString(),
                                descaja = dr["descaja"]?.ToString(),
                                cobrador = dr["cobrador"]?.ToString()
                            };
                        }
                    }
                }

            }
            return gestionCobro;
        }

        public async Task<object> InsertAperturaCaja(CajaGestionAperturaDTO request)
        {
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var transaction = await npgsql.BeginTransactionAsync())
                {
                    try
                    {
                        string sql = _query.InsertApertura();

                        using (var cmd = new NpgsqlCommand(sql, npgsql, transaction))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@codcaja", request.codcaja);
                            cmd.Parameters.AddWithValue("@codcobrador", request.codcobrador);
                            cmd.Parameters.AddWithValue("@fechaapertura", request.fechaapertura);
                            cmd.Parameters.AddWithValue("@montoapertura", request.montoapertura);
                            cmd.Parameters.AddWithValue("@codterminal", request.codterminal);
                            var result = await cmd.ExecuteScalarAsync();
                            await transaction.CommitAsync();
                            return JsonSerializer.Deserialize<object>(result.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception("Error al insertar apertura de caja: " + ex.Message, ex);
                    }
                }
            }
        }

        public async Task<object> UpdateCierreCaja(int codgestion, CajaGestionCierreDTO request)
        {
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var transaction = await npgsql.BeginTransactionAsync())
                {
                    try
                    {
                        string sql = _query.UpdateCierre();

                        using (var cmd = new NpgsqlCommand(sql, npgsql, transaction))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@codgestion", codgestion);
                            cmd.Parameters.AddWithValue("@fechacierre", request.fechacierre);
                            cmd.Parameters.AddWithValue("@montocierre", request.montocierre);
                            var result = await cmd.ExecuteScalarAsync();
                            await transaction.CommitAsync();
                            return JsonSerializer.Deserialize<object>(result.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception("Error al actualizar el cierre de caja: " + ex.Message, ex);
                    }
                }
            }
        }

    }
}