namespace Model.DTO.Ventas.Venta
{
    public class VentasListDTO
    {
        public int codventa { get; set; }
        public string datosventa { get; set; }
        public DateTime fechaventa { get; set; }
        public string datocliente { get; set; }
        public string desestmov { get; set; }
        public string datovendedor { get; set; }
        public string datosucursal { get; set; }
        public decimal? totalventa { get; set; }
        public string presupuestoventa { get; set; }
        public string moneda { get; set; }
        public string condicion { get; set; }
    }
}
