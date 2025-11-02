namespace Persistence.SQL.Referencial
{
    public class EstadoMovimientoSQL
    {
        private string query = "";

        public string SelectEstadoMovimiento()
        {
            query = @"select e.codestmov, e.numestmov, e.desestmov from referential.estadomovimiento e ;";
            return query;
        }
    }
}
