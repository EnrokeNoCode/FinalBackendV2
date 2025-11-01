namespace Model.DTO.Compras.Compra
{
    public class ComprasDTO
    {
        public int codcompra { get; set; }
        public string numcompra { get; set; }
        public DateTime fechacompra { get; set; }
        public string datoproveedor { get; set; }
        public string datoordenc { get; set; }
        public decimal totalcompra { get; set; }
        public decimal totaliva { get; set; }
        public decimal totalexenta { get; set; }
        public string condicion { get; set; }
        public decimal cotizacion { get; set; }
        public string moneda { get; set; }
        public List<ComprasDetDTO> detalle { get; set; }
    }
    public class ComprasDetDTO
    {
        public string datoproducto { get; set; }
        public decimal cantidad { get; set; }
        public decimal totallinea { get; set; }
        public decimal preciobruto { get; set; }
        public decimal precioneto { get; set; }
        public string datoiva { get; set; }
        
    }
}