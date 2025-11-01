namespace Model.DTO.Compras.Ajustes
{
    public class AjustesInsertDTO
    {
        public int codtipocomprobante { get; set; }
        public int codsucursal { get; set; }
        public string numajuste { get; set; }
        public string fechaajuste { get; set; }
        public int codmotivo { get; set; }
        public int codempleado { get; set; }
        public int condicion { get; set; }
        public int codterminal { get; set; }
        public int ultimo { get; set; }
        public ICollection<AjustesDetInsertDTO>? ajustedet { get; set; }
    }

    public class AjustesDetInsertDTO
    {
        public int codproducto { get; set; }
        public decimal cantidad { get; set; }
    }
}
