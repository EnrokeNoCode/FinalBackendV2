namespace Model.DTO.Referencial
{
    public class ProveedorGetDTO
    {
        public int codproveedor { get; set; }
        public string nrodocprv { get; set; }
        public string razonsocial { get; set; }
        public bool activo { get; set; }
        public int codtipoidnt { get; set; }
        public string datotipoidnt { get; set; }
        public string? direccionprv { get; set; }
        public string? nrotelefprv { get; set; }
        public string? contacto { get; set; }
        public int codciudad { get; set; }
        public string datociudad { get; set; }
        public string nrotimbrado { get; set; }
        public DateOnly fechaventimbrado { get; set; }
    }
}
