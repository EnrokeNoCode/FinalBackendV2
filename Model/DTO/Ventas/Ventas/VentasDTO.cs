
namespace Model.DTO.Ventas.Ventas
{
    public class VentasDTO
    {
        public int codventa { get; set; }
        public string numventa { get; set; }
        public DateTime fechaventa { get; set; }
        public string datocliente { get; set; }
        public string datopresupuesto { get; set; }
        public decimal totalventa { get; set; }
        public decimal totaliva { get; set; }
        public decimal totalexenta { get; set; }
        public string condicion { get; set; }
        public decimal cotizacion { get; set; }
        public string moneda { get; set; }
        public List<VentasDetDTO> detalle { get; set; }
    }

    public class VentasDetDTO
    {
        public string datoproducto { get; set; }
        public decimal cantidad { get; set; }
        public decimal totallinea { get; set; }
        public decimal preciobruto { get; set; }
        public decimal precioneto { get; set; }
        public string datoiva { get; set; }

    }
}
