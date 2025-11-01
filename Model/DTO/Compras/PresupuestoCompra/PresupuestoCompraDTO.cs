namespace Model.DTO.Compras.PresupuestoCompra
{
    public class PresupuestoCompraDTO
    {
        public int codpresupuestocompra { get; set; }
        public string numpresupuestocompra { get; set; }
        public DateTime fechapresupuesto { get; set; }
        public string datoproveedor { get; set; }
        public string datopedido { get; set; }
        public decimal totalpresupuestocompra { get; set; }
        public string condicion { get; set; }
        public decimal cotizacion { get; set; }
        public string moneda { get; set; }
        public List<PresupuestoCompraDetDTO> detalle { get; set; }

    }

    public class PresupuestoCompraDetDTO
    {
        public string datoproducto { get; set; }
        public decimal cantidad { get; set; }
        public decimal totallinea { get; set; }
        public decimal preciobruto { get; set; }
        public decimal precioneto { get; set; }
        public string datoiva { get; set; }
        
    }
}