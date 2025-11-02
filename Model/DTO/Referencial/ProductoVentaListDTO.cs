namespace Model.DTO
{
    public class ProductoVentaListDTO
    {
        public int codproducto { get; set; }
        public string datoproducto { get; set; }
        public decimal cantidad { get; set; }
        public decimal preciobruto { get; set; }
        public decimal precioneto { get; set; }
        public string desiva { get; set; }
        public int codiva { get; set; }
    }
}