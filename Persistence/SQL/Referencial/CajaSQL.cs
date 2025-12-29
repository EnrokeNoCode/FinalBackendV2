namespace Persistence.SQL.Referencial
{
    public class CajaSQL
    {
        private string query = "";

        public string SelectGestion(int pageSize, int offset)
        {
            query = @$"
                    SELECT DISTINCT ON (c.codcaja)
                        c.codcaja,
                        c.numcaja,
                        c.descaja,
                        CASE 
                            WHEN cg.estado = TRUE THEN 'CERRADO'
                            WHEN cg.estado = false THEN 'ABIERTO'
                            ELSE 'Sin Gestion'
                        END AS estado,
                        coalesce (cg.codcobrador,0) as codcobrador,
                        COALESCE(cg.codgestion, 0) AS codgestion,
                        CASE 
                            WHEN cg.estado = TRUE THEN 'Sin Cobrador'                                   
                            WHEN COALESCE(cg.codcobrador, 0) = 0 THEN 'Sin Cobrador'                    
                            ELSE cbr.numcobrador || ' - ' || e.nombre_emp                               
                        END AS cobrador
                    FROM referential.caja c
                    LEFT JOIN referential.cajagestion cg ON c.codcaja = cg.codcaja
                    LEFT JOIN referential.cobrador cbr ON cg.codcobrador = cbr.codcobrador
                    LEFT JOIN referential.empleado e ON cbr.codempleado = e.codempleado
                    WHERE c.codsucursal = @codsucursal
                    ORDER BY c.codcaja, cg.codgestion DESC
                    LIMIT {pageSize} OFFSET {offset};";
            return query;
        }

        public string SelectGestionCobranza()
        {
            query = @"select 
                    cj.codgestion , cj.fechaapertura , cj.fechacierre ,
                    c.codcaja , c.numcaja , c.descaja , 
                    cbr.numcobrador || '- ' || emp.nombre_emp || ', ' || emp.apellido_emp as cobrador
                    from referential.cajagestion cj
                    inner join referential.caja c on cj.codcaja = c.codcaja 
                    inner join referential.cobrador cbr on cj.codcobrador = cbr.codcobrador 
                    inner join referential.empleado emp on cbr.codempleado = emp.codempleado
                    where cj.codgestion = @codgestion;";
            return query;
        }

        public string SelectGestionDetalle()
        {
            query = @"select cg.codgestion, s.numsucursal, s.dessucursal, c.numcaja, c.descaja, 
                        cbr.numcobrador || '- ' || emp.nombre_emp || ', ' || emp.apellido_emp as cobrador,
                        cg.fechaapertura , cg.fechacierre , cg.montoapertura , cg.montocierre 
                        from referential.caja c
                        inner join referential.cajagestion cg on c.codcaja = cg.codcaja 
                        inner join referential.sucursal s on c.codsucursal = s.codsucursal 
                        inner join referential.cobrador cbr on cg.codcobrador = cbr.codcobrador 
                        inner join referential.empleado emp on cbr.codempleado = emp.codempleado 
                        where c.codcaja = @codcaja 
                        order by cg.codgestion ,s.numsucursal , c.numcaja , cbr.numcobrador , cg.fechaapertura , cg.fechacierre ;";
            return query;
        }

        public string InsertApertura()
        {
            query = @"SELECT referential.fn_apertura_caja(@codcaja, @codcobrador, @fechaapertura, @montoapertura, @codterminal);";
            return query;
        }
        public string UpdateCierre()
        {
            query = @"select referential.fn_cierre_caja(@codgestion, @fechacierre, @montocierre);";
            return query;
        }
    }
}