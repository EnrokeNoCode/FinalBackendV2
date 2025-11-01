namespace Model.DTO.Servicios.DiagnosticoTecnico
{

    public class DiagnosticoTecnicoListDTO
    {
        public int coddiagnostico { get; set; }
        public string nrodiagnostico { get; set; }
        public string empleado { get; set; }
        public string desestmov { get; set; }
        public DateTime fechadiagnostico { get; set; }
        public string datosucursal { get; set; }
        public string datovehiculo { get; set; }
        public string datocliente { get; set; }
    }

}