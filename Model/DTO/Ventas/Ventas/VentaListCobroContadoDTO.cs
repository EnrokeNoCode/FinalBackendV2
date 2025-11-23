namespace Model.DTO
{
    public class VentaListCobroContadoDTO
    {
        public int codventa { get; set; }
        public string numventa { get; set; }
        public decimal totalventa { get; set; }
        public int codmoneda { get; set; }
        public string nummoneda { get; set; }
        public decimal cotizacion { get; set; }
    }
}
