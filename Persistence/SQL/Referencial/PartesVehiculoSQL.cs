namespace Persistence.SQL.Referencial
{
    public class PartesVehiculoSQL
    {
        private string query = "";

        public string SelectPartesVehiculo()
        {
            query = @"select p.codparte, p.numparte, p.desparte from referential.partesvehiculo p ;";
            return query;
        }
    }
}
