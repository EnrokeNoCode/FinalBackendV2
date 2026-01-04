using Model.Reportes.Servicios;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Reports.Servicios
{
    public class PresupuestoServicioReport : IDocument
    {
        private readonly PresupuestoServicioReporteDTO _data;

        public PresupuestoServicioReport(PresupuestoServicioReporteDTO data)
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
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Helvetica"));

                    // --- HEADER ---
                    page.Header().Column(col =>
                    {
                        col.Item().Text("PRESUPUESTO DE SERVICIO")
                            .FontSize(22)
                            .ExtraBold()
                            .FontColor("#1A3B5D");

                        col.Item().Text($"Nro: {_data.nropresupuesto}")
                            .FontSize(12)
                            .SemiBold();

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Cliente: {_data.cliente}");
                                c.Item().Text($"Vehículo: {_data.vehiculo}");
                                c.Item().Text($"Diagnóstico: {_data.diagnostico}");
                            });

                            row.ConstantItem(150).Column(c =>
                            {
                                c.Item().Text($"Fecha: {_data.fechapresupuesto}");
                            });
                        });

                        col.Item().PaddingVertical(8); // espacio
                        col.Item().LineHorizontal(1.5f).LineColor("#1A3B5D");
                    });

                    // --- CONTENIDO DETALLES ---
                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        // DETALLES DE SERVICIO
                        if (_data.detallerepuesto != null && _data.detallerepuesto.Count > 0)
                        {
                            col.Item().Text("DETALLES DE SERVICIO")
                                .Bold()
                                .FontSize(12)
                                .FontColor("#1A3B5D");

                            col.Item().PaddingBottom(5);

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(3);
                                    columns.ConstantColumn(70);
                                    columns.ConstantColumn(70);
                                });

                                // Header
                                table.Header(header =>
                                {
                                    header.Cell().Text("Servicio").Bold().FontSize(9);
                                    header.Cell().Text("Observación").Bold().FontSize(9);
                                    header.Cell().Text("P. Neto").Bold().FontSize(9).AlignRight();
                                    header.Cell().Text("P. Bruto").Bold().FontSize(9).AlignRight();
                                });

                                // Rows
                                foreach (var det in _data.detallerepuesto)
                                {
                                    table.Cell().Text($"{det.codigotiposervicio} - {det.destiposervicio}").FontSize(8);
                                    table.Cell().Text(det.observacion).FontSize(8);
                                    table.Cell().Text(det.precioneto.ToString("N2")).FontSize(8).AlignRight();
                                    table.Cell().Text(det.preciobruto.ToString("N2")).FontSize(8).AlignRight();
                                }
                            });
                        }

                        // DETALLES DE REPUESTOS
                        if (_data.detalleservicio != null && _data.detalleservicio.Count > 0)
                        {
                            col.Item().PaddingTop(10);
                            col.Item().Text("DETALLES DE REPUESTOS")
                                .Bold()
                                .FontSize(12)
                                .FontColor("#1A3B5D");
                            col.Item().PaddingBottom(5);

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3); // Producto
                                    columns.ConstantColumn(50); // Cantidad
                                    columns.ConstantColumn(70); // P. Bruto
                                    columns.ConstantColumn(70); // P. Neto
                                    columns.ConstantColumn(70); // Total
                                });

                                // Header
                                table.Header(header =>
                                {
                                    header.Cell().Text("Producto").Bold().FontSize(8);
                                    header.Cell().Text("Cant.").Bold().FontSize(8).AlignCenter();
                                    header.Cell().Text("P.Bruto").Bold().FontSize(8).AlignRight();
                                    header.Cell().Text("P.Neto").Bold().FontSize(8).AlignRight();
                                    header.Cell().Text("Total").Bold().FontSize(8).AlignRight();
                                });

                                // Rows
                                foreach (var det in _data.detalleservicio)
                                {
                                    table.Cell().Text(det.codigobarra + "- " + det.desproducto).FontSize(8);
                                    table.Cell().Text(det.cantidad.ToString("N2")).FontSize(8).AlignCenter();
                                    table.Cell().Text(det.preciobruto.ToString("N2")).FontSize(8).AlignRight();
                                    table.Cell().Text(det.precioneto.ToString("N2")).FontSize(8).AlignRight();
                                    table.Cell().Text(det.totallinea.ToString("N2")).FontSize(8).AlignRight();
                                }
                            });
                        }

                        // TOTAL
                        col.Item().PaddingTop(10).AlignRight().Row(r =>
                        {
                            r.RelativeItem().Text("TOTAL PRESUPUESTO:").Bold();
                            r.ConstantItem(100).Text(_data.totalpresupuesto.ToString("N2")).Bold().AlignRight();
                        });
                    });

                    // --- FOOTER ---
                    page.Footer().Height(20).Row(row =>
                    {
                        // IZQUIERDA
                        row.RelativeItem().Text(x =>
                        {
                            x.Span($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8);
                        });

                        // DERECHA
                        row.RelativeItem().Text(x =>
                        {
                            x.Span("Página ").FontSize(8);
                            x.CurrentPageNumber().FontSize(8);
                            x.Span(" de ").FontSize(8);
                            x.TotalPages().FontSize(8);
                        });
                    });
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Error generando PDF de presupuesto de servicio: " + ex.Message, ex);
            }
        }
    }
}
