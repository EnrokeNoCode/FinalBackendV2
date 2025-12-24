namespace Model.DTO.Ventas.NotaCreditoVenta
{
    public class NotaCreditoVentaListDTO
    {
        public int codnotacredito { get; set; }
        public int codventa {  get; set; }
        public DateTime fechanotacredito { get; set; }
        public string nronotacredito { get; set; }
        public string datoventa { get; set; }
        public string datocliente { get; set; } 
        public string nummoneda { get; set; }   
        public string dessucursal { get; set; }
        public string desestmov { get;set; }
        public decimal totaldevolucion { get; set; }
    }
}
