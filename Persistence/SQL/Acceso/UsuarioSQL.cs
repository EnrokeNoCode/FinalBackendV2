namespace Persistence.SQL.Acceso
{
    public class UsuarioSQL
    {
        private string query = "";
        public string SelectUser()
        {
            query = "";
            return @"select u.codusuario, u.codempleado, emp.apellido_emp, emp.nombre_emp from access.usuario u 
                    inner join referential.empleado emp on u.codempleado = emp.codempleado
                    where u.nomusuario = @nomusuario and u.passusuario = @passusuario ;";
        }

        public string SelectTerminal()
        {
            query = "";
            return @"select 
                t.codterminal,
                t.numterminal, t.desterminal,
                t.codsucursal,
                s.numsucursal, s.dessucursal
                from access.terminal t 
                inner join referential.sucursal s on t.codsucursal = s.codsucursal
                where t.pcasociado = @pcasociado and t.codsucursal = @codsucursal;
                ";
        }
    }


}