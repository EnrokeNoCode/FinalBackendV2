namespace Persistence.SQL.Referencial
{
    public class ClienteSQL
    {
        private string query = "";

        public string SelectList(int pageSize, int offset)
        {
            query = $@"select c.codcliente, c.nrodoc, c.nombre || ', ' || c.apellido as nombre_apellido,
                        CASE 
                            WHEN c.activo = TRUE THEN 'Activo'
                            WHEN c.activo = false THEN 'Inactivo'
                            ELSE 'Inactivo'
                        END AS activo,
                        CASE 
                            WHEN c.clientecredito = TRUE THEN 'Autorizado'
                            WHEN c.clientecredito = false THEN 'No Autorizado'
                            ELSE 'No Autorizado'
                        END AS clientecredito,
                        coalesce(c.limitecredito,0) as limitecredito,
                        CASE 
                            WHEN c.activo = TRUE THEN c.fechaalta 
                            WHEN c.activo = false THEN c.fechabaja 
                        END AS fecha,
                        t.deslista as listaprecio
                    from referential.cliente c 
                    inner join referential.tipolistaprecio t on c.codlista = t.codlista LIMIT {pageSize} OFFSET {offset};";
            return query;
        }

        public string SelectListVehiculoCliente()
        {
            query = @"select v.codvehiculo, 'Marca: ' || m.desmarca || ' | Modelo: ' || v.modelo || ' | Nro. Chapa: ' || v.nrochapa as datovehiculo 
                        from referential.vehiculo v 
                        inner join referential.marca m on v.codmarca = m.codmarca where v.codcliente = @codcliente ";
            return query;
        }

        public string Select(int opcion) 
        {
            switch (opcion)
            {
                case 1:
                    //Este es para la pantalla de ventas y servicios
                    query = @"select c.codcliente, c.nrodoc, c.nombre || ', ' || c.apellido as nombre_apellido from referential.cliente c";
                    break;
            }
            
            return query; 
        }
    }
}
