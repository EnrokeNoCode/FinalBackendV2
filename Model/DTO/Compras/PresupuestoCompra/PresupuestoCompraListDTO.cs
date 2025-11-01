namespace Model.DTO.Compras.PresupuestoCompra
{
    public class PresupuestoCompraListDTO
    {
        public int codpresupuestocompra { get; set; }
        public DateTime fechapresupuesto { get; set; }
        public string numtipocomprobante { get; set; }
        public string destipocomprobante { get; set; }
        public string numpresupuestocompra { get; set; }
        public string nrodocprv { get; set; }
        public string proveedor { get; set; }
        public string empleado { get; set; }
        public string numestmov { get; set; }
        public string desestmov { get; set; }
        public string numsucursal { get; set; }
        public string dessucursal { get; set; }
        public decimal? totaliva { get; set; }
        public decimal? totalexento { get; set; }
        public decimal? totalpresupuestocompra { get; set; }
        public string pedidocompra { get; set; }
    }
}
