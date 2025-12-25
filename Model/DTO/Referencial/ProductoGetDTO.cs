namespace Model.DTO.Referencial
{
    public class ProductoGetDTO
    {
        public int codproducto { get; set; }
        public string codigobarra { get; set; }
        public string desproducto { get; set; }
        public int codproveedor { get; set; }
        public string datoproveedor { get; set; }
        public int codiva { get; set; }
        public string desiva { get; set; }
        public bool afectastock { get; set; }
        public bool activo { get; set; }
        public int codmarca { get; set; }
        public string desmarca { get; set; }
        public int codfamilia { get; set; }
        public string desfamilia { get; set; }
        public int codrubro { get; set; }
        public string desrubro { get; set; }
        public decimal costoultimo { get; set; }
    }
}
