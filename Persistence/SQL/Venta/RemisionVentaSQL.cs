namespace Persistence.SQL.Venta
{
    public class RemisionVenta_Sql
    {
        public string SelectList(int pageSize, int offset)
        {
            return $@"select rc.codremisionventa , rc.fecharemision, rc.fechallegada, tc.numtipocomprobante || '- ' || rc.numremisionventa  as numremisionventa,
                    tc2.numtipocomprobante || '- ' || v.numventa  as datoventa, c.nrodoc  || '- ' || c.nombre || ', ' || c.apellido  as datocliente,
                    s.numsucursal || '- ' || s.dessucursal as datosucursal , em.desestmov as estado
                    from sales.remisionventa rc
                    inner join sales.ventas v on rc.codventa= v.codventa
                    inner join referential.cliente c on rc.codcliente = c.codcliente 
                    inner join referential.sucursal s on rc.codsucursal = s.codsucursal
                    inner join referential.tipocomprobante tc on rc.codtipocomprobante = tc.codtipocomprobante 
                    inner join referential.tipocomprobante tc2 on v.codtipocomprobante = tc2.codtipocomprobante 
                    inner join referential.estadomovimiento em on rc.codestmov = em.codestmov 
                    order by rc.fecharemision, rc.codremisionventa  desc
                    limit {pageSize} offset {offset}";
        }

        public string Insert()
        {
            return @"
                SELECT sales.fn_insert_remisionventa(
                    @codventa,
                    @codsucursal,
                    @codtipocomprobante,
                    @codestmov,
                    @numremisionventa,
                    @fecharemision::timestamp,
                    @fecharegistro::timestamp,
                    @codcliente,
                    @codempleado,
                    @rucransportista,
                    @razonsocialtransportista,
                    @chapavehiculo,
                    @marcavehiculo,
                    @modelovehiculo,
                    @nrodocchofer,
                    @nombreapellidochofer,
                    @nrotelefonochofer,
                    @detalles,
                    @codterminal
                );";
        }


        public string Select(int option)
        {
            string sentence = "";

            switch (option)
            {
                case 1:
                    sentence = @"select tc.numtipocomprobante , rc.numremisionventa  , rc.fecharemision, rc.fechallegada,
                                c.nrodoc  || '- ' || c.nombre || ', ' || c.apellido  as datocliente,
                                tc.numtipocomprobante || '- ' || v.numventa  as datoventa, 
                                rc.ruc_transportista , rc.razonsocial_transportista , rc.nrochapa_vehiculo , rc.marca_vehiculo , rc.modelo_vehiculo , 
                                rc.nombreapellido_chofer, rc.nrodoc_chofer , rc.nrotelefono_chofer, 
                                (select sum(rvd.costo) * sum(rvd.cantidad) from sales.remisionventa_det rvd where rvd.codremisionventa = @codremisionventa) as totalremision,
                                e.nombre_emp || ' ' || e.apellido_emp as datoempleado, s.numsucursal || '- ' || s.dessucursal as datosucursal
                                from sales.remisionventa rc
                                inner join sales.ventas v on rc.codventa= v.codventa
                                inner join referential.tipocomprobante tr on rc.codtipocomprobante = tr.codtipocomprobante 
                                inner join referential.tipocomprobante tc on v.codtipocomprobante = tc.codtipocomprobante 
                                inner join referential.sucursal s on rc.codsucursal = s.codsucursal 
                                inner join referential.cliente c on rc.codcliente = c.codcliente 
                                inner join referential.empleado e on rc.codempleado = e.codempleado 
                                where rc.codremisionventa = @codremisionventa;";
                    break;
            }

            return sentence;    
        }
        public string SelectWithDetails()
        {
            return @"select p.codigobarra, p.desproducto, rd.cantidad, rd.costo 
                    from sales.remisionventa_det rd 
                    inner join referential.producto p on rd.codproducto = p.codproducto 
                    where rd.codremisionventa = @codremisionventa;";
        }
        public string Update()
        {
            return @"select sales.fn_update_remisionventaestado(@codremisionventa)";
        }

    }
        
}


