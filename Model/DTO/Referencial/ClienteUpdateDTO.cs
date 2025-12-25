namespace Model.DTO.Referencial
{
    public class ClienteUpdateDTO
    {
        public int codcliente { get; set; }
        public string nombre { get; set; }  
        public string apellido { get; set; }
        public string? direccion { get; set; }
        public string? nrotelef { get; set; }
        public int codciudad { get; set; }
        public int codlista { get; set; }
        public bool clientecredito { get; set; }
        public decimal? limitecredito { get; set; }

    }
}
