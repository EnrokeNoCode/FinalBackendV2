namespace Model.DTO.Ventas.PresupuestoVenta
{
    public class PresupuestoVentaDTO
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
        //public List<PresupuestoVentaDetDTO> presdetven { get; set; }
    }

    /*
    public class PresupuestoVentaDetDTO
    {
        public int codpresupuestoventa { get; set; }
        public int codproducto { get; set; }
        public string producto { get; set; }
        public int codiva { get; set; }
        public string iva { get; set; }
        public decimal cantidad { get; set; }
        public decimal preciobruto { get; set; }
        public decimal totallinea { get; set; }
    }*/
}
