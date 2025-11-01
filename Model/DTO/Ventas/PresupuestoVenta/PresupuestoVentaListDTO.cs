namespace Model.DTO.Ventas.PresupuestoVenta
{
    public class PresupuestoVentaListDTO
    {
        public int codpresupuestoventa { get; set; }
        public DateTime fechapresupuestoventa { get; set; }
        public string numpresupuestoventa { get; set; }
        public string cliente { get; set; }
        public string vendedor { get; set; }
        public string datopedidoventa { get; set; }
        public string sucursal { get; set; }
        public string condicionpago { get; set; }
        public string desestmov { get; set; }
        public decimal totalpresupuestoventa { get; set; }
    }
}