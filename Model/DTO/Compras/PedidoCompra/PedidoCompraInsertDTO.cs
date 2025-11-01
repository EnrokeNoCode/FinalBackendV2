namespace Model.DTO.Compras.PedidoCompra
{
    public class PedidoCompraInsertDTO
    {
        public int codtipocomprobante { get; set; }
        public string numpedcompra { get; set; }
        public string fechapedcompra { get; set; }
        public int codestmov { get; set; }
        public int codempleado { get; set; }
        public int codsucursal { get; set; }
        public int codterminal { get; set; }
        public int ultimo { get; set; }
        public List<PedidoCompraDetInsertDto> pedcompradet { get; set; }
    }

    public class PedidoCompraDetInsertDto
    {
        public int codproducto { get; set; }
        public decimal cantidad { get; set; }
        public decimal costoultimo { get; set; }
    }

}