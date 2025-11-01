using Persistence;
using Persistence.SQL.Acceso;
using Persistence.SQL.Compra;
using Persistence.SQL.Referencial;
using Persistence.SQL.Servicio;
using Persistence.SQL.Venta;
using Service.Acceso;
using Service.Compra;
using Service.Referencial;
using Service.Servicio;
using Service.Venta;

namespace Utils
{
    public static class InyectorDependencias
    {
        public static IServiceCollection Infraestructura(this IServiceCollection services)
        {
            //Conexion a la BD
            services.AddSingleton<DBConnector>();

            //Acceso
            services.AddSingleton<UsuarioSQL>();

            services.AddSingleton<UsuarioService>();

            //Referenciales
            services.AddSingleton<CajaService>();
            services.AddSingleton<SucursalService>();
            
            services.AddSingleton<CajaSQL>();
            services.AddSingleton<SucursalSQL>();
            

            //Compras
            services.AddSingleton<PedidoCompraService>();
            services.AddSingleton<PresupuestoCompraService>();
            services.AddSingleton<OrdenCompraService>();
            services.AddSingleton<NotaCreditoCompraService>();
            services.AddSingleton<ComprasService>();
            services.AddSingleton<AjustesService>();
            
            services.AddSingleton<PedidosCompras_sql>();
            services.AddSingleton<PresupuestoCompras_sql>();
            services.AddSingleton<OrdenCompra_Sql>();
            services.AddSingleton<Compras_Sql>();
            services.AddSingleton<NotaCreditoCompra_Sql>();
            services.AddSingleton<Ajustes_Sql>();

            //Ventas
            services.AddScoped<PedidoVentaService>();
            services.AddScoped<PresupuestoVentaService>();
            services.AddScoped<VentasService>();
            services.AddSingleton<NotaCreditoVentaService>();

            services.AddSingleton<Ventas_sql>();
            services.AddSingleton<PedidoVenta_sql>();
            services.AddSingleton<PresupuestoVenta_sql>();
            services.AddSingleton<NotaCreditoVenta_Sql>();

            //Servicios
            services.AddSingleton<RegistroVehiculoService>();
            services.AddSingleton<DiagnosticoTecnicoService>();

            services.AddSingleton<RegistroVehiculo_sql>();
            services.AddSingleton<DiagnosticoTecnico_sql>();

            return services;
        }
    }
}