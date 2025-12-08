namespace Model.DTO.Ventas.PedidoVenta
{
    public class PedidoVentaUpdateDTO
    {
        public int codpedidov { get; set; }
        public decimal totalpedidov { get; set; }
        public List<PedidoVentaDetUpdateDTO> pedventadet { get; set; }
    }

    public class PedidoVentaDetUpdateDTO
    {
        public int codproducto { get; set; }
        public decimal cantidad { get; set; }
        public decimal precioventa { get; set; }

    }
}


