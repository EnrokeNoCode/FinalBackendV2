namespace Model.DTO.Ventas.PedidoVenta
{
    public class PedidoVentaInsertDTO
    {
        public int codtipocomprobante { get; set; }
        public int codsucursal { get; set; }
        public int codestmov { get; set; }
        public DateTime fechapedidov { get; set; }
        public string numpedventa { get; set; }
        public int codvendedor { get; set; }
        public int codcliente { get; set; }
        public int codmoneda { get; set; }
        public decimal totalpedidov { get; set; }
        public decimal cotizacion1 { get; set; }
        public int codterminal { get; set; }
        public int ultimo { get; set; }
        public ICollection<PedidoVentaDetInsertDTO>? pedventadet { get; set; }
    }
    public class PedidoVentaDetInsertDTO
    {
        public int codproducto { get; set; }
        public decimal cantidad { get; set; }
        public decimal precioventa { get; set; }
    }

}