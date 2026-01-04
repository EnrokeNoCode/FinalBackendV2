
namespace Persistence.SQL.Servicio
{
    public class DiagnosticoTecnico_sql
    {
        public string SelectList()
        { return @"select dt.coddiagnostico , t.numtipocomprobante || ' ' || dt.nrodiagnostico   as nrodiagnostico, dt.fechadiagnostico ,
                e.nombre_emp || ' ' || e.apellido_emp as empleado , e2.desestmov ,
                s.numsucursal || ' ' || s.dessucursal as datosucursal, 'Marca: ' || m.desmarca || ' Modelo: ' || v.modelo   || ' Chapa: ' || v.nrochapa as datovehiculo
                , 'Nro Doc: ' || c.nrodoc  || ' Nombre Apellido: ' || c.nombre  || ', ' || c.apellido  as datocliente
                from service.diagnosticotecnico dt
                inner join referential.vehiculo v on dt.codvehiculo = v.codvehiculo 
                inner join referential.tipocomprobante t on dt.codtipocomprobante = t.codtipocomprobante 
                inner join referential.cliente c on v.codcliente = c.codcliente
                inner join referential.marca m on v.codmarca  = m.codmarca 
                inner join referential.empleado e on dt.codempleado  = e.codempleado 
                inner join referential.estadomovimiento e2 on dt.codestmov = e2.codestmov 
                inner join referential.sucursal s on dt.codsucursal = s.codsucursal"; }

        public string Insert()
        {
            return @"select service.fn_insert_diagnosticotecnico(@codtipocomprobante,@codsucursal,@nrodiagnostico,@codestmov,@codempleado,@fechadiagnostico,@codvehiculo,@codterminal,@ultimo,@detalles)";
        }

        public string Select()
        {
            return @"select t.numtipocomprobante || ' ' || dt.nrodiagnostico   as nrodiagnostico, dt.fechadiagnostico ,
                    e.nombre_emp || ' ' || e.apellido_emp as empleado , 'Marca: ' || m.desmarca || ' Modelo: ' || v.modelo   || ' Chapa: ' || v.nrochapa as datovehiculo
                    , 'Nro Doc: ' || c.nrodoc  || ' Nombre Apellido: ' || c.nombre  || ', ' || c.apellido  as datocliente
                    from service.diagnosticotecnico dt
                    inner join referential.vehiculo v on dt.codvehiculo = v.codvehiculo 
                    inner join referential.tipocomprobante t on dt.codtipocomprobante = t.codtipocomprobante 
                    inner join referential.cliente c on v.codcliente = c.codcliente
                    inner join referential.marca m on v.codmarca  = m.codmarca 
                    inner join referential.empleado e on dt.codempleado  = e.codempleado where dt.coddiagnostico = @coddiagnostico";
        }

        public string SelectDet()
        {
            return @"select p.numparte , p.desparte , dtt.observacion  from service.diagnosticotecnicodet dtt 
                    inner join referential.partesvehiculo p on dtt.codparte  = p.codparte where dtt.coddiagnostico = @coddiagnostico";
        }

        public string SelectStatus()
        {
            return @"
                    SELECT codestmov 
                    FROM service.diagnosticotecnico
                    WHERE coddiagnostico = @coddiagnostico";
        }

        public string Update(int option)
        {
            string sentece = "";
            if (option == 1)
            {
                sentece = @"update service.diagnosticotecnico dt set codestmov = @codestado
                             where dt.coddiagnostico = @coddiagnostico ";
            }
            else if (option == 2)
            {
                sentece = @"";
            }
            return sentece;
        }
    }

}