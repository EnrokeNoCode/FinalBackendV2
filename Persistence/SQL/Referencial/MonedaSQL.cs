namespace Persistence.SQL.Referencial
{
    public class MonedaSQL
    {
        private string query = "";

        public string SelectMoneda()
        {
            query = @"select m.codmoneda, m.nummoneda, m.desmoneda from referential.moneda m ;";
            return query;
        }
    }
}
