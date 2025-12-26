namespace Persistence.SQL.Reporte.Referenciales
{
    public class ProductoReporteSQL
    {
        private string _query = "";
        public string ProductoListado()
        {
            _query = @"select prd.codigobarra || '- ' || prd.desproducto as datoproducto,
                        prv.nrodocprv || '- ' || prv.razonsocial  as datoproveedor, ti.desiva as datoiva,
                        CASE 
                            WHEN prd.afectastock = TRUE THEN 'Si Actualiza'
                            WHEN prd.afectastock = false THEN 'No Actualiza'
                            ELSE 'No Actualiza'
                        END AS afectastock,
                        CASE 
                            WHEN prd.activo = TRUE THEN 'Activo'
                            WHEN prd.activo = false THEN 'Inactivo'
                            ELSE 'Inactivo'
                        END AS estado,
                        prd.costoultimo ,
                        'Marca: ' || m.desmarca || ' |Familia: ' || f.desfamilia || ' |Rubro: ' || r.desrubro as datoseccion
                        from referential.producto prd
                        inner join referential.proveedor prv on prd.codproveedor = prv.codproveedor
                        inner join referential.marca m on prd.codmarca = m.codmarca 
                        inner join referential.rubro r on prd.codrubro = r.codrubro  
                        inner join referential.familia f on prd.codfamilia = f.codfamilia 
                        inner join referential.tipoiva ti on prd.codiva = ti.codiva ";
            return _query;
        }
    }
}
