namespace Model.DTO.Compras.Ajustes
{
    public class AjusteUpdateDTO
    {
        public int codajuste { get; set; }
        public int codsucursal { get; set; }
        public List<AjustesDetUpdateDTO> ajustesdet { get; set; }
    }

    public class AjustesDetUpdateDTO
    {
        public int codproducto { get; set; }
        public decimal cantidad { get; set; }

    }
}
