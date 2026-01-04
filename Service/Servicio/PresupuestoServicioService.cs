using Persistence.SQL.Servicio;
using Persistence;
using Model.DTO;
using Model.DTO.Servicios.PresupuestoServicio;
using Npgsql;
using System.Data;

namespace Service.Servicio
{
    public class PresupuestoServicioService
    {
        private readonly DBConnector _cn;
        private readonly PresupuestoServicioSQL _query;

        public PresupuestoServicioService(DBConnector cn, PresupuestoServicioSQL query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<PaginadoDTO<PresupuestoServicioListDTO>> PresupuestoServicioLista(int page, int pageSize)
        {
            var lista = new List<PresupuestoServicioListDTO>();
            int totalItems = 0;
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var cmdCount = new NpgsqlCommand("SELECT COUNT(*) FROM service.presupuestoservicio", npgsql))
                {
                    totalItems = Convert.ToInt32(await cmdCount.ExecuteScalarAsync());
                }

                int offset = (page - 1) * pageSize;
                string consultapresupuestos_ = _query.SelectList(pageSize, offset);
                using (var cmdPresupuestoServicio = new NpgsqlCommand(consultapresupuestos_, npgsql))
                {
                    using (var readerPrstServicio = await cmdPresupuestoServicio.ExecuteReaderAsync())
                    {
                        while (await readerPrstServicio.ReadAsync())
                        {
                            var prstservicio_ = new PresupuestoServicioListDTO
                            {
                                codpresupuesto = (int)readerPrstServicio["codpresupuesto"],
                                fechapresupuesto = (string)readerPrstServicio["fechapresupuesto"],
                                nropresupuesto = (string)readerPrstServicio["nropresupuesto"],
                                cliente = (string)readerPrstServicio["cliente"],
                                vehiculo = (string)readerPrstServicio["vehiculo"],
                                desestmov = (string)readerPrstServicio["desestmov"],
                                diagnostico = (string)readerPrstServicio["diagnostico"],
                                totalpresupuesto = (decimal)readerPrstServicio["totalpresupuesto"]
                            };
                            lista.Add(prstservicio_);
                        }
                    }
                }
                await npgsql.CloseAsync();
            }
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new PaginadoDTO<PresupuestoServicioListDTO>
            {
                Data = lista,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            }; ;
        }

        public async Task<int> InsertarPresupuestoServicio(PresupuestoServicioInsertDTO dto)
        {
            using var conn = new NpgsqlConnection(_cn.cadenaSQL());
            await conn.OpenAsync();
            using var tx = await conn.BeginTransactionAsync();
            try
            {
                var insertarCabecera = _query.InsertPresupuestoCabecera();
                var cmdCab = new NpgsqlCommand(insertarCabecera, conn, tx);
                cmdCab.Parameters.AddWithValue("@codcliente", dto.codcliente);
                cmdCab.Parameters.AddWithValue("@codvehiculo", dto.codvehiculo);
                cmdCab.Parameters.AddWithValue("@coddiagnostico", dto.coddiagnostico);
                cmdCab.Parameters.AddWithValue("@codtipocomprobante", dto.codtipocomprobante);
                cmdCab.Parameters.AddWithValue("@codestmov", dto.codestmov);
                cmdCab.Parameters.AddWithValue("@nropresupuesto", dto.nropresupuesto ?? (object)DBNull.Value);
                cmdCab.Parameters.AddWithValue("@fechapresupuesto", dto.fechapresupuesto);
                cmdCab.Parameters.AddWithValue("@codsucursal", dto.codsucursal);
                cmdCab.Parameters.AddWithValue("@codempleado", dto.codempleado);
                cmdCab.Parameters.AddWithValue("@totaliva", dto.totaliva);
                cmdCab.Parameters.AddWithValue("@totalexenta", dto.totalexenta);
                cmdCab.Parameters.AddWithValue("@totalpresupuesto", dto.totalpresupuesto);

                int codPresupuesto = (int)await cmdCab.ExecuteScalarAsync();

                if (dto.detalleservicio != null)
                {
                    int linea = 0;
                    foreach (var s in dto.detalleservicio)
                    {
                        linea = linea + 1;
                        var insertarDetServicio = _query.InsertPresupuestoDetalleServicio();
                        var cmdSrv = new NpgsqlCommand(insertarDetServicio, conn, tx);
                        cmdSrv.Parameters.AddWithValue("@codp", codPresupuesto);
                        cmdSrv.Parameters.AddWithValue("@codts", s.codtiposervicio);
                        cmdSrv.Parameters.AddWithValue("@obs", s.observacion ?? "");
                        cmdSrv.Parameters.AddWithValue("@pn", s.precioneto);
                        cmdSrv.Parameters.AddWithValue("@pb", s.preciobruto);
                        cmdSrv.Parameters.AddWithValue("@linea", linea);
                        await cmdSrv.ExecuteNonQueryAsync();
                    }
                }
                if (dto.detallerepuesto != null)
                {
                    int linea = 0;
                    foreach (var r in dto.detallerepuesto)
                    {
                        linea = linea + 1;
                        var insertarDetRepuesto = _query.InsertPresupuestoDetalleRepuesto();
                        var cmdRep = new NpgsqlCommand(insertarDetRepuesto,conn, tx);
                        cmdRep.Parameters.AddWithValue("@codp", codPresupuesto);
                        cmdRep.Parameters.AddWithValue("@codprod", r.codproducto);
                        cmdRep.Parameters.AddWithValue("@cant", r.cantidad);
                        cmdRep.Parameters.AddWithValue("@pn", r.precioneto);
                        cmdRep.Parameters.AddWithValue("@pb", r.preciobruto);
                        cmdRep.Parameters.AddWithValue("@linea", linea);
                        await cmdRep.ExecuteNonQueryAsync();
                    }
                }
                await tx.CommitAsync();
                return codPresupuesto;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<string> ActualizarEstado(int codpresupuestoservicio, string mov)
        {

            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var transaction = await npgsql.BeginTransactionAsync())
                {
                    try
                    {
                        string actulizarestado = _query.UpdateEstado();
                        using (var cmd = new NpgsqlCommand(actulizarestado, npgsql))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@codpresupuesto", codpresupuestoservicio);
                            cmd.Parameters.AddWithValue("@mov", mov);
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
