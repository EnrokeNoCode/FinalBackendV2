namespace Persistence.SQL.Referencial
{
    public class TipoTarjetaSQL
    {
        private string query = "";

        public string Select()
        {
            query = @"select t.codtipotar , t.numtipotar , t.destipotar  from referential.tipotarjeta t;";
            return query;
        }
    }
}
