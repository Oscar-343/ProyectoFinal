using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinal.Filters;
using ProyectoFinal.Service;

namespace ProyectoFinal.Controllers
{
    [RequiereSesion]
    public class ReporteController : Controller
    {
        private readonly IReporteService _reporteService;

        public ReporteController(IReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta)
        {
            var fechaDesde = desde ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var fechaHasta = hasta ?? DateTime.Today;

            var reporte = await _reporteService.GenerarReporteAsync(fechaDesde, fechaHasta);

            ViewBag.Desde = fechaDesde.ToString("yyyy-MM-dd");
            ViewBag.Hasta = fechaHasta.ToString("yyyy-MM-dd");

            return View(reporte);
        }

        [HttpPost]
        public async Task<IActionResult> ExportarExcel(DateTime desde, DateTime hasta)
        {
            var reporte = await _reporteService.GenerarReporteAsync(desde, hasta);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Reporte");

            var colorMorado = XLColor.FromHtml("#6a2fb0");
            var colorEncabezadoTabla = XLColor.FromHtml("#404040");
            var colorFilaAlterna = XLColor.FromHtml("#f2f2f2");
            var colorBorde = XLColor.FromHtml("#d9d9d9");

            // ===== TÍTULO CENTRADO ARRIBA DE TODA LA TABLA =====
            ws.Cell(1, 1).Value = "AMIGURUMIS LUNA";
            ws.Range(1, 1, 1, 7).Merge();
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 22;
            ws.Cell(1, 1).Style.Font.FontColor = colorMorado;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell(2, 1).Value = $"Reporte del {desde:dd/MM/yyyy} al {hasta:dd/MM/yyyy}";
            ws.Range(2, 1, 2, 7).Merge();
            ws.Cell(2, 1).Style.Font.FontSize = 12;
            ws.Cell(2, 1).Style.Font.Italic = true;
            ws.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;
            ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // ===== RESUMEN SIMPLE =====
            ws.Cell(4, 1).Value = "Dinero ingresado";
            ws.Cell(4, 1).Style.Font.Bold = true;
            ws.Cell(4, 2).Value = reporte.IngresosTotales;
            ws.Cell(4, 2).Style.NumberFormat.Format = "Bs #,##0.00";

            ws.Cell(4, 4).Value = "Gasto en materiales";
            ws.Cell(4, 4).Style.Font.Bold = true;
            ws.Cell(4, 5).Value = reporte.GastoMateriales;
            ws.Cell(4, 5).Style.NumberFormat.Format = "Bs #,##0.00";

            ws.Cell(5, 1).Value = "Ganancia neta";
            ws.Cell(5, 1).Style.Font.Bold = true;
            ws.Cell(5, 2).Value = reporte.UtilidadTotal;
            ws.Cell(5, 2).Style.NumberFormat.Format = "Bs #,##0.00";
            ws.Cell(5, 2).Style.Font.FontColor = colorMorado;
            ws.Cell(5, 2).Style.Font.Bold = true;

            string[] encabezados = { "N.° PEDIDO", "CLIENTE", "FECHA PEDIDO", "FECHA ENTREGA", "ESTADO", "HORAS", "TOTAL" };

            // ===== TABLA DE PEDIDOS NORMALES =====
            int filaEncabezado = 7;

            ws.Cell(filaEncabezado - 1, 1).Value = "PEDIDOS";
            ws.Cell(filaEncabezado - 1, 1).Style.Font.Bold = true;
            ws.Cell(filaEncabezado - 1, 1).Style.Font.FontSize = 14;
            ws.Cell(filaEncabezado - 1, 1).Style.Font.FontColor = colorMorado;

            for (int col = 0; col < encabezados.Length; col++)
            {
                var celda = ws.Cell(filaEncabezado, col + 1);
                celda.Value = encabezados[col];
                celda.Style.Font.Bold = true;
                celda.Style.Font.FontColor = XLColor.White;
                celda.Style.Font.FontSize = 10;
                celda.Style.Fill.BackgroundColor = colorEncabezadoTabla;
                celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                celda.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            var normales = reporte.Pedidos.Where(p => p.Tipo == "Normal").ToList();

            int fila = filaEncabezado + 1;
            foreach (var p in normales)
            {
                ws.Cell(fila, 1).Value = $"P{p.IdPedido:D4}";
                ws.Cell(fila, 2).Value = p.Cliente;
                ws.Cell(fila, 3).Value = p.FechaPedido;
                ws.Cell(fila, 3).Style.DateFormat.Format = "dd/MM/yyyy";
                ws.Cell(fila, 4).Value = p.FechaEntrega;
                ws.Cell(fila, 4).Style.DateFormat.Format = "dd/MM/yyyy";
                ws.Cell(fila, 5).Value = p.Estado;
                ws.Cell(fila, 6).Value = p.Horas;
                ws.Cell(fila, 7).Value = p.Total;
                ws.Cell(fila, 7).Style.NumberFormat.Format = "Bs #,##0.00";

                var rango = ws.Range(fila, 1, fila, 7);
                rango.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                rango.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                rango.Style.Border.OutsideBorderColor = colorBorde;
                rango.Style.Border.InsideBorderColor = colorBorde;

                if ((fila - filaEncabezado) % 2 == 0)
                {
                    rango.Style.Fill.BackgroundColor = colorFilaAlterna;
                }
                fila++;
            }

            // ===== TABLA DE PEDIDOS PERSONALIZADOS =====
            var personalizados = reporte.Pedidos.Where(p => p.Tipo == "Personalizado").ToList();

            if (personalizados.Any())
            {
                int filaTituloPersonalizados = fila + 2;
                ws.Cell(filaTituloPersonalizados, 1).Value = "PEDIDOS PERSONALIZADOS";
                ws.Range(filaTituloPersonalizados, 1, filaTituloPersonalizados, 7).Merge();
                ws.Cell(filaTituloPersonalizados, 1).Style.Font.Bold = true;
                ws.Cell(filaTituloPersonalizados, 1).Style.Font.FontSize = 14;
                ws.Cell(filaTituloPersonalizados, 1).Style.Font.FontColor = colorMorado;

                int filaEncabezadoPersonalizados = filaTituloPersonalizados + 1;
                for (int col = 0; col < encabezados.Length; col++)
                {
                    var celda = ws.Cell(filaEncabezadoPersonalizados, col + 1);
                    celda.Value = encabezados[col];
                    celda.Style.Font.Bold = true;
                    celda.Style.Font.FontColor = XLColor.White;
                    celda.Style.Font.FontSize = 10;
                    celda.Style.Fill.BackgroundColor = colorEncabezadoTabla;
                    celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    celda.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                int filaP = filaEncabezadoPersonalizados + 1;
                foreach (var p in personalizados)
                {
                    ws.Cell(filaP, 1).Value = $"P{p.IdPedido:D4}";
                    ws.Cell(filaP, 2).Value = p.Cliente;
                    ws.Cell(filaP, 3).Value = p.FechaPedido;
                    ws.Cell(filaP, 3).Style.DateFormat.Format = "dd/MM/yyyy";
                    ws.Cell(filaP, 4).Value = p.FechaEntrega;
                    ws.Cell(filaP, 4).Style.DateFormat.Format = "dd/MM/yyyy";
                    ws.Cell(filaP, 5).Value = p.Estado;
                    ws.Cell(filaP, 6).Value = p.Horas;
                    ws.Cell(filaP, 7).Value = p.Total;
                    ws.Cell(filaP, 7).Style.NumberFormat.Format = "Bs #,##0.00";

                    var rangoP = ws.Range(filaP, 1, filaP, 7);
                    rangoP.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    rangoP.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    rangoP.Style.Border.OutsideBorderColor = colorBorde;
                    rangoP.Style.Border.InsideBorderColor = colorBorde;

                    if ((filaP - filaEncabezadoPersonalizados) % 2 == 0)
                    {
                        rangoP.Style.Fill.BackgroundColor = colorFilaAlterna;
                    }
                    filaP++;
                }
            }

            ws.SheetView.FreezeRows(filaEncabezado);
            ws.Columns().AdjustToContents();
            ws.Column(2).Width = 22;

            // ===== DESCARGA =====
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Reporte_{desde:yyyyMMdd}_{hasta:yyyyMMdd}.xlsx");
        }
    }
}