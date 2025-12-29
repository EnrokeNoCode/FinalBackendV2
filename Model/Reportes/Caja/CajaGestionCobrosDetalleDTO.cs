namespace Model.Reportes.Caja
{
    public class CajaGestionCobrosDetalleDTO
    {
        public int codgestion { get; set; }
        public string numtipocomprobante { get; set; }
        public string numventa { get; set; }
        public string nrodoc { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; } 
        public string detallecobros { get; set; }
        public decimal totalcobrado { get; set; }
    }
}
