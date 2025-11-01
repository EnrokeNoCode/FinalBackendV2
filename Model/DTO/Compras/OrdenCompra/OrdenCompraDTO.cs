namespace Model.DTO.Compras.OrdenCompra
{
    public class OrdenCompraDTO
    {
        public int codordenc { get; set; }
        public string numordencompra { get; set; }
        public DateTime fechaorden { get; set; }
        public string datoproveedor { get; set; }
        public string datopresupuesto { get; set; }
        public decimal totalordencompra { get; set; }
        public decimal totaliva { get; set; }
        public decimal totalexenta { get; set; }
        public string condicion { get; set; }
        public decimal cotizacion { get; set; }
        public string moneda { get; set; }
        public List<OrdenCompraDetDTO> detalle { get; set; }

    }

    public class OrdenCompraDetDTO
    {
        public string datoproducto { get; set; }
        public decimal cantidad { get; set; }
        public decimal totallinea { get; set; }
        public decimal preciobruto { get; set; }
        public decimal precioneto { get; set; }
        public string datoiva { get; set; }
        
    }
}