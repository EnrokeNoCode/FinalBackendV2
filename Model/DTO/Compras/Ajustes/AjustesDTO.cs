namespace Model.DTO.Compras.Ajustes
{
    public class AjustesDTO
    {
        public int codajuste { get; set; }
        public DateTime fechaajuste { get; set; }
        public string numtipocomprobante { get; set; }
        public string destipocomprobante { get; set; }
        public string numajuste { get; set; }
        public string empleado { get; set; }
        public string estado { get; set; }
        public string numsucursal { get; set; }
        public string dessucursal { get; set; }
        public string datomotivo { get; set; }
        public ICollection<AjustesDetDTO>? ajustesdet { get; set; }
    }
    public class AjustesDetDTO
    {
        public int codajuste { get; set; }
        public int codproducto { get; set; }
        public string? codigobarra { get; set; }
        public string? desproducto { get; set; }
        public decimal cantidad { get; set; }
    }
}
