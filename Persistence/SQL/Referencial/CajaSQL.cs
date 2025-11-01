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

        public string InsertApertura()
        {
            query = @"insert into referential.cajagestion(codgestion,codcaja,codcobrador,fechaapertura,montoapertura,estado,codterminal)
                        values(@codgestion, @codcaja, @codcobrador, @fechaapertura,@montoapertura, false, @codterminal);";
            return query;
        }
        public string UpdateCierre()
        {
            query = @"update referential.cajagestion
                        set fechacierre = @fechacierre,
                            montocierre = @montocierre,
                            estado = true,
                            codterminalcierre = @codterminalcierre
                        where codgestion = @codgestion;";
            return query;
        }
    }
}