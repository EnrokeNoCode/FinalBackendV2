namespace Persistence.SQL.Referencial
{
    public class SucursalSQL
    {
        private string query = "";

        public string SelectListSession()
        {
            query = @"select s.codsucursal, s.numsucursal, s.dessucursal from referential.sucursal s;";
            return query;
        }
    }
}
