namespace Model.DTO.Ventas.PresupuestoVenta
{
    public class PresupuestoVentaInsertDTO
    {
        public int codtipocomprobante { get; set; }
        public int codsucursal { get; set; }
        public int codvendedor { get; set; }
        public int codcliente { get; set; }
        public DateTime fechapresupuestoventa { get; set; }
        public string numpresupuestoventa { get; set; }
        public int? codpedidov { get; set; }
        public string observacion { get; set; }
        public int diaven { get; set; }
        public short condicionpago { get; set; }
        public int codmoneda { get; set; }
        public decimal cotizacion1 { get; set; }
        public int codestmov { get; set; }
        public decimal totaliva { get; set; }
        public decimal totaldescuento { get; set; }
        public decimal totalexento { get; set; }
        public decimal totalgravada { get; set; }
        public decimal totalpresupuestoventa { get; set; }
        public int codterminal{ get; set; }
        public int ultimo { get; set; }
        public ICollection<PresupuestoVentaDetInsertDTO>? presventadet { get; set; }
    }
    
    public class PresupuestoVentaDetInsertDTO
    {
        public int codproducto { get; set; }
        public decimal precioneto { get; set; }
        public decimal preciobruto { get; set; }
        public decimal cantidad { get; set; }
        public decimal costoultimo { get; set; }
        public int codiva { get; set; }
    }
}