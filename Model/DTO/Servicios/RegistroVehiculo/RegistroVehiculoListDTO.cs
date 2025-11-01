namespace Model.DTO.Servicios.RegistroVehiculo
{
    public class RegistroVehiculoListDTO
    {
        public int codregistro { get; set; }
        public DateTime fecharegistro { get; set; }
        public string nroregistro { get; set; }
        public string empleado { get; set; }
        public string desestmov { get; set; }
        public string datosucursal { get; set; }
        public string datovehiculo { get; set; }
        public string? datocliente { get; set; }

    }

}