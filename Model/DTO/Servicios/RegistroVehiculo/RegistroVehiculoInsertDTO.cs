namespace Model.DTO.Servicios.RegistroVehiculo
{
    public class RegistroVehiculoInsertDTO
    {
        public int codcliente { get; set; }
        public int codsucursal { get; set; }
        public int codempleado { get; set; }
        public int codestmov { get; set; }
        public int codtipocomprobante { get; set; }
        public string numregistro { get; set; }
        public DateTime fecharegistro { get; set; }
        public int codmarca { get; set; }
        public string modelo { get; set; }
        public string nrochapa { get; set; }
        public string nrochasis { get; set; }
        public int codterminal { get; set; }
    }
}