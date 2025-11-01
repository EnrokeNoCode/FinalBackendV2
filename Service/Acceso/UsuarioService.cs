using System.Data;
using Model.DTO;
using Npgsql;
using Persistence;
using Persistence.SQL.Acceso;

namespace Service.Acceso
{
    public class UsuarioService
    {
        private readonly DBConnector _cn;
        private readonly UsuarioSQL _query;

        public UsuarioService(DBConnector cn, UsuarioSQL query)
        {
            _cn = cn;
            _query = query;
        }
        public async Task<UsuarioDTO?> AccessUser(string nomusuario, string passusuario)
        {
            await using var npgsql = new NpgsqlConnection(_cn.cadenaSQL());
            await npgsql.OpenAsync();

            string selectQuery = _query.SelectUser();

            await using var cmd = new NpgsqlCommand(selectQuery, npgsql);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@nomusuario", nomusuario);
            cmd.Parameters.AddWithValue("@passusuario", passusuario);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new UsuarioDTO
                {
                    codusuario = reader.GetInt32(reader.GetOrdinal("codusuario")),
                    codempleado = reader.GetInt32(reader.GetOrdinal("codempleado")),
                    apellido_emp = reader.GetString(reader.GetOrdinal("apellido_emp")),
                    nombre_emp = reader.GetString(reader.GetOrdinal("nombre_emp"))
                };
            }
            return null;
        }

        public async Task<DatosTerminalDTO?> TerminalData(int codsucursal, string pcasociado)
        {
             await using var npgsql = new NpgsqlConnection(_cn.cadenaSQL());
            await npgsql.OpenAsync();

            string selectQuery = _query.SelectTerminal();
            await using var cmd = new NpgsqlCommand(selectQuery, npgsql);
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@codsucursal", codsucursal);
            cmd.Parameters.AddWithValue("@pcasociado", pcasociado);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new DatosTerminalDTO
                {
                    codterminal = reader.GetInt32(reader.GetOrdinal("codterminal")),
                    numterminal = reader.GetString(reader.GetOrdinal("numterminal")),
                    desterminal = reader.GetString(reader.GetOrdinal("desterminal")),
                    codsucursal = reader.GetInt32(reader.GetOrdinal("codsucursal")),
                    numsucursal = reader.GetString(reader.GetOrdinal("numsucursal")),
                    dessucursal = reader.GetString(reader.GetOrdinal("dessucursal")),
                };
            }
            return null;
        }
    }
}