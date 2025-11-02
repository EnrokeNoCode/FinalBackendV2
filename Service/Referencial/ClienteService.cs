using Persistence.SQL.Referencial;
using Persistence;
using Model.DTO;
using Npgsql;

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


    }
}
