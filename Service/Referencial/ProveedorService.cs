using Persistence.SQL.Referencial;
using Persistence;
using Model.DTO;
using Npgsql;
using Model.DTO.Referencial;
using System.Data;
using Api.POS.Model.DTO.Referencial;

namespace Service.Referencial
{
    public class ProveedorService
    {
        private readonly DBConnector _cn;
        private readonly ProveedorSQL _query;

        public ProveedorService(DBConnector cn, ProveedorSQL query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<List<ProveedorListCompraDTO>> GetProveedorCompra()
        {
            var lista = new List<ProveedorListCompraDTO>();
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consultaLista = _query.SelectProveedorCompra();
                using (var cmdLista = new NpgsqlCommand(consultaLista, npgsql))
                {
                    using (var readerLista = await cmdLista.ExecuteReaderAsync())
                    {
                        while (await readerLista.ReadAsync())
                        {
                            var listaProveedorCompra = new ProveedorListCompraDTO
                            {
                                codproveedor = (int)readerLista["codproveedor"],
                                nrodocprv = (string)readerLista["nrodocprv"],
                                razonsocial = (string)readerLista["razonsocial"],
                                nrotimbrado = (string)readerLista["nrotimbrado"],
                                fechaventimbrado = Convert.ToString((DateTime)readerLista["fechaventimbrado"])
                            };
                            lista.Add(listaProveedorCompra);
                        }
                    }
                }
                await npgsql.CloseAsync();
            }
            return lista;
        }

        public async Task<PaginadoDTO<ProveedorListDTO>> GetProveedor( int page, int pageSize)
        {
            var lista = new List<ProveedorListDTO>();
            int totalItems = 0;
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var cmdCount = new NpgsqlCommand($"SELECT COUNT(*) FROM referential.proveedor;", npgsql))
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
                string consulta = _query.SelectProveedor(pageSize, offset);
                using (var cmd = new NpgsqlCommand(consulta, npgsql))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            try
                            {
                                var listaProveedor = new ProveedorListDTO
                                {
                                    codproveedor = (int)reader["codproveedor"],
                                    proveedor = (string)reader["proveedor"],
                                    activo = (string)reader["activo"],
                                    datofacturacion = (string)reader["datofacturacion"],
                                    fecha = (DateTime)reader["fecha"],
                                    datocontacto = (string)reader["datocontacto"]
                                };
                                lista.Add(listaProveedor);
                            }
                            catch (Exception ex) { 
                            }
                            
                        }
                    }
                }
                await npgsql.CloseAsync();
            }
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new PaginadoDTO<ProveedorListDTO>
            {
                Data = lista,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<ProveedorGetDTO> ObtenerProveedor(int codproveedor)
        {
            using var cn = new NpgsqlConnection(_cn.cadenaSQL());
            await cn.OpenAsync();

            string selectProveedor = _query.SelectProveedorMod();

            using var cmd = new NpgsqlCommand(selectProveedor, cn);
            cmd.Parameters.AddWithValue("@codproveedor", codproveedor);

            using var dr = await cmd.ExecuteReaderAsync();
            if (!await dr.ReadAsync()) return null;

            return new ProveedorGetDTO
            {
                codproveedor = dr.GetInt32(0),
                nrodocprv = dr.GetString(1),
                razonsocial = dr.GetString(2),
                activo = dr.GetBoolean(3),
                codtipoidnt = dr.GetInt32(4),
                datotipoidnt = dr.GetString(5),
                direccionprv = dr.GetString(6),
                nrotelefprv = dr.GetString(7),
                contacto = dr.GetString(8),
                codciudad = dr.GetInt32(9),
                datociudad = dr.GetString(10),
                nrotimbrado = dr.GetString(11),
                fechaventimbrado = DateOnly.FromDateTime(dr.GetDateTime(12)),
            };
        }

        public async Task<string> ActualizarProveedor(ProveedorUpdateDTO dto)
        {
            using var cn = new NpgsqlConnection(_cn.cadenaSQL());
            await cn.OpenAsync();

            string actualizarProveedor = _query.UpdateProveedor();

            using var cmd = new NpgsqlCommand(actualizarProveedor, cn);

            cmd.Parameters.AddWithValue("@codproveedor", dto.codproveedor);
            cmd.Parameters.AddWithValue("@razonsocial", dto.razonsocial);
            cmd.Parameters.AddWithValue("@codciudad", dto.codciudad);
            cmd.Parameters.AddWithValue("@direccionprv", dto.direccionprv);
            cmd.Parameters.AddWithValue("@nrotelefprv", dto.nrotelefprv);
            cmd.Parameters.AddWithValue("@contacto", dto.contacto);
            cmd.Parameters.AddWithValue("@nrotimbrado", dto.nrotimbrado);
            cmd.Parameters.AddWithValue("@fechaventimbrado", dto.fechaventimbrado);

            int filas = await cmd.ExecuteNonQueryAsync();
            if (filas == 0)
                throw new Exception("No se pudo actualizar el proveedor");

            return "Proveedor actualizado correctamente";
        }

        public async Task<string> InsertarNuevoProveedor(ProveedorInsertDTO proveedor)
        {
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var transaction = await npgsql.BeginTransactionAsync())
                {
                    try
                    {
                        string insertarProveedor = _query.Insert();
                        using (var cmd = new NpgsqlCommand(insertarProveedor, npgsql))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Transaction = transaction;
                            cmd.Parameters.AddWithValue("@nrodocprv", proveedor.nrodocprv ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@razonsocial", proveedor.razonsocial ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@activo", proveedor.activo);
                            cmd.Parameters.AddWithValue("@codtipoidnt", proveedor.codtipoidnt);
                            cmd.Parameters.AddWithValue("@direccionprv", proveedor.direccionprv ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@nrotelefprv", proveedor.nrotelefprv ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@contacto", proveedor.contacto ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@codciudad", proveedor.codciudad);
                            cmd.Parameters.AddWithValue("@nrotimbrado", proveedor.nrotimbrado ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@fechaventimbrado", proveedor.fechaventimbrado);
                            string resultado = (string)await cmd.ExecuteScalarAsync();
                            await transaction.CommitAsync();
                            return resultado;
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine("Error al insertar el proveedor: " + ex.Message);
                        throw;
                    }
                }
            }
        }

        public async Task<string> ActulizarEliminarRegistro(int cod)
        {

            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var transaction = await npgsql.BeginTransactionAsync())
                {
                    try
                    {
                        string actulizarestado = _query.UpdateDeleteStatus();
                        using (var cmd = new NpgsqlCommand(actulizarestado, npgsql))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@cod", cod);
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
