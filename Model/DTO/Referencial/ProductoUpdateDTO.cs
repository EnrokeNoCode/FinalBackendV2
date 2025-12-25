namespace Model.DTO.Referencial
{
    public class ProductoUpdateDTO
    {
        public int codproducto { get; set; }
        public string codigobarra { get; set; }
        public string desproducto { get; set; }
        public int codproveedor { get; set; }
        public int codmarca { get; set; }
        public int codfamilia { get; set; }
        public int codrubro { get; set; }
        public int codiva { get; set; }
        public bool afectastock { get; set; }
        public decimal costoultimo { get; set; }
    }
}
