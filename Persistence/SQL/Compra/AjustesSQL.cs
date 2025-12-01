namespace Persistence.SQL.Compra
{
    public class Ajustes_Sql
    {
         public string SelectList(int pageSize, int offset)
         {
            return $@"select a.codajuste , a.fechaajuste , tc.numtipocomprobante  || '- ' || a.numajuste as datoajuste,
                    s.dessucursal , case when a.condicion = 0 then 'SUMA por ' || ma.desmotivo else 'RESTA por ' || ma.desmotivo end as datomotivo ,
                    e.nombre_emp || ', ' || e.apellido_emp as datoempleado, 
                    case
                    	when coalesce(a.estado, 0) = 0 then 'ABIERTO'
                    	when coalesce(a.estado, 0) = 1 then 'CERRADO'
                    end as estado
                    from shared.ajustes a 
                    inner join referential.sucursal s on a.codsucursal = s.codsucursal 
                    inner join referential.empleado e on a.codempleado = e.codempleado 
                    inner join referential.tipocomprobante tc on a.codtipocomprobante  = tc.codtipocomprobante 
                    inner join referential.motivoajuste ma on a.codmotivo = ma.codmotivo
                    where a.codsucursal = @codsucursal
                    order by a.codajuste , a.fechaajuste desc
                    limit {pageSize} offset {offset}; ";
         }

        public string Insert()
        {
            return @"
                    SELECT shared.fn_insert_ajustes(
                        @codtipocomprobante,
                        @codsucursal,
                        @numajuste,
                        @fechaajuste,
                        @codmotivo,
                        @codempleado,
                        @condicion,
                        @codterminal,
                        @ultimo,
                        @detalles
                    );";
        }

        public string Update(int option)
        {
            string query = "";
            switch (option)
            {
                case 1:
                    // para el update de estado
                    query = @"select shared.fn_update_ajustecerrar(@codajuste)";
                    break;
                case 2:
                    // para el update de detalles
                    query = @"select shared.fn_update_ajustedet(@codajuste, @codsucursal, @detalles)";
                    break;
            }
            return query;
        }

        public string Select()
        {
            return @"select a.codajuste , a.fechaajuste , t.numtipocomprobante, t.destipocomprobante, 
                    a.numajuste , e.nombre_emp || ' ' || e.apellido_emp as empleado , case
                    	when coalesce(a.estado, 0) = 0 then 'ABIERTO'
                    	when coalesce(a.estado, 0) = 1 then 'CERRADO'
                    end as estado,
                    s.numsucursal , s.dessucursal,
                    case when a.condicion = 0 then 'SUMA por ' || ma.desmotivo else 'RESTA por ' || ma.desmotivo end as datomotivo 
                    from shared.ajustes a
                    inner join referential.tipocomprobante t  on a.codtipocomprobante  = t.codtipocomprobante 
                    inner join referential.empleado e on a.codempleado  = e.codempleado 
                    inner join referential.sucursal s on a.codsucursal = s.codsucursal
                    inner join referential.motivoajuste ma on a.codmotivo = ma.codmotivo
                   	where a.codajuste = @codajuste; ";
        }

        public string SelectWithDetail()
        {
            return @"select ad.codajuste, ad.codproducto, p2.codigobarra, p2.desproducto, ad.cantidad
                    from shared.ajustesdet ad
                    inner join referential.producto p2 on ad.codproducto = P2.codproducto
                    where ad.codajuste = @codajuste;";
        }

    }
}