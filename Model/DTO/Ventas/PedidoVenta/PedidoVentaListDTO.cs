namespace Model.DTO.Ventas.PedidoVenta
{
    public class PedidoVentaListDTO
    {
        public int codpedidov { get; set; }
        public DateTime fechapedidov { get; set; }
        public string numpedventa { get; set; }
        public string cliente { get; set; }
        public string vendedor { get; set; }
        public string desestmov { get; set; }
        public string sucursal { get; set; }
        public decimal totalpedidov { get; set; }
    }
}