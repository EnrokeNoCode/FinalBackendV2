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

        public string Insert()
        {
            query = @"select referential.fn_insert_proveedor(@nrodocprv, @razonsocial, @activo, @codtipoidnt, @direccionprv, @nrotelefprv, @contacto, @codciudad, @nrotimbrado, @fechaventimbrado)";
            return query;
        }

        public string SelectProveedorMod()
        {
            query = @"select prv.codproveedor, prv.nrodocprv , prv.razonsocial , coalesce(prv.activo, false) as activo, prv.codtipoidnt,
                        ti.numtipoidnt || '- ' || ti.desctipoidnt as datotipoidnt,
                        coalesce(prv.direccionprv, '') as direccionprv , coalesce(prv.nrotelefprv,'') as nrotelefprv,  coalesce(prv.contacto ,'') as contacto,
                        prv.codciudad,  cd.numciudad || '- ' || cd.descciudad as datociudad, prv.nrotimbrado , prv.fechaventimbrado 
                        from referential.proveedor prv 
                        inner join referential.tipo_identificacion ti on prv.codtipoidnt = ti.codtipoidnt 
                        inner join referential.ciudad cd on prv.codciudad = cd.codciudad 
                        where prv.codproveedor = @codproveedor ;";
            return query;
        }

        public string UpdateProveedor()
        {
            query = @"update referential.proveedor set razonsocial = @razonsocial, codciudad = @codciudad, direccionprv = @direccionprv, nrotelefprv = @nrotelefprv, contacto = @contacto,
                                                        nrotimbrado = @nrotimbrado , fechaventimbrado = @fechaventimbrado where codproveedor = @codproveedor ";
            return query;
        }

        public string UpdateDeleteStatus()
        {
            query = @"select referential.fn_inactivar_eliminar_registro(@cod, 'proveedor')";
            return query;
        }
    }
}
