using System.Data;
using System.Text.Json;
using Model.DTO.CobroVenta;
using Npgsql;
using Persistence;
using Persistence.SQL.Venta;

namespace Service.Venta
{
    public class CobrosVentaService
    {
        private readonly DBConnector _cn;
        private readonly CobroContado_sql _query;

        public CobrosVentaService(DBConnector cn, CobroContado_sql query)
        {
            _cn = cn;
            _query = query;
        }
        public async Task<string> InsertarCobros(CobroVentaContadoDTO cobrosContado)
        {
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();

                using (var transaction = await npgsql.BeginTransactionAsync())
                {
                    try
                    {
                        string insertarQuery = _query.InsertCobros();

                        using (var cmd = new NpgsqlCommand(insertarQuery, npgsql))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Transaction = transaction;

                            cmd.Parameters.AddWithValue("@codgestion", cobrosContado.codgestion);
                            var detallesVentasJson = JsonSerializer.Serialize(cobrosContado.cobroVentaContados);
                            cmd.Parameters.AddWithValue("@detallesVentas", NpgsqlTypes.NpgsqlDbType.Json, detallesVentasJson);
                            var detallesFPJson = JsonSerializer.Serialize(cobrosContado.cobroVentaFormaCobros);
                            cmd.Parameters.AddWithValue("@detallesFormaPago", NpgsqlTypes.NpgsqlDbType.Json, detallesFPJson);
                            string mensaje = (string)await cmd.ExecuteScalarAsync();
                            await transaction.CommitAsync();
                            return mensaje;
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine("Error al generar los Cobros: " + ex.Message);
                        throw;
                    }
                }
            }
        }
    }
}