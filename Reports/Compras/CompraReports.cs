using Model.Reportes.Compras;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Reports.ComprasReports
{
    public class CompraReports : IDocument
    {
        private readonly List<CompraListadoReporteDTO> _data;
        private readonly bool _mostrarDetalle;

        public CompraReports(List<CompraListadoReporteDTO> data, bool mostrarDetalle = false)
        {
            _data = data;
            _mostrarDetalle = mostrarDetalle;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            try
            {
                var totalGeneral = _data.Sum(x => x.totalcompra);
                var totalIva = _data.Sum(x => x.totaliva);
                var totalExento = _data.Sum(x => x.totalexento);

                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Helvetica"));
                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("REPORTE DE COMPRAS")
                                    .FontSize(22).ExtraBold().FontColor("#1A3B5D");

                                c.Item().Text("Listado consolidado de facturas y comprobantes")
                                    .FontSize(9).Italic().FontColor(Colors.Grey.Medium);
                            });

                            row.ConstantItem(120).AlignRight().Column(c =>
                            {
                                c.Item().Text($"{DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9);
                                c.Item().Text($"Registros: {_data.Count}").FontSize(9).SemiBold();
                            });
                        });

                        col.Item().PaddingVertical(8).LineHorizontal(1.5f).LineColor("#1A3B5D");
                    });
                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        foreach (var compra in _data)
                        {
                            col.Item().PaddingTop(10).Background(Colors.Grey.Lighten4).Padding(6).Row(r =>
                            {
                                r.RelativeItem().Column(c =>
                                {
                                    c.Item().Text($"{compra.numtipocomprobante}-{compra.numcompra}")
                                        .Bold().FontSize(11);
                                    c.Item().Text($"{compra.razonsocial} | {compra.nrodocprv}")
                                        .FontSize(9).FontColor(Colors.Grey.Darken2);
                                });

                                r.ConstantItem(150).AlignRight().Column(c =>
                                {
                                    c.Item().Text($"Fecha: {compra.fechacompra:dd/MM/yyyy}").FontSize(9);
                                    c.Item().Text($"Condición: {compra.condicionpago}").FontSize(9);
                                });
                            });

                            if (_mostrarDetalle && compra.detalle != null && compra.detalle.Count > 0)
                            {
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.ConstantColumn(50);
                                        columns.ConstantColumn(70);
                                        columns.ConstantColumn(70);
                                        columns.ConstantColumn(70);
                                        columns.ConstantColumn(40);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().BorderBottom(0.5f).PaddingVertical(2).Text("Producto").Bold().FontSize(8);
                                        header.Cell().BorderBottom(0.5f).PaddingVertical(2).AlignCenter().Text("Cant.").Bold().FontSize(8);
                                        header.Cell().BorderBottom(0.5f).PaddingVertical(2).AlignRight().Text("P.Bruto").Bold().FontSize(8);
                                        header.Cell().BorderBottom(0.5f).PaddingVertical(2).AlignRight().Text("P.Neto").Bold().FontSize(8);
                                        header.Cell().BorderBottom(0.5f).PaddingVertical(2).AlignRight().Text("Total").Bold().FontSize(8);
                                        header.Cell().BorderBottom(0.5f).PaddingVertical(2).AlignCenter().Text("IVA").Bold().FontSize(8);
                                    });

                                    foreach (var det in compra.detalle)
                                    {
                                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).Text(det.desproducto).FontSize(8);
                                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).AlignCenter().Text(det.cantidad.ToString("N2")).FontSize(8);
                                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).AlignRight().Text(det.preciobruto.ToString("N0")).FontSize(8);
                                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).AlignRight().Text(det.precioneto.ToString("N0")).FontSize(8);
                                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).AlignRight().Text(det.totallinea.ToString("N0")).FontSize(8);
                                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).PaddingVertical(2).AlignCenter().Text(det.desiva).FontSize(8);
                                    }
                                });
                            }

                            col.Item().AlignRight().Width(180).PaddingVertical(5).Table(t =>
                            {
                                t.ColumnsDefinition(cs => { cs.RelativeColumn(); cs.RelativeColumn(); });

                                t.Cell().Text("Iva:").FontSize(8).FontColor(Colors.Grey.Darken1);
                                t.Cell().AlignRight().Text(compra.totaliva.ToString("N0")).FontSize(8).FontColor(Colors.Grey.Darken1);

                                t.Cell().Text("TOTAL:").FontSize(9).Bold();
                                t.Cell().AlignRight().Text(compra.totalcompra.ToString("N0")).FontSize(9).Bold().FontColor("#1A3B5D");
                            });

                            col.Item().PaddingVertical(2).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                        }

                        col.Item().PaddingTop(20).Background("#1A3B5D").Padding(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.ConstantColumn(180);
                            });

                            table.Cell().AlignMiddle().Text("RESUMEN DE OPERACIONES").FontColor(Colors.White).Bold();

                            table.Cell().Column(c =>
                            {
                                c.Item().Row(r => {
                                    r.RelativeItem().Text("TOTAL EXENTO:").FontSize(8).FontColor(Colors.White);
                                    r.RelativeItem().AlignRight().Text(totalExento.ToString("N0")).FontSize(8).FontColor(Colors.White);
                                });
                                c.Item().Row(r => {
                                    r.RelativeItem().Text("TOTAL IVA:").FontSize(8).FontColor(Colors.White);
                                    r.RelativeItem().AlignRight().Text(totalIva.ToString("N0")).FontSize(8).FontColor(Colors.White);
                                });
                                c.Item().PaddingTop(3).BorderTop(0.5f).BorderColor(Colors.White).Row(r => {
                                    r.RelativeItem().Text("TOTAL GENERAL:").Bold().FontColor(Colors.White);
                                    r.RelativeItem().AlignRight().Text(totalGeneral.ToString("N0")).FontSize(14).Bold().FontColor(Colors.White);
                                });
                            });
                        });
                    });

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
                throw new Exception("Error generando el PDF de compras.", ex);
            }
        }
    }
}