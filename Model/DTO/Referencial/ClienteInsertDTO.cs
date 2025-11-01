namespace Model.DTO
{
    public class ClienteInsertDTO
    {
        public string nrodoc { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public bool activo { get; set; }
        public DateTime? fechaalta { get; set; }
        public DateTime? fechabaja { get; set; }
        public int codtipoidnt { get; set; }
        public string? direccion { get; set; }
        public string? nrotelef { get; set; }
        public int codciudad { get; set; }
        public int codlista { get; set; }
        public bool clientecredito { get; set; }
        public decimal? limitecredito { get; set; }
    }
}