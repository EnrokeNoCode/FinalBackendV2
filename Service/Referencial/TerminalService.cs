using Persistence.SQL.Referencial;
using Persistence;
using Model.DTO;
using Npgsql;

namespace Service.Referencial
{
    public class TerminalService
    {
        private readonly DBConnector _cn;
        private readonly TerminalSQL _query;

        public TerminalService(DBConnector cn, TerminalSQL query)
        {
            _cn = cn;
            _query = query;
        }

        public async Task<ComprobanteTerminalListDTO?> GetComprobanteTerminal(int codterminal, int codtipocomprobante)
        {
            ComprobanteTerminalListDTO? comprobante = null;

            using (var npgsql = new NpgsqlConnection(_cn.cadenaSQL()))
            {
                await npgsql.OpenAsync();
                string consultaLista = _query.SelectTerminalComprobante();

                using (var cmdLista = new NpgsqlCommand(consultaLista, npgsql))
                {
                    cmdLista.Parameters.AddWithValue("@codterminal", codterminal);
                    cmdLista.Parameters.AddWithValue("@codtipocomprobante", codtipocomprobante);

                    using (var reader = await cmdLista.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            comprobante = new ComprobanteTerminalListDTO
                            {
                                codterminal = (int)reader["codterminal"],
                                inciovalidez = (DateTime)reader["iniciovalidez"],
                                finvalidez = (DateTime)reader["finvalidez"],
                                inicio = Convert.ToInt32(reader["inicio"]),
                                fin = Convert.ToInt32(reader["fin"]),
                                actual = Convert.ToInt32(reader["actual"]),
                                nrotimbrado = Convert.ToInt32(reader["nrotimbrado"]),
                                datocomprobante = (string)reader["datocomprobante"]
                            };
                        }
                    }
                }
            }

            return comprobante;
        }

    }
}
