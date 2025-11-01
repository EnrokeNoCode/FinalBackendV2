using Model.DTO.Servicios.DiagnosticoTecnico;
using Npgsql;
using Persistence;
using Persistence.SQL.Servicio;
using System.Data;
using System.Text.Json;

namespace Service.Servicio
{
    public class DiagnosticoTecnicoService
    {
        private readonly DBConnector _cn;
        private readonly DiagnosticoTecnico_sql _query;

        public DiagnosticoTecnicoService(DBConnector cn, DiagnosticoTecnico_sql query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<List<DiagnosticoTecnicoListDTO>> DiagnosticoTecnicoList()
        {
            var lista = new List<DiagnosticoTecnicoListDTO>();
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consultaDiagnosticoTecnico = _query.SelectList();
                using (var cmdDiagnosticoTecnico = new NpgsqlCommand(consultaDiagnosticoTecnico, npgsql))
                {
                    using (var readerDiagnosticoTecnico = await cmdDiagnosticoTecnico.ExecuteReaderAsync())
                    {
                        while (await readerDiagnosticoTecnico.ReadAsync())
                        {
                            var diagnosticotecnico_ = new DiagnosticoTecnicoListDTO
                            {
                                coddiagnostico = (int)readerDiagnosticoTecnico["coddiagnostico"],
                                fechadiagnostico = (DateTime)readerDiagnosticoTecnico["fechadiagnostico"],
                                nrodiagnostico = (string)readerDiagnosticoTecnico["nrodiagnostico"],
                                empleado = (string)readerDiagnosticoTecnico["empleado"],
                                desestmov = (string)readerDiagnosticoTecnico["desestmov"],
                                datosucursal = (string)readerDiagnosticoTecnico["datosucursal"],
                                datovehiculo = (string)readerDiagnosticoTecnico["datovehiculo"],
                                datocliente = (string)readerDiagnosticoTecnico["datocliente"]
                            };
                            lista.Add(diagnosticotecnico_);
                        }
                    }
                }
                await npgsql.CloseAsync();
            }
            return lista;
        }

        public async Task<DiagnosticoTecnicoListWithDetDTO?> DiagnosticoTecnicoConDet(int coddiagnostico)
        {
            DiagnosticoTecnicoListWithDetDTO? diagnosticotecnico = null;
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consultadiagnosticoteccab_ = _query.SelectList();
                using (var cmdDiagnosticoTecnico = new NpgsqlCommand(consultadiagnosticoteccab_, npgsql))
                {
                    cmdDiagnosticoTecnico.Parameters.AddWithValue("@coddiagnostico", coddiagnostico);
                    using (var reader = await cmdDiagnosticoTecnico.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            diagnosticotecnico = new DiagnosticoTecnicoListWithDetDTO
                            {
                                fechadiagnostico = reader.GetDateTime(reader.GetOrdinal("fechadiagnostico")),
                                nrodiagnostico = reader.GetString(reader.GetOrdinal("nrodiagnostico")),
                                datocliente = reader.GetString(reader.GetOrdinal("datocliente")),
                                datovehiculo = reader.GetString(reader.GetOrdinal("datovehiculo")),
                                empleado = reader.GetString(reader.GetOrdinal("empleado")),
                                desestmov = reader.GetString(reader.GetOrdinal("desestmov")),
                                diagtecwithdet = new List<DiagnosticoTecnicoListDetDTO>()
                            };
                        }
                    }
                }

                if (diagnosticotecnico == null)
                    return null;

                string consultaDetalle_ = _query.SelectDet();
                using (var cmdDetalle = new NpgsqlCommand(consultaDetalle_, npgsql))
                {
                    cmdDetalle.Parameters.AddWithValue("@coddiagnostico", coddiagnostico);
                    using (var readerDet = await cmdDetalle.ExecuteReaderAsync())
                    {
                        while (await readerDet.ReadAsync())
                        {
                            var detalle = new DiagnosticoTecnicoListDetDTO
                            {
                                numparte = readerDet.GetString(readerDet.GetOrdinal("numparte")),
                                desparte = readerDet.GetString(readerDet.GetOrdinal("desparte")),
                                observacion = readerDet.GetString(readerDet.GetOrdinal("observacion")),
                            };
                            diagnosticotecnico.diagtecwithdet?.Add(detalle);
                        }
                    }
                }
            }

            return diagnosticotecnico;
        }

        public async Task<int> ActualizarEstado(int coddiagnostico, int codestado)
        {
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var transaction = await npgsql.BeginTransactionAsync())
                {
                    try
                    {
                        string consultaEstado = _query.SelectStatus();

                        using (var cmdValidar = new NpgsqlCommand(consultaEstado, npgsql))
                        {
                            cmdValidar.Parameters.AddWithValue("@coddiagnostico", coddiagnostico);
                            cmdValidar.Transaction = transaction;
                            var estadoActual = await cmdValidar.ExecuteScalarAsync();

                            switch ((int)estadoActual)
                            {
                                case 3:
                                    throw new Exception("El diagnostico ya se encuentra cancelado, favor verificar");
                                case 4:
                                    throw new Exception("El diagnostico ya fue anulado, favor verificar");
                                case 5:
                                    throw new Exception("El diagnostico ya fue finalizado, favor verificar");
                                case 6:
                                    throw new Exception("El estado no puede ser cambiado. Esta En control");
                                case 7:
                                    throw new Exception("El diagnostico fue marcado como No Reparable");
                            }
                        }
                        string actulizarestado = _query.Update(1);

                        using (var cmd = new NpgsqlCommand(actulizarestado, npgsql))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@coddiagnostico", coddiagnostico);
                            cmd.Parameters.AddWithValue("@codestado", codestado);
                            cmd.Transaction = transaction;
                            int filasAfectadas = await cmd.ExecuteNonQueryAsync();
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
        
        public async Task<int> InsertarDiagnosticoTecnico(DiagnosticoTecnicoInsertDTO diagtec)
        {
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();

                using (var transaction = await npgsql.BeginTransactionAsync())
                {
                    try
                    {
                        string insertarDiagnostico = _query.Insert();

                        using (var cmd = new NpgsqlCommand(insertarDiagnostico, npgsql))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Transaction = transaction;

                            cmd.Parameters.AddWithValue("@codtipocomprobante", diagtec.codtipocomprobante);
                            cmd.Parameters.AddWithValue("@codsucursal", diagtec.codsucursal);
                            cmd.Parameters.AddWithValue("@nrodiagnostico", diagtec.nrodiagnostico);
                            cmd.Parameters.AddWithValue("@codestmov", diagtec.codestmov);
                            cmd.Parameters.AddWithValue("@codempleado", diagtec.codempleado);
                            cmd.Parameters.AddWithValue("@fechadiagnostico", diagtec.fechadiagnostico);
                            cmd.Parameters.AddWithValue("@codvehiculo", diagtec.codvehiculo);
                            cmd.Parameters.AddWithValue("@codterminal", diagtec.codterminal);
                            cmd.Parameters.AddWithValue("@ultimo", diagtec.ultimo);

                            var detallesJson = JsonSerializer.Serialize(diagtec.diagtecdet);
                            cmd.Parameters.AddWithValue("@detalles", NpgsqlTypes.NpgsqlDbType.Json, detallesJson);

                            int codpedcompra = (int)await cmd.ExecuteScalarAsync();

                            await transaction.CommitAsync();
                            return codpedcompra;
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