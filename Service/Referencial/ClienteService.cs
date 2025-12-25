using Persistence.SQL.Referencial;
using Persistence;
using Model.DTO;
using Npgsql;
using Model.DTO.Ventas.PedidoVenta;
using System.Data;
using System.Text.Json;
using Model.DTO.Referencial;
using Api.POS.Model.DTO.Referencial;

namespace Service.Referencial
{
    public class ClienteService
    {
        private readonly DBConnector _cn;
        private readonly ClienteSQL _query;

        public ClienteService(DBConnector cn, ClienteSQL query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<List<ClienteListDTO>> GetListaClienteMov()
        {
            var lista = new List<ClienteListDTO>();
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consultaLista = _query.Select(1);
                using (var cmdLista = new NpgsqlCommand(consultaLista, npgsql))
                {
                    using (var readerLista = await cmdLista.ExecuteReaderAsync())
                    {
                        while (await readerLista.ReadAsync())
                        {
                            var listaClienteVenta = new ClienteListDTO
                            {
                                codcliente = (int)readerLista["codcliente"],
                                nrodoc = (string)readerLista["nrodoc"],
                                nombre_apellido = (string)readerLista["nombre_apellido"]
                            };
                            lista.Add(listaClienteVenta);
                        }
                    }
                }

                await npgsql.CloseAsync();
            }
            return lista;
        }

        public async Task<List<VehiculoListDTO>> GetListaClienteVehiculo(int codcliente)
        {
            var lista = new List<VehiculoListDTO>();
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consultaLista = _query.SelectListVehiculoCliente();
                using (var cmdLista = new NpgsqlCommand(consultaLista, npgsql))
                {
                    cmdLista.Parameters.AddWithValue("@codcliente", codcliente);
                    using (var readerLista = await cmdLista.ExecuteReaderAsync())
                    {
                        while (await readerLista.ReadAsync())
                        {
                            var listaClienteVehiculo = new VehiculoListDTO
                            {
                                codvehiculo = (int)readerLista["codvehiculo"],
                                datovehiculo = (string)readerLista["datovehiculo"],
                            };
                            lista.Add(listaClienteVehiculo);
                        }
                    }
                }
                await npgsql.CloseAsync();
            }
            return lista;
        }

        public async Task<PaginadoDTO<ClienteListDTO>> GetListaCliente(int page, int pageSize)
        {
            var lista = new List<ClienteListDTO>();
            int totalItems = 0;
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                try
                {
                    await npgsql.OpenAsync();
                    using (var cmdCount = new NpgsqlCommand($"SELECT COUNT(*) FROM referential.cliente;", npgsql))
                    {
                        totalItems = Convert.ToInt32(await cmdCount.ExecuteScalarAsync());            
                    }
                    int offset = (page - 1) * pageSize;
                    string consulta = _query.SelectList(pageSize, offset);
                    using (var cmd = new NpgsqlCommand(consulta, npgsql))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var listaCliente = new ClienteListDTO
                                {
                                    codcliente = (int)reader["codcliente"],
                                    nrodoc = (string)reader["nrodoc"],
                                    nombre_apellido = (string)reader["nombre_apellido"],
                                    listaprecio = (string)reader["listaprecio"],
                                    activo = (string)reader["activo"],
                                    clientecredito = (string)reader["clientecredito"],
                                    limitecredito = (decimal)reader["limitecredito"],
                                    fecha = (DateTime)reader["fecha"]
                                };
                                lista.Add(listaCliente);
                            }
                        }
                    }
                    await npgsql.CloseAsync();
                }
                catch(Exception ex)
                {

                }
            }
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            return new PaginadoDTO<ClienteListDTO>
            {
                Data = lista,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }
        public async Task<string> InsertarNuevoCliente(ClienteInsertDTO cliente)
        {
            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                using (var transaction = await npgsql.BeginTransactionAsync())
                {
                    try
                    {
                        string insertarCliente = _query.Insert();
                        using (var cmd = new NpgsqlCommand(insertarCliente, npgsql))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Transaction = transaction;

                            cmd.Parameters.AddWithValue("@nrodoc", cliente.nrodoc);
                            cmd.Parameters.AddWithValue("@nombre", cliente.nombre);
                            cmd.Parameters.AddWithValue("@apellido", cliente.apellido);
                            cmd.Parameters.AddWithValue("@activo", cliente.activo);
                            cmd.Parameters.AddWithValue("@codtipoidnt", cliente.codtipoidnt);
                            cmd.Parameters.AddWithValue("@direccion", cliente.direccion);
                            cmd.Parameters.AddWithValue("@nrotelef", cliente.nrotelef);
                            cmd.Parameters.AddWithValue("@codciudad", cliente.codciudad);
                            cmd.Parameters.AddWithValue("@codlista", cliente.codlista);
                            cmd.Parameters.AddWithValue("@clientecredito", cliente.clientecredito);
                            cmd.Parameters.AddWithValue("@limitecredito", cliente.limitecredito);
                            string resultado = (string)await cmd.ExecuteScalarAsync();
                            await transaction.CommitAsync();
                            return resultado;
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine("Error al insertar el Cliente: " + ex.Message);
                        throw;
                    }
                }
            }
        }

        public async Task<ClienteGetDTO> ObtenerCliente(int codcliente)
        {
            using var cn = new NpgsqlConnection(_cn.cadenaSQL());
            await cn.OpenAsync();

            string selectCliente = _query.SelectClienteMod();

            using var cmd = new NpgsqlCommand(selectCliente, cn);
            cmd.Parameters.AddWithValue("@codcliente", codcliente);

            using var dr = await cmd.ExecuteReaderAsync();
            if (!await dr.ReadAsync()) return null;

            return new ClienteGetDTO
            {
                codcliente = dr.GetInt32(0),
                nrodoc = dr.GetString(1),
                nombre = dr.GetString(2),
                apellido = dr.GetString(3),
                activo = dr.GetBoolean(4),
                codtipoidnt = dr.GetInt32(5),
                datotipoidnt = dr.GetString(6),
                direccion = dr.GetString(7),
                nrotelef = dr.GetString(8),
                codciudad = dr.GetInt32(9),
                datociudad = dr.GetString(10),
                codlista = dr.GetInt32(11),
                datolista = dr.GetString(12),
                clientecredito = dr.GetBoolean(13),
                limitecredito = dr.GetDecimal(14)
            };
        }

        public async Task<string> ActualizarCliente(ClienteUpdateDTO dto)
        {
            using var cn = new NpgsqlConnection(_cn.cadenaSQL());
            await cn.OpenAsync();

            string actualizarCliente = _query.UpdateCliente();

            using var cmd = new NpgsqlCommand(actualizarCliente, cn);

            cmd.Parameters.AddWithValue("@codcliente", dto.codcliente);
            cmd.Parameters.AddWithValue("@nombre", dto.nombre);
            cmd.Parameters.AddWithValue("@apellido", dto.apellido);
            cmd.Parameters.AddWithValue("@codciudad", dto.codciudad);
            cmd.Parameters.AddWithValue("@direccion", dto.direccion);
            cmd.Parameters.AddWithValue("@nrotelef", dto.nrotelef);
            cmd.Parameters.AddWithValue("@codlista", dto.codlista);
            cmd.Parameters.AddWithValue("@clientecredito", dto.clientecredito);
            cmd.Parameters.AddWithValue("@limitecredito", dto.limitecredito);

            int filas = await cmd.ExecuteNonQueryAsync();
            if (filas == 0)
                throw new Exception("No se pudo actualizar el cliente");

            return "Cliente actualizado correctamente";
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
