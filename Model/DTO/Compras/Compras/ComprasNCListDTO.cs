namespace Model.DTO.Compras.Compra
{
    public class ComprasNCListDTO
    {
        public int codcompra { get; set; }
        public string datocompra { get; set; }
        public int codmoneda { get; set; }
        public string nummoneda { get; set; }
        public decimal cotizacion { get; set; }
        //public List<ComprasDetNCListDTO> detalle { get; set; }
    }

    public class ComprasDetNCListDTO
    {
        public int codproducto  { get; set; }
        public string datoproducto { get; set; }
        public int codiva { get; set; }
        public string desiva { get; set; }
        public decimal disponible { get; set; }
        public decimal preciobruto { get; set; }
        public decimal precioneto { get; set; }
        public decimal costoultimo { get; set; }   
    }
}
