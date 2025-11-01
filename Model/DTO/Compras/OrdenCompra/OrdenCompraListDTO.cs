namespace Model.DTO.Compras.OrdenCompra
{
    public class OrdenCompraListDTO
    {
        public int codordenc { get; set; }                  
        public string numordencompra { get; set; }         
        public DateTime fechaorden { get; set; }            
        public string numtipocomprobante { get; set; }
        public string destipocomprobante { get; set; }
        public string nrodocprv { get; set; }
        public string proveedor { get; set; }
        public string empleado { get; set; }
        public string numestmov { get; set; }
        public string desestmov { get; set; }
        public string numsucursal { get; set; }
        public string dessucursal { get; set; }
        public decimal? totaliva { get; set; }
        public decimal? totalexento { get; set; }
        public decimal? totalordencompra { get; set; }    
        public string presupuestocompra { get; set; }            
        public string condicion { get; set; }               
    }
}
