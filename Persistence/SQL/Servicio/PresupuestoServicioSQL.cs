namespace Persistence.SQL.Servicio
{
    public class PresupuestoServicioSQL
    {
        private string _query;

        public string SelectList(int pageSize, int offset)
        {
            _query = $@"select ps.codpresupuesto, TO_CHAR(ps.fechapresupuesto, 'DD/MM/YYYY HH:MM') as fechapresupuesto, tc1.numtipocomprobante || '- ' || ps.nropresupuesto as nropresupuesto,
                        c.nrodoc || '- ' || c.nombre  || ', ' ||c.apellido  as cliente, 'Nro. Chapa: ' || v.nrochapa || ' |Marca: ' || m.desmarca  || ' |Modelo: ' || v.modelo as vehiculo,
                        em.desestmov, tc2.numtipocomprobante || '- ' || dt.nrodiagnostico as diagnostico , ps.totalpresupuesto 
                        from service.presupuestoservicio ps
                        left join service.diagnosticotecnico dt on ps.coddiagnostico = dt.coddiagnostico 
                        inner join referential.cliente c on ps.codcliente= c.codcliente 
                        inner join referential.vehiculo v on ps.codvehiculo = v.codvehiculo 
                        inner join referential.marca m on v.codmarca = m.codmarca 
                        inner join referential.tipocomprobante tc1 on ps.codtipocomprobante = tc1.codtipocomprobante 
                        inner join referential.tipocomprobante tc2 on dt.codtipocomprobante = tc2.codtipocomprobante
                        inner join referential.estadomovimiento em on ps.codestmov = em.codestmov order by 1
                        limit {pageSize} offset {offset};";
            return _query;  
        }
        public string InsertPresupuestoCabecera()
        {
            _query = @"SELECT service.fn_insert_presupuesto_servicio(@codcliente,@codvehiculo,
                                @coddiagnostico,@codtipocomprobante,@codestmov,@nropresupuesto,
                                @fechapresupuesto,@codsucursal,
                                @codempleado,@totaliva,@totalexenta,@totalpresupuesto) ;";
            return _query;
        }
        public string InsertPresupuestoDetalleServicio()
        {
            _query = @"CALL service.sp_insert_presupuesto_servicio_detservicio(@codp,@codts,@obs,@pn,@pb,@linea) ;";
            return _query;
        }
        public string InsertPresupuestoDetalleRepuesto()
        {
            _query = @"CALL service.sp_insert_presupuesto_servicio_detrepuesto(@codp,@codprod,@cant,@pn,@pb,@linea) ;";
            return _query;
        }

        public string UpdateEstado()
        {
            _query = "select service.fn_update_presupuestoservicioestado(@codpresupuesto, @mov) ; ";
            return _query;
        }
    }
}
