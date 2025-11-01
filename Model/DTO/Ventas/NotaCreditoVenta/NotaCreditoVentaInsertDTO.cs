
namespace Model.DTO.Ventas.NotaCreditoVenta
{
        public class NotaCreditoVentaInsertDTO
        {
            public int codventa { get; set; }
            public int codcliente { get; set; }
            public int codtipocomprobante { get; set; }
            public string numnotacredito { get; set; }
            public string nrotimbrado { get; set; }
            public string fechavalidez { get; set; }
            public string fechanotacredito { get; set; }
            public int codsucursal { get; set; }
            public int codempleado { get; set; }
            public int codestmov { get; set; }
            public int codmoneda { get; set; }
            public decimal cotizacion { get; set; }
            public decimal totaliva { get; set; }
            public decimal totalexenta { get; set; }
            public decimal totalgravada { get; set; }
            public decimal totaldescuento { get; set; }
            public decimal totaldevolucion { get; set; }
            public int codterminal { get; set; }
            public int ultimo { get; set; }
            public ICollection<NotaCreditoVentaInsertDetDTO>? notacreditodet { get; set; }
        }
        public class NotaCreditoVentaInsertDetDTO
    {
            public int codproducto { get; set; }
            public decimal cantidaddev { get; set; }
            public decimal preciobruto { get; set; }
            public decimal precioneto { get; set; }
            public decimal costoultimo { get; set; }
            public int codiva { get; set; }
        }
}
