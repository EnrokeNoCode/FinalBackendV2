
namespace Model.DTO.Compras.PresupuestoCompra
{
    public class PresupuestoCompraOCListDTO
    {
        public int codpresupuestocompra { get; set; }
        public int codproveedor { get; set; }
        public DateTime fechapresupuesto { get; set; }
        public string datopresupuesto { get; set; }
        public decimal totalpresupuestocompra { get; set; }
        public List<PresupuestoCompraOCListDetDTO> presdetoc { get; set; }
    }
    public class PresupuestoCompraOCListDetDTO
    {
        public int codpresupuestocompra { get; set; }
        public int codproducto { get; set; }
        public string producto { get; set; }
        public int codiva { get; set; }
        public string iva { get; set; }
        public decimal cantidad { get; set; }
        public decimal preciobruto { get; set; }
        public decimal totallinea { get; set; }
    }
}