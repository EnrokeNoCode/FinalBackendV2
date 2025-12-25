namespace Api.POS.Model.DTO.Referencial
{
    public class ClienteGetDTO
    {
        public int codcliente { get; set; }
        public string nrodoc { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public bool activo { get; set; }
        public int codtipoidnt { get; set; }
        public string datotipoidnt { get; set; }
        public string? direccion { get; set; }
        public string? nrotelef { get; set; }
        public int codciudad { get; set; }
        public string datociudad { get; set; }
        public int codlista { get; set; }
        public string datolista { get; set; }
        public bool clientecredito { get; set; }
        public decimal? limitecredito { get; set; }
    }
}
