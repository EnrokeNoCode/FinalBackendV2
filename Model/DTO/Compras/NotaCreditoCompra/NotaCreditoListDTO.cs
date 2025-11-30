namespace Model.DTO.Compras.NotaCreditoCompra
{
    public class NotaCreditoListDTO
    {
        public int codnotacredito { get; set; }
        public int codcompra {  get; set; }
        public DateTime fechanotacredito { get; set; }
        public string nronotacredito { get; set; }
        public string datocompra { get; set; }
        public string datoproveedor { get; set; } 
        public string nummoneda { get; set; }   
        public string dessucursal { get; set; }
        public string estado { get; set; }
        public decimal totaldevolucion { get; set; }
    }
}
