namespace Model.DTO
{
    public class ProductoInsertDTO
    {
        public string codigobarra { get; set; }
        public string desproducto { get; set; }
        public int codfamilia { get; set; }
        public int codmarca { get; set; }
        public int codrubro { get; set; }
        public int codunidadmedida { get; set; }
        public int codiva { get; set; }
        public int codproveedor { get; set; }
        public bool activo { get; set; }
        public bool afectastock { get; set; }

    }
}