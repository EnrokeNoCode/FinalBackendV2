using Model.Reportes.Caja;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Reports.CajaReports
{
    public class CajaReports : IDocument
    {
        private readonly List<CajaFormaCobroDTO> _data;
        private readonly int _codGestion;

        public CajaReports(List<CajaFormaCobroDTO> data, int codGestion)
        {
            _data = data;
            _codGestion = codGestion;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);

                page.Header().Text($"Detalle de Cobros - Gestión {_codGestion}")
                    .FontSize(18).Bold();

                page.Content().Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn(2);
                            columns.ConstantColumn(100);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Forma de Cobro").Bold();
                            header.Cell().Text("Detalle").Bold();
                            header.Cell().AlignRight().Text("Monto").Bold();
                        });

                        foreach (var item in _data)
                        {
                            table.Cell().Text(item.formacobro);
                            table.Cell().Text(item.datoformacobro);
                            table.Cell().AlignRight().Text(item.montocobro.ToString("N0"));
                        }
                    });

                    col.Item()
                       .AlignRight()
                       .Text($"TOTAL COBRADO: {_data.Sum(x => x.montocobro):N0}")
                       .Bold()
                       .FontSize(14);
                });
            });
        }
    }
}
