namespace Model.DTO.Compras.Compra
{
    public class ComprasREMListDTO
    {
        public int codcompra { get; set; }
        public string datocompra { get; set; }
        public int codmoneda { get; set; }
        public string nummoneda { get; set; }
        public decimal cotizacion { get; set; }
        //public List<ComprasDetNCListDTO> detalle { get; set; }
    }

    public class ComprasREMDetListDTO
    {
        public int codproducto  { get; set; }
        public string datoproducto { get; set; }
        public decimal disponible { get; set; }
        public decimal costoultimo { get; set; }   
    }
}
