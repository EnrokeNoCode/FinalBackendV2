namespace Persistence.SQL.Referencial
{
    public class CobradorSQL
    {
        private string query = "";

        public string SelectCobradorCaja()
        {
            query = @"select cbr.codcobrador, cbr.numcobrador || '- ' || e.nombre_emp as datocobrador 
                        from referential.cobrador cbr
                        inner join referential.empleado e on cbr.codempleado = e.codempleado ;";
            return query;
        }
    }
}
