using Model.Reportes.Producto;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Reports.Referenciales
{
    public class ProductoReports : IDocument
    {
        private readonly List<ProductoListadoDTO> _data;

        public ProductoReports(List<ProductoListadoDTO> data)
        {
            _data = data;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            try
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Helvetica"));

                    // HEADER
                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("REPORTE DE PRODUCTOS")
                                    .FontSize(20).ExtraBold().FontColor("#1A3B5D");

                                c.Item().Text("Listado general de productos")
                                    .FontSize(9).Italic().FontColor(Colors.Grey.Medium);
                            });

                            row.ConstantItem(120).AlignRight().Column(c =>
                            {
                                c.Item().Text($"{DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9);
                                c.Item().Text($"Registros: {_data.Count}").FontSize(9).SemiBold();
                            });
                        });

                        col.Item().PaddingVertical(8)
                            .LineHorizontal(1.5f)
                            .LineColor("#1A3B5D");
                    });

                    // CONTENT
                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // Producto
                                columns.RelativeColumn(3); // Proveedor
                                columns.RelativeColumn(2); // Sección
                                columns.ConstantColumn(60); // IVA
                                columns.ConstantColumn(70); // Costo
                                columns.ConstantColumn(60); // Stock
                                columns.ConstantColumn(60); // Estado
                            });

                            // HEADER TABLA
                            table.Header(header =>
                            {
                                header.Cell().Text("Producto").Bold();
                                header.Cell().Text("Proveedor").Bold();
                                header.Cell().Text("Sección").Bold();
                                header.Cell().AlignCenter().Text("IVA").Bold();
                                header.Cell().AlignRight().Text("Costo").Bold();
                                header.Cell().AlignCenter().Text("Stock").Bold();
                                header.Cell().AlignCenter().Text("Estado").Bold();
                            });

                            foreach (var p in _data)
                            {
                                table.Cell().Text(p.datoproducto);
                                table.Cell().Text(p.datoproveedor).FontSize(8);
                                table.Cell().Text(p.datoseccion).FontSize(8);
                                table.Cell().AlignCenter().Text(p.datoiva);
                                table.Cell().AlignRight().Text(p.costoultimo.ToString("N0"));
                                table.Cell().AlignCenter().Text(p.afectastock);
                                table.Cell().AlignCenter().Text(p.estado);
                            }
                        });
                    });

                    // FOOTER
                    page.Footer().PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem().Text(t =>
                        {
                            t.Span("Generado el: ").FontSize(8);
                            t.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(8).SemiBold();
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
            catch (Exception ex)
            {
                throw new Exception("Error generando el PDF de productos.", ex);
            }
        }
    }
}
