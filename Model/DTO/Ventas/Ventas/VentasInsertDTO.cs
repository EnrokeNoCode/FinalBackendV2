namespace Model.DTO.Ventas.Venta
{
    public class VentasInsertDTO
    {
        public int codtipocomprobante { get; set; }
        public string numventa { get; set; }
        public string fechaventa { get; set; }
        public int codcliente { get; set; }
        public string finvalideztimbrado { get; set; }
        public string nrotimbrado { get; set; }
        public int codsucursal { get; set; }
        public int codvendedor { get; set; }
        public int codestmov { get; set; }
        public int condicionpago { get; set; }
        public int codmoneda { get; set; }
        public decimal cotizacion { get; set; }
        public string observacion { get; set; }
        public decimal totaliva { get; set; }
        public decimal totalexento { get; set; }
        public decimal totaldescuento { get; set; }
        public decimal totalgravada { get; set; }
        public decimal totalventa { get; set; }
        public int? codpresupuestoventa { get; set; }
        public int codterminal { get; set; }
        public int ultimo { get; set; }
        public int cant_cuotas { get; set; }
        public ICollection<VentasDetInsertDTO>? ventasdet { get; set; }
    }

    public class VentasDetInsertDTO
    {
        public int codproducto { get; set; }
        public int codiva { get; set; }
        public decimal cantidad { get; set; }
        public decimal descuento { get; set; }
        public decimal preciobruto { get; set; }
        public decimal precioneto { get; set; }
        public decimal cotizacion1 { get; set; }
        public decimal costoultimo { get; set; }
    }
}
