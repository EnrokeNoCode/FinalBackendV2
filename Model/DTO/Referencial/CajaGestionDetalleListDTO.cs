namespace Model.DTO.Referencial
{
    public class CajaGestionDetalleListDTO
    {
        public int codgestion { get; set; }
        public string numsucursal { get; set; }
        public string dessucursal { get; set; }
        public string numcaja { get; set; }
        public string descaja { get; set; } 
        public string cobrador { get; set; }
        public DateTime fechaapertura { get; set; }
        public DateTime fechacierre { get; set; } 
        public decimal montoapertura { get; set; }
        public decimal montocierre { get; set; }
    }
}
