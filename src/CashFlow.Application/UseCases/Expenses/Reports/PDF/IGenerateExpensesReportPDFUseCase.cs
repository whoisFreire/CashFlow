namespace CashFlow.Application.UseCases.Expenses.Reports.PDF;
public interface IGenerateExpensesReportPDFUseCase
{
    Task<byte[]> Execute(DateOnly month);
}
