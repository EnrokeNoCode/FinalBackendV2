namespace Model.DTO.Servicios.DiagnosticoTecnico
{
    public class DiagnosticoTecnicoListWithDetDTO
    {
        public string nrodiagnostico { get; set; }
        public string empleado { get; set; }
        public string desestmov { get; set; }
        public DateTime fechadiagnostico { get; set; }
        public string datovehiculo { get; set; }
        public string datocliente { get; set; }
        public ICollection<DiagnosticoTecnicoListDetDTO>? diagtecwithdet { get; set; }
    }

    public class DiagnosticoTecnicoListDetDTO
    {
        public string numparte { get; set; }
        public string desparte { get; set; }
        public string observacion { get; set; }
    }

}