namespace Model.DTO
{
    public class ComprobanteTerminalListDTO
    {
        public int codterminal { get; set; }
        public int inicio { get; set; }
        public int fin { get; set; }
        public int actual { get; set; }
        public DateTime inciovalidez { get; set; }
        public DateTime finvalidez { get; set; }
        public int nrotimbrado { get; set; }
        public string? datocomprobante { get; set; }
    }
}
