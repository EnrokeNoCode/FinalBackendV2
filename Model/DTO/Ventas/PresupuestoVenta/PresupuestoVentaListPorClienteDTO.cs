namespace Model.DTO.Ventas.PresupuestoVenta
{
    public class PresupuestoVentaListPorClienteDTO
    {
        public int codpresupuestoventa {  get; set; }
        public DateTime fechapresupuestoventa { get; set; }
        public string numpresupuestoventa { get; set; }
        public string cliente { get; set; }
        public int codvendedor { get; set; }
        public string vendedor { get; set; }
        public int codcondicion { get; set; }
        public string condicionpago { get; set; }
        public decimal totalpresupuestoventa { get; set; }
    }
}
