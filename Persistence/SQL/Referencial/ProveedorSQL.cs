namespace Persistence.SQL.Referencial
{
    public class ProveedorSQL
    {
        private string query = "";

        public string SelectProveedor(int pageSize, int offset)
        {
            query = @$"select p.codproveedor, p.nrodocprv || ' ' || p.razonsocial as proveedor, 'Fecha Venc.: ' || p.fechaventimbrado || ' Nro. Timbrado: ' || p.fechaventimbrado as datofacturacion,
                        CASE 
                            WHEN p.activo = TRUE THEN 'Activo'
                            WHEN p.activo = false THEN 'Inactivo'
                            ELSE 'Inactivo'
                        END AS activo,
                        CASE 
                            WHEN p.activo = TRUE THEN p.fechaalta
                            WHEN p.activo = false THEN p.fechabaja
                        END AS fecha, coalesce (p.nrotelefprv,'') || ' ' ||coalesce ( p.contacto,'') as datocontacto
                        from referential.proveedor p  LIMIT {pageSize} OFFSET {offset};";
            return query;
        }

        public string SelectProveedorCompra()
        {
            query = @"select p.codproveedor, p.nrodocprv, p.razonsocial, p.nrotimbrado, p.fechaventimbrado from referential.proveedor p ;";
            return query;
        }
    }
}
