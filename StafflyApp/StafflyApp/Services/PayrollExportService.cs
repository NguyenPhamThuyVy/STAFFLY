using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.IO;

public class PayrollExportService
{
    public byte[] ExportPayrollToExcel(List<PayrollReportDto> payrollData, string period)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var package = new ExcelPackage())
        {
            // Tạo Worksheet Chi Tiết
            var ws = package.Workbook.Worksheets.Add("Bảng Lương Chi Tiết");
            ws.View.ShowGridLines = true;
            // 1. Tiêu đề Báo cáo
            ws.Cells["A2"].Value = "STAFFLY - HỆ THỐNG QUẢN TRỊ NHÂN SỰ THÔNG MINH";
            ws.Cells["A2"].Style.Font.Name = "Segoe UI";
            ws.Cells["A2"].Style.Font.Size = 11;
            ws.Cells["A2"].Style.Font.Color.SetColor(Color.Gray);

            ws.Cells["A3"].Value = "BẢNG LƯƠNG NHÂN VIÊN CHI TIẾT";
            ws.Cells["A3"].Style.Font.Name = "Segoe UI";
            ws.Cells["A3"].Style.Font.Size = 16;
            ws.Cells["A3"].Style.Font.Bold = true;
            ws.Cells["A3"].Style.Font.Color.SetColor(Color.FromArgb(27, 54, 93)); // Navy Dark

            ws.Cells["A4"].Value = $"Kỳ tính lương: {period} | Trạng thái: Đã duyệt";
            ws.Cells["A4"].Style.Font.Italic = true;

            // 2. Thiết lập Header Bảng
            string[] headers = { "Mã NV", "Họ và Tên", "Chức Vụ", "Phòng Ban", "Lương Cơ Bản", "Phụ Cấp", "Lương Ngày Công", "Lương Tăng Ca", "Tổng Thu Nhập", "Bảo Hiểm (10.5%)", "Thuế TNCN", "Thực Nhận", "Trạng Thái" };
            int headerRow = 6;

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cells[headerRow, i + 1];
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.Color.SetColor(Color.White);
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(27, 54, 93));
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cell.Style.WrapText = true;
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.LightGray);
            }
            ws.Row(headerRow).Height = 28;

            // 3. Đổ dữ liệu nhân viên
            int startRow = 7;
            for (int i = 0; i < payrollData.Count; i++)
            {
                int r = startRow + i;
                var emp = payrollData[i];

                ws.Cells[r, 1].Value = emp.EmployeeId;
                ws.Cells[r, 2].Value = emp.FullName;
                ws.Cells[r, 3].Value = emp.Position;
                ws.Cells[r, 4].Value = emp.Department;

                // Gán giá trị số & Viết công thức Excel tự động thay vì gán giá trị cứng tĩnh
                ws.Cells[r, 5].Value = emp.BaseSalary;
                ws.Cells[r, 6].Value = emp.Allowance;
                ws.Cells[r, 7].Formula = $"=(E{r}/22)*{emp.ActualWorkdays}"; // Tính lương ngày công
                ws.Cells[r, 8].Value = emp.OvertimePay;
                ws.Cells[r, 9].Formula = $"=G{r}+F{r}+H{r}";                 // Tổng thu nhập
                ws.Cells[r, 10].Formula = $"=I{r}*0.105";                     // Bảo hiểm trích nộp
                ws.Cells[r, 11].Value = emp.PersonalTax;
                ws.Cells[r, 12].Formula = $"=I{r}-J{r}-K{r}";                 // Thực nhận

                var statusCell = ws.Cells[r, 13];
                statusCell.Value = emp.Status;
                statusCell.Style.Font.Bold = true;
                statusCell.Style.Font.Color.SetColor(Color.FromArgb(21, 87, 36));
                statusCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                statusCell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(212, 237, 218)); // Soft green

                // Định dạng hiển thị tiền tệ & Căn lề dòng dữ liệu
                for (int col = 1; col <= 13; col++)
                {
                    var cell = ws.Cells[r, col];
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.LightGray);
                    if (col >= 5 && col <= 12)
                    {
                        cell.Style.Numberformat.Format = "#,##0";
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    }
                    if (col == 1 || col == 4 || col == 13) cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // Thêm dòng sọc xen kẽ (Zebra rows)
                    if (i % 2 == 1 && col != 13)
                    {
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(242, 245, 249));
                    }
                }
                ws.Row(r).Height = 22;
            }

            // 4. Dòng Tổng Cộng (Total Row)
            int totRow = startRow + payrollData.Count;
            ws.Cells[totRow, 1].Value = "Tổng cộng";
            ws.Cells[totRow, 1].Style.Font.Bold = true;
            ws.Cells[totRow, 1, totRow, 4].Merge = true;
            ws.Cells[totRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            for (int col = 5; col <= 12; col++)
            {
                var colLetter = ((char)('A' + col - 1)).ToString();
                var cell = ws.Cells[totRow, col];
                cell.Formula = $"=SUM({colLetter}{startRow}:{colLetter}{totRow - 1})";
                cell.Style.Font.Bold = true;
                cell.Style.Numberformat.Format = "#,##0";
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            }

            // Kẻ viền double dưới dòng tổng chuẩn kế toán
            for (int col = 1; col <= 13; col++)
            {
                var cell = ws.Cells[totRow, col];
                cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Bottom.Style = ExcelBorderStyle.Double;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(230, 236, 245));
            }
            ws.Row(totRow).Height = 24;

            // Tự động căn chỉnh chiều rộng cột vừa với text
            foreach (var column in ws.Columns)
            {
                column.AutoFit();
                if (column.StartColumn == 2) column.Width = 24;
            }
                return package.GetAsByteArray();
            }
        }
    }