using Model.Reportes.Caja;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Reports.CajaReports
{
    public class CajaReports : IDocument
    {
        private readonly CajaCobroDTO _data;
        public CajaReports(CajaCobroDTO data)
        {
            _data = data;
        }
        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public void Compose(IDocumentContainer container)
        {
            var grupos = _data.cajaformacobro
                .GroupBy(x => x.formacobro)
                .ToList();

            var totalGeneral = _data.cajaformacobro.Sum(x => x.montocobro);

            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.Header().Column(col =>
                {
                    col.Item().Text("REPORTE DE COBROS DE CAJA")
                        .FontSize(18)
                        .Bold();

                    col.Item().Text($"Sucursal: {_data.sucursal}");
                    col.Item().Text($"Caja: {_data.caja}");
                    col.Item().Text($"Cobrador: {_data.cobrador}");
                });
                page.Content().Column(col =>
                {
                    col.Spacing(15);

                    foreach (var grupo in grupos)
                    {
                        var subtotal = grupo.Sum(x => x.montocobro);
                        col.Item().Text(grupo.Key.ToUpper())
                            .Bold()
                            .FontSize(14)
                            .FontColor(Colors.Blue.Darken2);
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(3);
                                columns.ConstantColumn(100);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Forma").Bold();
                                header.Cell().Text("Detalle").Bold();
                                header.Cell().AlignRight().Text("Monto").Bold();
                            });

                            foreach (var item in grupo)
                            {
                                table.Cell().Text(item.formacobro);
                                table.Cell().Text(item.datoformacobro ?? "-");
                                table.Cell().AlignRight().Text(item.montocobro.ToString("N0"));
                            }
                            table.Cell().ColumnSpan(2)
                                .AlignRight()
                                .Text("SUBTOTAL")
                                .Bold();

                            table.Cell()
                                .AlignRight()
                                .Text(subtotal.ToString("N0"))
                                .Bold();
                        });
                    }
                    col.Item().PaddingTop(10)
                        .AlignRight()
                        .Text($"TOTAL COBRADO: {totalGeneral:N0}")
                        .Bold()
                        .FontSize(16)
                        .FontColor(Colors.Green.Darken2);
                });
            });
        }
    }
}
