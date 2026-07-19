using Microsoft.AspNetCore.Http;

public class SubmitReportDto
{
    public string Message { get; set; }
    public List<IFormFile>? Images { get; set; }
}