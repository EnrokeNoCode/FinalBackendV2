namespace Persistence.SQL.Reporte.Servicios
{
    public class PresupuestoServicioReporteSQL
    {
        private string _query = "";


        public string SelectPresupuestoCab()
        {
            _query = @"select ps.codpresupuesto, TO_CHAR(ps.fechapresupuesto, 'DD/MM/YYYY HH:MM') as fechapresupuesto, 
                        tc1.numtipocomprobante || '- ' || ps.nropresupuesto as nropresupuesto,
                        c.nrodoc || '- ' || c.nombre  || ', ' ||c.apellido  as cliente,
                        'Nro. Chapa: ' || v.nrochapa || ' |Marca: ' || m.desmarca  || ' |Modelo: ' || v.modelo as vehiculo,
                        tc2.numtipocomprobante || '- ' || dt.nrodiagnostico as diagnostico , ps.totalpresupuesto 
                        from service.presupuestoservicio ps
                        inner join service.diagnosticotecnico dt on ps.coddiagnostico = dt.coddiagnostico 
                        inner join referential.cliente c on ps.codcliente= c.codcliente 
                        inner join referential.vehiculo v on ps.codvehiculo = v.codvehiculo 
                        inner join referential.marca m on v.codmarca = m.codmarca 
                        inner join referential.tipocomprobante tc1 on ps.codtipocomprobante = tc1.codtipocomprobante 
                        inner join referential.tipocomprobante tc2 on dt.codtipocomprobante = tc2.codtipocomprobante
                        where ps.codpresupuesto = @codpresupuesto  ;";
            return _query; 
        }

        public string SelectPresupuestoDetServicio()
        {
            _query = @"select pr.codigobarra, pr.desproducto, pdr.cantidad, pdr.precioneto, pdr.preciobruto, (pdr.preciobruto * pdr.cantidad) as totallinea 
                        from service.presupuestoservicio_detrepuesto pdr
                        inner join referential.producto pr on pdr.codproducto = pr.codproducto 
                        where pdr.codpresupuesto = @codpresupuesto order by pdr.lineadetalle ;";
            return _query;
        }

        public string SelectPresupuestoDetRepuesto()
        {
            _query = @"select ts.codigotiposervicio , ts.destiposervicio , pds.observacion , pds.precioneto, pds.preciobruto 
                        from service.presupuestoservicio_detservicio pds
                        inner join referential.tiposervicio ts on pds.codtiposervicio = ts.codtiposervicio
                        where pds.codpresupuesto = @codpresupuesto order by pds.lineadetalle ;";
            return _query;
        }
    }
}
