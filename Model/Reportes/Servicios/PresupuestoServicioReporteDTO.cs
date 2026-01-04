namespace Model.Reportes.Servicios
{
    public class PresupuestoServicioReporteDTO
    {
        public int codpresupuesto { get; set; }
        public string fechapresupuesto { get; set; }
        public string nropresupuesto { get; set; }
        public string cliente { get; set; }
        public string vehiculo { get; set; }
        public string diagnostico { get; set; }
        public decimal totalpresupuesto { get; set; }
        public ICollection<PresupuestoServicioDetServicioReporteDTO>? detalleservicio { get; set; }
        public ICollection<PresupuestoServicioDetRepuestoReporteDTO>? detallerepuesto { get; set; }
    }

    public class PresupuestoServicioDetServicioReporteDTO
    {
        public string codigobarra { get; set; }
        public string desproducto { get; set; }
        public decimal cantidad { get; set; }
        public decimal precioneto { get; set; }
        public decimal preciobruto { get; set; }
        public decimal totallinea { get; set; }
    }

    public class PresupuestoServicioDetRepuestoReporteDTO
    {
        public string codigotiposervicio { get; set; }
        public string destiposervicio { get; set; }
        public string observacion { get; set; }
        public decimal precioneto { get; set; }
        public decimal preciobruto { get; set; }
    }
}
