namespace Model.Reportes.Caja
{

    public class CajaCobroDTO
    {
        public string sucursal {  get; set; }
        public string caja { get; set; }
        public string cobrador { get; set; }
        public List<CajaFormaCobroDTO> cajaformacobro { get; set; }
    }

    public class CajaFormaCobroDTO
    {
        public string formacobro { get; set; }
        public string datoformacobro { get; set; }
        public decimal montocobro { get; set; }
    }
}
