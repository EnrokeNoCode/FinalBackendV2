namespace Model.DTO.Compras.RemisionCompra
{
    public class RemisionCompraListDTO
    {
        public int codremisioncompra { get; set; }
        public string datosucursal { get; set; }
        public DateTime fecharemision { get; set; }
        public string numremisioncompra { get; set; }
        public string datoproveedor { get; set; }
        public string datocompra { get; set; }
        public DateTime fechallegada { get; set; }
        public decimal totalnotaremision { get; set; }
        public string estado { get; set; }
    }

}