namespace Model.DTO.Compras.PedidoCompra
{
    public class PedidoCompraListDTO
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
    }
}