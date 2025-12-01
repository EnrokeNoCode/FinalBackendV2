namespace Persistence.SQL.Compra
{
    public class RemisionCompra_Sql
    {
        public string SelectList(int pageSize, int offset)
        {
            return $@"select rc.codremisioncompra, rc.fecharemision, rc.fechallegada, tc.numtipocomprobante || '- ' || rc.numremisioncompra as numremisioncompra,
                    tc2.numtipocomprobante || '- ' || c.numcompra as datocompra, prv.nrodocprv || '- ' || prv.razonsocial as datoproveedor,
                    s.numsucursal || '- ' || s.dessucursal as datosucursal , em.desestmov as estado
                    from purchase.remisioncompra rc
                    inner join purchase.compras c on rc.codcompra= c.codcompra
                    inner join referential.proveedor prv on rc.codproveedor = prv.codproveedor 
                    inner join referential.sucursal s on rc.codsucursal = s.codsucursal
                    inner join referential.tipocomprobante tc on rc.codtipocomprobante = tc.codtipocomprobante 
                    inner join referential.tipocomprobante tc2 on c.codtipocomprobante = tc2.codtipocomprobante 
                    inner join referential.estadomovimiento em on rc.codestmov = em.codestmov 
                    where rc.codsucursal = @codsucursal
                    order by rc.fecharemision, rc.codremisioncompra desc
                    limit {pageSize} offset {offset}";
        }

        public string Insert()
        {
            return @"
                SELECT purchase.fn_insert_remisioncompra(
                    @codcompra,
                    @codsucursal,
                    @codtipocomprobante,
                    @codestmov,
                    @numremisioncompra,
                    @fecharemision::timestamp,
                    @fecharegistro::timestamp,
                    @codproveedor,
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
                    sentence = @"select tc.numtipocomprobante , rc.numremisioncompra , rc.fecharemision, rc.fechallegada,
                                p.nrodocprv || '- ' || p.razonsocial as datoproveedor,
                                tc.numtipocomprobante || '- ' || c.numcompra as datocompra, 
                                rc.ruc_transportista , rc.razonsocial_transportista , rc.nrochapa_vehiculo , rc.marca_vehiculo , rc.modelo_vehiculo , 
                                rc.nombreapellido_chofer, rc.nrodoc_chofer , rc.nrotelefono_chofer, 
                                (select sum(rcd.costo) * sum(rcd.cantidad) from purchase.remisioncompra_det rcd where rcd.codremisioncompra = @codremisioncompra) as totalremision,
                                e.nombre_emp || ' ' || e.apellido_emp as datoempleado, s.numsucursal || '- ' || s.dessucursal as datosucursal
                                from purchase.remisioncompra rc
                                inner join purchase.compras c on rc.codcompra = c.codcompra 
                                inner join referential.tipocomprobante tr on rc.codtipocomprobante = tr.codtipocomprobante 
                                inner join referential.tipocomprobante tc on c.codtipocomprobante = tc.codtipocomprobante 
                                inner join referential.sucursal s on rc.codsucursal = s.codsucursal 
                                inner join referential.proveedor p on rc.codproveedor = p.codproveedor 
                                inner join referential.empleado e on rc.codempleado = e.codempleado 
                                where rc.codremisioncompra = @codremisioncompra;";
                    break;
            }

            return sentence;    
        }
        public string SelectWithDetails()
        {
            return @"select p.codigobarra, p.desproducto, rd.cantidad, rd.costo 
                    from purchase.remisioncompra_det rd 
                    inner join referential.producto p on rd.codproducto = p.codproducto 
                    where rd.codremisioncompra = @codremisioncompra;";
        }
        public string Update()
        {
            return @"select purchase.fn_update_remisioncompraestado(@codremisioncompra)";
        }

    }
        
}


