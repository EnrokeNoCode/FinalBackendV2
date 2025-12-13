namespace Model.DTO.Ventas.PresupuestoVenta;

public class PresupuestoVentaDTO
{
    public int codpresupuestoventa {get;set;}
    public string fechapresupuestoventa {get;set;}
    public string numtipocomprobante{get;set;}
    public string numpresupuestoventa{get;set;}
    public string cliente{get;set;}
    public string vendedor{get;set;}
    public string condicionpago{get;set;}
    public decimal totalpresupuestoventa{get;set;}
    public int diaven{get;set;}
    public string nummoneda{get;set;}
    public decimal cotizacion1{get;set;}
    public string observacion{get;set;}
    public string datopedidoventa {get;set;}
    public string dessucursal{get;set;}
    public List<PresupuestoVentaDetDTO> detalle { get; set; }


}

public class PresupuestoVentaDetDTO
{
    public string datoproducto {get;set;}
    public string datoiva {get;set;}
    public decimal cantidad {get;set;}
    public decimal preciobruto {get;set;}
    public decimal totallinea {get;set;}
}