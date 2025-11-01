namespace Model.DTO
{
    public class ClienteListDTO
    {
        public int codcliente { get; set; }
        public string nrodoc { get; set; }
        public string nombre_apellido { get; set; }
        public string listaprecio { get; set; }
        public string clientecredito { get; set; }
        public decimal limitecredito { get; set; }
        public string activo { get; set; }
        public DateTime? fecha { get; set; }
    }
}