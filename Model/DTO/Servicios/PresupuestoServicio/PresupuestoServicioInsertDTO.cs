
namespace Model.DTO.Servicios.PresupuestoServicio
{
    public class PresupuestoServicioInsertDTO
    {
        public int codcliente { get; set; }
        public int codvehiculo { get; set; }
        public int coddiagnostico { get; set; }
        public int codtipocomprobante { get; set; }
        public int codestmov { get; set; }
        public string nropresupuesto { get; set; }
        public DateTime fechapresupuesto { get; set; }
        public int codsucursal { get; set; }
        public int codempleado { get; set; }
        public decimal totaliva { get; set; }
        public decimal totalexenta { get; set; }
        public decimal totalpresupuesto { get; set; }
        public ICollection<PresupuestoServicioDetServicioInsertDTO>? detalleservicio { get; set; }
        public ICollection<PresupuestoServicioDetRepuestoInsertDTO>? detallerepuesto { get; set; }
    }

    public class PresupuestoServicioDetServicioInsertDTO
    {
        public int codtiposervicio { get; set; }
        public string observacion { get; set; }
        public decimal precioneto { get; set; }
        public decimal preciobruto { get; set; }
    }

    public class PresupuestoServicioDetRepuestoInsertDTO
    {
        public int codproducto { get; set; }
        public decimal cantidad { get; set; }
        public decimal precioneto { get; set; }
        public decimal preciobruto { get; set; }
    }
}
