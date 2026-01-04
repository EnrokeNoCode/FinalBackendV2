namespace Model.DTO.Servicios.PresupuestoServicio
{
    public class PresupuestoServicioListDTO
    {
        public int codpresupuesto { get; set; }
        public string fechapresupuesto { get; set; }
        public string nropresupuesto { get; set; }
        public string cliente { get; set; }
        public string vehiculo { get; set; }
        public string desestmov { get; set; }
        public string diagnostico { get; set; }
        public decimal totalpresupuesto { get; set; }
    }
}
