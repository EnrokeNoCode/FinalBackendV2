namespace Model.DTO.CobroVenta
{
    public class CobroVentaContadoDTO
    {
        public int codgestion {get;set;}
        public ICollection<CobroVentaFormaCobroListaDTO> cobroVentaFormaCobros {get;set;}
        public ICollection<CobroVentaContadoListaDTO> cobroVentaContados {get;set;}
    }

    public class CobroVentaFormaCobroListaDTO
    {
        public int codformacobro{get;set;}
        public string tipo {get;set;}
        public decimal monto {get;set;}
        public int codbanco {get;set;}
        public DateOnly fechacheque {get;set;}
        public string numcheque {get;set;}
        public string librador {get;set;}
        public int codtipotar {get;set;}
        public string numvaucher {get;set;}
    }

    public class CobroVentaContadoListaDTO
    {
        public int codventa {get;set;}
        public int codmoneda {get;set;}
        public decimal cotizacion {get;set;}
    }
}