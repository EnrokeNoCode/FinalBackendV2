namespace Model.DTO.Servicios.DiagnosticoTecnico
{
    public class DiagnosticoTecnicoInsertDTO
    {
        public int coddiagnostico { get; set; }
        public int codtipocomprobante { get; set; }
        public int codsucursal { get; set; }
        public string nrodiagnostico { get; set; }
        public int codestmov { get; set; }
        public int codempleado { get; set; }
        public DateTime fechadiagnostico { get; set; }
        public int codvehiculo { get; set; }
        public int codterminal { get; set; }
        public int ultimo { get; set; }
        public ICollection<DiagnosticoTecnicoDetInsertDTO>? diagtecdet { get; set; }

    }
    public class DiagnosticoTecnicoDetInsertDTO
    {
        public int coddiagnostico { get; set; }
        public int codparte { get; set; }
        public string observacion { get; set; }
    }


}