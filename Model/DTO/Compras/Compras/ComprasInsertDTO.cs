namespace Model.DTO.Compras.Compra
{
    public class ComprasInsertDTO
    {
        public int codtipocomprobante { get; set; }
        public int codterminal { get; set; }
        public int ultimo { get; set; }
        public string numcompra { get; set; }
        public string fechacompra { get; set; }
        public int codproveedor { get; set; }
        public string finvalideztimbrado { get; set; }
        public string nrotimbrado { get; set; }
        public int codsucursal { get; set; }
        public int codempleado { get; set; }
        public int codestmov { get; set; }
        public int condicionpago { get; set; }
        public int cant_cuotas { get; set; }
        public int codmoneda { get; set; }
        public decimal cotizacion { get; set; }
        public string observacion { get; set; }
        public decimal totaliva { get; set; }
        public decimal totalexento { get; set; }
        public decimal totaldescuento { get; set; }
        public decimal totalgravada { get; set; }
        public decimal totalcompra { get; set; }
        public int? codordenc { get; set; }
        public List<ComprasDetInsertDTO> comprasdet { get; set; }
    }

    public class ComprasDetInsertDTO
    {
        public int codproducto { get; set; }
        public int coddepsuc { get; set; }
        public int codiva { get; set; }
        public decimal cantidad { get; set; }
        public decimal descuento { get; set; }
        public decimal preciobruto { get; set; }
        public decimal precioneto { get; set; }
        public decimal cotizacion1 { get; set; }
        public decimal costoultimo { get; set; }
    }
}