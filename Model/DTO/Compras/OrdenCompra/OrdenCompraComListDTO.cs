namespace Model.DTO.Compras.OrdenCompra
{
    public class OrdenCompraComListDTO
    {
        public int codordenc { get; set; }
        public int codproveedor { get; set; }
        public DateTime fechaorden { get; set; }
        public string datoordencompra { get; set; }
        public decimal totalordencompra { get; set; }
        public List<OrdenCompraComListDetDTO> ordedetcom { get; set; }
    }

    public class OrdenCompraComListDetDTO
    {
        public int codordenc { get; set; }
        public int codproducto { get; set; }
        public string producto { get; set; }
        public int codiva { get; set; }
        public string iva { get; set; }
        public decimal cantidad { get; set; }
        public decimal preciobruto { get; set; }
        public decimal totallinea { get; set; }
    }
}
