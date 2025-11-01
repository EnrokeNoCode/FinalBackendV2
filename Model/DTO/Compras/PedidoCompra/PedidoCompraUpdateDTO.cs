namespace Model.DTO.Compras.PedidoCompra
{
    public class PedidoCompraUpdateDTO
    {
        public int codpedcompra { get; set; }
        public List<PedidoCompraDetUpdateDTO> pedcompradet { get; set; }
    }

    public class PedidoCompraDetUpdateDTO
    {
        public int codproducto { get; set; }
        public decimal cantidad { get; set; }
        public decimal costoultimo { get; set; }

    }
}
