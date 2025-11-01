namespace Model.DTO.Compras.NotaCreditoCompra
{
    public class NotaCreditoDTO
    {
        public string nrotimbrado { get; set; }
        public DateTime fechavalidez { get; set; }
        public DateTime fechanotacredito { get; set; }
        public string datonotacredito { get; set; }
        public decimal totaldevolucion { get; set; }
        public string datoproveedor { get; set; }
        public string datocompra { get; set; }
        public ICollection<NotaCreditoDetDTO>? notacreditodet { get; set; }
    }
    public class NotaCreditoDetDTO
    {
        public string datoproducto { get; set; }
        public string desiva { get; set; }
        public decimal preciobruto { get; set; }
        public decimal precioneto { get; set; }
        public decimal cantidaddev { get; set; }
        public decimal totallinea { get; set; }
    }
}