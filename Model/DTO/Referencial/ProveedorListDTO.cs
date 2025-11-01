namespace Model.DTO
{
    public class ProveedorListDTO
    {
        public int codproveedor { get; set; }
        public string proveedor { get; set; }
        public string activo { get; set; }
        public string datofacturacion { get; set; }
        public DateTime? fecha { get; set; }
        public string datocontacto { get; set; }

    }
}