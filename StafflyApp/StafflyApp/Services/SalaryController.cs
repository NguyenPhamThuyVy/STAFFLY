using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Runtime.InteropServices;

[ApiController]
[Route("api/[controller]")]
public class SalaryController : ControllerBase
{
    private readonly ExcelImporter _excelImporter;

    // Inject class vào qua Constructor (Dependency Injection)
    public SalaryController()
    {
        _excelImporter = new ExcelImporter();
    }

    [HttpPost("import")]
    public IActionResult ImportSalary(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File không hợp lệ hoặc trống.");

        // 1. Lưu file tạm thời lên Server
        var tempPath = Path.GetTempFileName();
        using (var stream = new FileStream(tempPath, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        try
        {
            // 2. Gọi class ExcelImporter để xử lý dữ liệu
            string connectionString = "Chuỗi_Kết_Nối_SQL_Server_Của_Bạn";
            _excelImporter.ImportSalaryData(tempPath, connectionString);

            return Ok(new { message = "Import dữ liệu bảng lương thành công!" });
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, $"Lỗi hệ thống: {ex.Message}");
        }
        finally
        {
            // 3. Xóa file tạm sau khi đã import xong để tránh đầy ổ cứng
            if (System.IO.File.Exists(tempPath)) System.IO.File.Delete(tempPath);
        }
    }
}