namespace Model.Reportes.Compras
{
    public class CompraListadoReporteDTO
    {
        public int codcompra { get; set; }
        public string fechacompra { get; set; }
        public string numtipocomprobante { get; set; }
        public string numcompra { get; set; }
        public string nrodocprv { get; set; }
        public string razonsocial { get; set; }
        public string desestmov { get; set; }
        public string nummoneda { get; set; }
        public decimal cotizacion { get; set; }
        public string condicionpago { get; set; }
        public decimal totaliva { get; set; }
        public decimal totalexento { get; set; }
        public decimal totalcompra { get; set; }

        public List<CompraDetListadoReporteDTO>? detalle { get; set; }
    }

    public class CompraDetListadoReporteDTO
    {
        public string codigobarra { get; set; }
        public string desproducto { get; set; }
        public string desiva { get; set; }
        public decimal cantidad { get; set; }
        public decimal preciobruto { get; set; }
        public decimal precioneto { get; set; }
        public decimal totallinea { get; set; }
    }
}
