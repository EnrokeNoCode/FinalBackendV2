using Persistence.SQL.Referencial;
using Persistence.SQL.Reporte.Referenciales;
using Service.Reportes.Referenciales;
using Persistence;
using Persistence.SQL.Acceso;
using Persistence.SQL.Compra;
using Persistence.SQL.Servicio;
using Persistence.SQL.Venta;
using Service.Acceso;
using Service.Compra;
using Service.Referencial;
using Service.Servicio;
using Service.Venta;
using Service.Reportes.Compras;
using Persistence.SQL.Reporte.Compras;
using Service.Reportes.Ventas;
using Persistence.SQL.Reporte.Ventas;
using Persistence.SQL;
using Service.Reportes.Servicios;
using Persistence.SQL.Reporte.Servicios;

namespace Utils
{
    public static class InyectorDependencias
    {
        public static IServiceCollection Infraestructura(this IServiceCollection services)
        {
            //Conexion a la BD
            services.AddScoped<DBConnector>();

            //Acceso
            services.AddScoped<UsuarioSQL>();

            services.AddScoped<UsuarioService>();

            //Referenciales
            services.AddScoped<CajaService>();
            services.AddScoped<SucursalService>();
            services.AddScoped<CobradorService>();
            services.AddScoped<ClienteService>();
            services.AddScoped<TerminalService>();
            services.AddScoped<EstadoMovimientoService>();
            services.AddScoped<MarcaService>();
            services.AddScoped<MonedaService>();
            services.AddScoped<MotivoAjusteService>();
            services.AddScoped<PartesVehiculoService>();
            services.AddScoped<TipoComprobanteService>();
            services.AddScoped<VendedorService>();
            services.AddScoped<ProductoService>();
            services.AddScoped<ProveedorService>();
            services.AddScoped<FormaCobroService>();
            services.AddScoped<BancoService>();
            services.AddScoped<TipoTarjetaService>();
            services.AddScoped<TemporalService>();

            services.AddScoped<CajaSQL>();
            services.AddScoped<SucursalSQL>();
            services.AddScoped<CobradorSQL>();
            services.AddScoped<ClienteSQL>();
            services.AddScoped<TerminalSQL>();
            services.AddScoped<EstadoMovimientoSQL>();
            services.AddScoped<MarcaSQL>();
            services.AddScoped<MonedaSQL>();
            services.AddScoped<MotivoAjusteSQL>();
            services.AddScoped<PartesVehiculoSQL>();
            services.AddScoped<TipoComprobanteSQL>();
            services.AddScoped<VendedorSQL>();
            services.AddScoped<ProductoSQL>();
            services.AddScoped<ProveedorSQL>();
            services.AddScoped<FormaCobroSQL>();
            services.AddScoped<BancoSQL>();
            services.AddScoped<TipoTarjetaSQL>();
            services.AddScoped<TemporalSQL>();

            //Compras
            services.AddScoped<PedidoCompraService>();
            services.AddScoped<PresupuestoCompraService>();
            services.AddScoped<OrdenCompraService>();
            services.AddScoped<NotaCreditoCompraService>();
            services.AddScoped<ComprasService>();
            services.AddScoped<AjustesService>();
            services.AddScoped<RemisionCompraService>();
            
            services.AddScoped<PedidosCompras_sql>();
            services.AddScoped<PresupuestoCompras_sql>();
            services.AddScoped<OrdenCompra_Sql>();
            services.AddScoped<Compras_Sql>();
            services.AddScoped<NotaCreditoCompra_Sql>();
            services.AddScoped<Ajustes_Sql>();
            services.AddScoped<RemisionCompra_Sql>();

            //Ventas
            services.AddScoped<PedidoVentaService>();
            services.AddScoped<PresupuestoVentaService>();
            services.AddScoped<VentasService>();
            services.AddScoped<NotaCreditoVentaService>();
            services.AddScoped<CobrosVentaService>();
            services.AddScoped<RemisionVentaService>();

            services.AddScoped<Ventas_sql>();
            services.AddScoped<PedidoVenta_sql>();
            services.AddScoped<PresupuestoVenta_sql>();
            services.AddScoped<NotaCreditoVenta_Sql>();
            services.AddScoped<CobroContado_sql>();
            services.AddScoped<RemisionVenta_Sql>();

            //Servicios
            services.AddScoped<RegistroVehiculoService>();
            services.AddScoped<DiagnosticoTecnicoService>();
            services.AddScoped<PresupuestoServicioService>();

            services.AddScoped<RegistroVehiculo_sql>();
            services.AddScoped<DiagnosticoTecnico_sql>();
            services.AddScoped<PresupuestoServicioSQL>();

            //Reportes
            services.AddScoped<CajaReporteService>();
            services.AddScoped<CompraReporteService>();
            services.AddScoped<VentaReporteService>();
            services.AddScoped<ProductoReporteService>();
            services.AddScoped<PresupuestoServicioReporteService>();

            services.AddScoped<CajaReporteSQL>();
            services.AddScoped<CompraReporteSQL>();
            services.AddScoped<VentaReporteSQL>();
            services.AddScoped<ProductoReporteSQL>();
            services.AddScoped<PresupuestoServicioReporteSQL>();

            return services;
        }
    }
}