namespace Model.DTO.Ventas.RemisionVenta
{
    public class RemisionVentaInsertDTO
    {
        public int codventa { get; set; }
        public int codsucursal { get; set; }
        public int codtipocomprobante { get; set; }
        public int codestmov { get; set; }
        public string numremisionventa { get; set; }
        public DateTime fecharemision { get; set; }
        public DateTime fecharegistro { get; set; }
        public int codcliente { get; set; }
        public int codempleado { get; set; }
        public string ruc_ransportista { get; set; }
        public string razonsocial_transportista { get; set; }
        public string chapa_vehiculo { get; set; }
        public string marca_vehiculo { get; set; }
        public string modelo_vehiculo { get; set; }
        public string nrodoc_chofer { get; set; }
        public string nombreapellido_chofer { get; set; }
        public string telefono_chofer { get; set; }
        public int codterminal { get; set; }
        public ICollection<RemisionVentaDetInsertDTO> remisionventadet { get; set; }
    }

    public class RemisionVentaDetInsertDTO
    {
        public int codproducto { get; set; }
        public decimal cantidad { get; set; }
        public decimal costo { get; set; }
    }
}
