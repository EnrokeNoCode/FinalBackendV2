namespace Model.Reportes.Ventas
{
    public class VentaListadoReporteDTO
    {
        public int codventa { get; set; }
        public string fechaventa { get; set; }
        public string numtipocomprobante { get; set; }
        public string numventa { get; set; }
        public string nrodoc { get; set; }
        public string cliente { get; set; }
        public string desestmov { get; set; }
        public string nummoneda { get; set; }
        public decimal cotizacion { get; set; }
        public string condicionpago { get; set; }
        public decimal totaliva { get; set; }
        public decimal totalexento { get; set; }
        public decimal totalventa { get; set; }

        public List<VentaDetListadoReporteDTO>? detalle { get; set; }
    }

    public class VentaDetListadoReporteDTO
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
