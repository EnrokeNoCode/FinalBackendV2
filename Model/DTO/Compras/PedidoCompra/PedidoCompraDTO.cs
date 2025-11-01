namespace Model.DTO.Compras.PedidoCompra
{

    public class PedidoCompraDTO
    {
        public int codpedcompra { get; set; }
        public DateTime fechapedcompra { get; set; }
        public string numtipocomprobante { get; set; }
        public string destipocomprobante { get; set; }
        public string numpedcompra { get; set; }
        public string empleado { get; set; }
        public string numestmov { get; set; }
        public string desestmov { get; set; }
        public string numsucursal { get; set; }
        public string dessucursal { get; set; } 
        
        public ICollection<PedidoCompraDetDTO>? pedcompradet { get; set; }
    }
    public class PedidoCompraDetDTO
    {
        public int codpedcompra { get; set; }
        public int codproducto { get; set; }
        public string? codigobarra { get; set; }
        public string? desproducto { get; set; }
        public decimal cantidad { get; set; }
        public decimal costoultimo { get; set; }
    }
}