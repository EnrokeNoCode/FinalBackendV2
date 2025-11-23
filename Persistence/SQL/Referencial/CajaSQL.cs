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