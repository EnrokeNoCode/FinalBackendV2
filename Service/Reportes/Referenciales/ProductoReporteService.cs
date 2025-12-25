using Persistence.SQL.Reporte.Referenciales;
using Persistence;

namespace Service.Reportes.Referenciales
{
    public class ProductoReporteService
    {
        private readonly DBConnector _cn;
        private readonly ProductoReporteSQL _query;

        public ProductoReporteService(DBConnector cn, ProductoReporteSQL query)
        {
            _cn = cn;
            _query = query;
        }

    }
}
