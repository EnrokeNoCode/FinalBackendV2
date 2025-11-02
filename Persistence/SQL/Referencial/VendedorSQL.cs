namespace Persistence.SQL.Referencial
{
    public class VendedorSQL
    {
        private string query = "";

        public string SelectListVendedor()
        {
            query = @"select v.codvendedor, v.numvendedor || '- ' || e.nombre_emp || ', ' || e.apellido_emp as datovendedor 
               from referential.vendedor v 
               inner join referential.empleado e on v.codempleado = e.codempleado ;";
            return query;
        }
    }
}
