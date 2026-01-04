namespace Persistence.SQL.Servicio
{
    public class RegistroVehiculo_sql
    {
        public string SelectList()
        {
            return @"select r.codregistro, r.fecharegistro , t.numtipocomprobante || ' ' || r.numregistro  as nroregistro, e.nombre_emp || ' ' || e.apellido_emp as empleado , '' as desestmov ,
                    s.numsucursal || ' ' || s.dessucursal as datosucursal, 'Marca: ' || m.desmarca || ' Modelo: ' || v.modelo   || ' Chapa: ' || v.nrochapa as datovehiculo
                    , 'Nro Doc: ' || c.nrodoc  || ' Nombre Apellido: ' || c.nombre  || ', ' || c.apellido  as datocliente
                    from service.registrovehiculo r 
                    inner join referential.cliente c on r.codcliente = c.codcliente
                    inner join referential.vehiculo v on r.codvehiculo = v.codvehiculo and r.codcliente = v.codcliente 
                    inner join referential.marca m on v.codmarca  = m.codmarca 
                    inner join referential.tipocomprobante t on r.codtipocomprobante = t.codtipocomprobante 
                    inner join referential.empleado e on r.codempleado  = e.codempleado 
                    inner join referential.sucursal s on r.codsucursal = s.codsucursal ;
                    ";
        }
        public string Insert()
        {
            return @"select service.fn_insert_registrovehiculo2(
                    @codcliente ,
                    @codsucursal ,
                    @codempleado ,
                    @codestmov ,
                    @codtipocomprobante ,
                    @numregistro,
                    @fecharegistro,
                    @codmarca ,
                    @modelo,
                    @nrochapa,
                    @nrochasis,
                    @codterminal 
                )";
        }
    }
}
