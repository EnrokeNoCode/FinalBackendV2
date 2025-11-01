namespace Model.DTO.Compras.OrdenCompra
{
    public class OrdenCompraInsertDTO
    {
        public int codtipocomprobante { get; set; }
        public int codterminal { get; set; }
        public int ultimo { get; set; }
        public string numordencompra { get; set; }         
        public string fechaorden { get; set; }             
        public int codestmov { get; set; }
        public int codempleado { get; set; }
        public int codproveedor { get; set; }
        public int codmoneda { get; set; }
        public int codsucursal { get; set; }
        public decimal totaliva { get; set; }
        public decimal totaldescuento { get; set; }
        public decimal totalexento { get; set; }
        public decimal totalgravada { get; set; }
        public decimal totalordencompra { get; set; }       
        public decimal cotizacion { get; set; }
        public string observacion { get; set; }
        public string contactoprv { get; set; }
        public int condiciopago { get; set; }                
        public int? codpresupuestocompra { get; set; }       

        public List<OrdenCompraDetInsertDto> ordcompradet { get; set; }
    }

    public class OrdenCompraDetInsertDto
    {
        public int codproducto { get; set; }
        public decimal cantidad { get; set; }
        public decimal preciobruto { get; set; }
        public decimal precioneto { get; set; }
        public decimal costoultimo { get; set; }
        public int codiva { get; set; }
    }
}
