namespace Model.DTO.Compras.RemisionCompra
{
    public class RemisionCompraDTO
    {
        public string numtipocomprobante { get; set; }
        public string numremisioncompra { get; set; }
        public DateTime fecharemision { get; set; }
        public DateTime fechallegada { get; set; }
        public string datoproveedor { get; set; }
        public string datocompra { get; set; }
        public string ruc_transportista { get; set; }
        public string razonsocial_transportista { get; set; }
        public string nrochapa_vehiculo { get; set; }
        public string marca_vehiculo { get; set; }
        public string modelo_vehiculo { get; set; }
        public string nombreapellido_chofer { get; set; }
        public string nrodoc_chofer { get; set; }
        public string nrotelefono_chofer { get; set; }
        public decimal totalremision { get; set; }
        public string datoempleado { get; set; }
        public string datosucursal { get; set; }
        public ICollection<RemisionCompraDetDTO>? remisiondet { get; set; }
    }
    public class RemisionCompraDetDTO
    {
        public string codigobarra { get; set; }
        public string desproducto { get; set; }
        public decimal cantidad { get; set; }
        public decimal costo { get; set; }

    }
}