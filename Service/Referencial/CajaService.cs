using Model.DTO;
using Npgsql;
using Persistence;
using Persistence.SQL.Referencial;

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
                    catch (Exception ex) { 
                        Console.WriteLine(ex.Message);
                    }
                }

                int offset = (page - 1) * pageSize;
                string consulta = _query.SelectGestion(pageSize, offset);
                using (var cmd = new NpgsqlCommand(consulta, npgsql))
                {
                    cmd.Parameters.AddWithValue("codsucursal", codsucursal);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            var cajaGestion = new CajaGestionListDTO
                            {
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
    }
}