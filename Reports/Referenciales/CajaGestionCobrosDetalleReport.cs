using Model.Reportes.Caja;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Reports.Referenciales
{
    public class CajaGestionCobrosDetalleReport : IDocument
    {
        private readonly List<CajaGestionCobrosDetalleDTO> _data;

        public CajaGestionCobrosDetalleReport(List<CajaGestionCobrosDetalleDTO> data)
        {
            _data = data;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Helvetica"));
                page.Header().Column(col =>
                {
                    col.Item().Text("REPORTE DE COBROS POR VENTA")
                        .FontSize(18)
                        .ExtraBold()
                        .FontColor("#1A3B5D");

                    col.Item().Text($"Gestión N°: {_data.FirstOrDefault()?.codgestion}")
                        .FontSize(10)
                        .SemiBold();

                    col.Item().PaddingVertical(5)
                        .LineHorizontal(1.5f)
                        .LineColor("#1A3B5D");
                });
                page.Content().PaddingVertical(10).Column(col =>
                {
                    foreach (var v in _data)
                    {
                        col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(box =>
                        {
                            box.Item().Row(r =>
                            {
                                r.RelativeItem().Text($"Comprobante: {v.numtipocomprobante} - {v.numventa}")
                                    .Bold();

                                r.ConstantItem(120)
                                    .AlignRight()
                                    .Text($"Total: {v.totalcobrado:N0}")
                                    .Bold();
                            });

                            box.Item().Text($"Cliente: {v.nrodoc} - {v.nombre} {v.apellido}")
                                .FontSize(9);

                            box.Item().PaddingVertical(5)
                                .LineHorizontal(1)
                                .LineColor(Colors.Grey.Lighten1);
                            box.Item().Text("Detalle de cobros:")
                                .SemiBold()
                                .FontSize(9);

                            var cobros = v.detallecobros.Split(" | ");

                            foreach (var c in cobros)
                            {
                                var textoLimpio = c.Contains("|")
                                    ? c.Split('|')[1].Trim()
                                    : c.Trim();

                                box.Item().Text($"• {textoLimpio}")
                                    .FontSize(8);
                            }
                        });

                        col.Item().PaddingBottom(8);
                    }
                });
                page.Footer().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Text(t =>
                    {
                        t.Span("Generado el: ").FontSize(8);
                        t.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                            .FontSize(8)
                            .SemiBold();
                    });
                    row.RelativeItem().AlignRight().Text(t =>
                    {
                        t.Span("Página ").FontSize(8);
                        t.CurrentPageNumber().FontSize(8);
                        t.Span(" de ").FontSize(8);
                        t.TotalPages().FontSize(8);
                    });
                });
            });
        }
    }
}
