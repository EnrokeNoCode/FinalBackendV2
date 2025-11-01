namespace Model.DTO
{
    public class PrecioVentaProductoListDTO
    {
        public int codproducto { get; set; }
        public string producto { get; set; }
        public decimal preciobruto { get; set; }
        public decimal precioneto { get; set; }
        public int codiva_ { get; set; }
    }
}