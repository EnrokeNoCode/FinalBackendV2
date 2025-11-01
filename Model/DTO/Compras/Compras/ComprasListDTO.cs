namespace Model.DTO.Compras.Compra
{
    public class CompraListDTO
    {
        public int codcompra { get; set; }
        public string numcompra { get; set; }
        public DateTime fechacompra { get; set; }
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
        public decimal? totalcompra { get; set; }    
        public string ordencompra { get; set; }            
        public string condicion { get; set; }   
    }
}