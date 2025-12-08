namespace Model.DTO.Ventas.PedidoVenta
{

    public class PedidoVentaDTO
    {
        public int codpedidov { get; set; }
        public DateTime fechapedventa { get; set; }
        public string numtipocomprobante { get; set; }
        public string numpedventa { get; set; }
        public string nrodoc { get; set; }
        public string cliente { get; set; }
        public string vendedor { get; set; }
        public string desestmov { get; set; }
        public string dessucursal { get; set; }
        public string nummoneda { get; set; }
        public decimal totalpedidov { get; set; }
        public ICollection<PedidoVentaDetDTO>? pedventadet { get; set; }
    }
    public class PedidoVentaDetDTO
    {
        public int codpedidov { get; set; }
        public int codproducto { get; set; }
        public string? datoproducto { get; set; }
        public decimal cantidad { get; set; }
        public decimal precioventa { get; set; }
        public int codiva { get; set; }
        public string? desiva { get; set; }
    }
}