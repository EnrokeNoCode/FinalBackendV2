using Model.DTO.Servicios.RegistroVehiculo;
using Npgsql;
using Persistence;
using Persistence.SQL.Servicio;
using System.Data;
using System.Text.Json;

namespace Service.Servicio
{
    public class RegistroVehiculoService
    {
        private readonly DBConnector _cn;
        private readonly RegistroVehiculo_sql _query;

        public RegistroVehiculoService(DBConnector cn, RegistroVehiculo_sql query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<List<RegistroVehiculoListDTO>> RegistroVehiculoList()
        {
            var lista = new List<RegistroVehiculoListDTO>();
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consultaRegistroVehiculo = _query.SelectList();
                using (var cmdRegistroVehiculo = new NpgsqlCommand(consultaRegistroVehiculo, npgsql))
                {
                    using (var readerRegistroVehiculo = await cmdRegistroVehiculo.ExecuteReaderAsync())
                    {
                        while (await readerRegistroVehiculo.ReadAsync())
                        {
                            var registroVehiculo_ = new RegistroVehiculoListDTO
                            {
                                codregistro = (int)readerRegistroVehiculo["codregistro"],
                                fecharegistro = (DateTime)readerRegistroVehiculo["fecharegistro"],
                                nroregistro = (string)readerRegistroVehiculo["nroregistro"],
                                empleado = (string)readerRegistroVehiculo["empleado"],
                                desestmov = (string)readerRegistroVehiculo["desestmov"],
                                datosucursal = (string)readerRegistroVehiculo["datosucursal"],
                                datovehiculo = (string)readerRegistroVehiculo["datovehiculo"],
                                datocliente = (string)readerRegistroVehiculo["datocliente"]
                            };
                            lista.Add(registroVehiculo_);
                        }
                    }
                }
                await npgsql.CloseAsync();
            }
            return lista;
        }

        public async Task<int> InsertarRegistroVehiculo(RegistroVehiculoInsertDTO registro)
        {
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();

                using (var transaction = await npgsql.BeginTransactionAsync())
                {
                    try
                    {
                        string insertarRegistro = _query.Insert();

                        using (var cmd = new NpgsqlCommand(insertarRegistro, npgsql))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Transaction = transaction;

                            cmd.Parameters.AddWithValue("@codcliente", registro.codcliente);
                            cmd.Parameters.AddWithValue("@codsucursal", registro.codsucursal);
                            cmd.Parameters.AddWithValue("@codempleado", registro.codempleado);
                            cmd.Parameters.AddWithValue("@codestmov", registro.codestmov);
                            cmd.Parameters.AddWithValue("@codtipocomprobante", registro.codtipocomprobante);
                            cmd.Parameters.AddWithValue("@numregistro", registro.numregistro);
                            cmd.Parameters.AddWithValue("@fecharegistro", registro.fecharegistro);
                            cmd.Parameters.AddWithValue("@codmarca", registro.codmarca);
                            cmd.Parameters.AddWithValue("@modelo", registro.modelo);
                            cmd.Parameters.AddWithValue("@nrochapa", registro.nrochapa);
                            cmd.Parameters.AddWithValue("@nrochasis", registro.nrochasis);
                            cmd.Parameters.AddWithValue("@codterminal", registro.codterminal);

                            int codregistro = (int)await cmd.ExecuteScalarAsync();

                            await transaction.CommitAsync();
                            return codregistro;
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine("Error al insertar el registro: " + ex.Message);
                        throw;
                    }
                }
            }
        }
        
    }
}
