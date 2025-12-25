namespace Model.DTO.Referencial
{
    public class ProveedorInsertDTO
    {
        public string nrodocprv { get; set; }
        public string razonsocial { get; set; }
        public bool activo { get; set; }
        public int codtipoidnt { get; set; }
        public string? direccionprv { get; set; }
        public string? nrotelefprv { get; set; }
        public string? contacto { get; set; }
        public int codciudad { get; set; }
        public string nrotimbrado { get; set; }
        public DateOnly fechaventimbrado { get; set; }
    }
}
