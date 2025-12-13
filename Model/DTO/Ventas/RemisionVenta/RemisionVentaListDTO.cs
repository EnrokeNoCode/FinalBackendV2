namespace Model.DTO.Ventas.RemisionVenta
{
    public class RemisionVentaListDTO
    {
        public int codremisionventa { get; set; }
        public string datosucursal { get; set; }
        public DateTime fecharemision { get; set; }
        public string numremisionventa { get; set; }
        public string datocliente { get; set; }
        public string datoventa { get; set; }
        public DateTime fechallegada { get; set; }
        public decimal totalnotaremision { get; set; }
        public string estado { get; set; }
    }

}