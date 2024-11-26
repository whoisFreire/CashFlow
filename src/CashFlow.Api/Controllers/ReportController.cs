using CashFlow.Application.UseCases.Expenses.Reports.Excel;
using CashFlow.Application.UseCases.Expenses.Reports.PDF;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace CashFlow.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ReportController : ControllerBase
{
    [HttpGet("excel")]
    public async Task<IActionResult> GetExcel([FromServices] IGenerateExpensesReportExcelUseCase useCase, [FromHeader] DateOnly month)
    {
        var result = await useCase.Execute(month);

        if(result.Length  == 0)
        {
            return NoContent();
        }

        return File(result, MediaTypeNames.Application.Octet, "report.xlsx");
    }

    [HttpGet("pdf")]
    public async Task<IActionResult> GetPDF([FromServices] IGenerateExpensesReportPDFUseCase useCase, [FromQuery] DateOnly month)
    {
        var result = await useCase.Execute(month);

        if (result.Length == 0)
        {
            return NoContent();
        }

        return File(result, MediaTypeNames.Application.Pdf, "report.pdf");
    }

}
