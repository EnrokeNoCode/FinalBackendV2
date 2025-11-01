namespace Model.DTO.Ventas.NotaCreditoVenta
{
    public class NotaCreditoVentaDTO
    {
        public string nrotimbrado { get; set; }
        public DateTime fechavalidez { get; set; }
        public DateTime fechanotacredito { get; set; }
        public string datonotacredito { get; set; }
        public decimal totaldevolucion { get; set; }
        public string datocliente { get; set; }
        public string datoventa { get; set; }
        public ICollection<NotaCreditoVentaDetDTO>? notacreditodet { get; set; }
    }
    public class NotaCreditoVentaDetDTO
    {
        public string datoproducto { get; set; }
        public string desiva { get; set; }
        public decimal preciobruto { get; set; }
        public decimal precioneto { get; set; }
        public decimal cantidaddev { get; set; }
        public decimal totallinea { get; set; }
    }
}