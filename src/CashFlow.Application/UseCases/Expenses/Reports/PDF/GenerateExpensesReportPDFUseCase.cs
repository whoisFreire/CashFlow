
using CashFlow.Application.UseCases.Expenses.Reports.PDF.Colors;
using CashFlow.Application.UseCases.Expenses.Reports.PDF.Fonts;
using CashFlow.Domain.Entities;
using CashFlow.Domain.Extensions;
using CashFlow.Domain.Reports;
using CashFlow.Domain.Repositories.Expenses;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Drawing;
using PdfSharp.Fonts;

namespace CashFlow.Application.UseCases.Expenses.Reports.PDF;

public class GenerateExpensesReportPDFUseCase : IGenerateExpensesReportPDFUseCase
{
    private const string CURRENCY_SYMBOL = "R$";
    private const int HEIGHT_ROW_EXPENSE_TABLE = 25;
    private readonly IExpensesReadOnlyRepository _repository;

    public GenerateExpensesReportPDFUseCase(IExpensesReadOnlyRepository repository)
    {
        GlobalFontSettings.FontResolver = new ExpensesReportFontResolver();
        _repository = repository;
    }

    public async Task<byte[]> Execute(DateOnly month)
    {
        var expenses = await _repository.FilterByMonth(month);

        if(expenses.Count == 0)
        {
            return [];
        }

        var document = CreateDocument(month);
        var page = CreatePage(document);

        CreateHeaderWithProfileName(page);

        var totalExpenses = expenses.Sum(expense => expense.Amount);
        CreateTotalSpentSection(page, month, totalExpenses);

        CreateExpensesTable(expenses, page);

        return RenderDocument(document);
    }

    private Document CreateDocument(DateOnly month)
    {
        var document = new Document();
        document.Info.Title = $"{ResourceReportGenerationMessages.EXPENSE_FOR} {month:Y}";
        document.Info.Author = "Leonardo";

        var style = document.Styles["Normal"];
        style!.Font.Name = FontHelper.RALEWAY_REGULAR;
        return document;
    }

    private Section CreatePage(Document document)
    {
        var section = document.AddSection();
        section.PageSetup = document.DefaultPageSetup.Clone();
        section.PageSetup.PageFormat = PageFormat.A4;
        section.PageSetup.LeftMargin = 40;
        section.PageSetup.RightMargin = 40;
        section.PageSetup.TopMargin = 80;
        section.PageSetup.BottomMargin = 80;

        return section;
    }


    private void CreateHeaderWithProfileName(Section page)
    {
        var table = page.AddTable();
        table.AddColumn("300");

        var row = table.AddRow();
        row.Cells[0].AddParagraph("Hey, Leonardo");
        row.Cells[0].Format.Font = new Font
        {
            Name = FontHelper.RALEWAY_BLACK,
            Size = 16
        };
        row.Cells[0].VerticalAlignment = MigraDoc.DocumentObjectModel.Tables.VerticalAlignment.Center;
    }

    private void CreateTotalSpentSection(Section page, DateOnly month, decimal totalExpenses)
    {
        var paragraph = page.AddParagraph();
        paragraph.Format.SpaceBefore = "40";
        paragraph.Format.SpaceAfter = "40";

        var title = string.Format(ResourceReportGenerationMessages.TOTAL_SPENT_IN, month.ToString("Y"));

        paragraph.AddFormattedText(title, new Font
        {
            Name = FontHelper.RALEWAY_REGULAR,
            Size = 15
        });

        paragraph.AddLineBreak();

        paragraph.AddFormattedText($"{CURRENCY_SYMBOL} {totalExpenses}", new Font
        {
            Name = FontHelper.WORKSANS_BLACK,
            Size = 50
        });
    }

    private Table CreateExpenseDetailTable(Section page)
    {
        var table = page.AddTable();
        table.AddColumn("195").Format.Alignment = ParagraphAlignment.Left;
        table.AddColumn("80").Format.Alignment = ParagraphAlignment.Center;
        table.AddColumn("120").Format.Alignment = ParagraphAlignment.Center;
        table.AddColumn("120").Format.Alignment = ParagraphAlignment.Right;


        return table;
    }


    private void CreateExpensesTable(List<Expense> expenses, Section page)
    {
        foreach (var expense in expenses)
        {
            CreateExpenseTable(page, expense);
        }
    }

    private void CreateExpenseTable(Section page, Expense expense)
    {
        var table = CreateExpenseDetailTable(page);
        var row = table.AddRow();
        row.Height = HEIGHT_ROW_EXPENSE_TABLE;

        AddExpenseTitle(row.Cells[0], expense.Title);
        AddAmountHeader(row.Cells[3]);

        row = table.AddRow();
        row.Height = HEIGHT_ROW_EXPENSE_TABLE;


        var dateRow = row.Cells[0];
        dateRow.AddParagraph(expense.Date.ToString("dddd, MMM d, yyyy"));
        SetStyleBaseForExpenseInformation(dateRow);
        dateRow.Format.LeftIndent = 20;

        var hourRow = row.Cells[1];
        hourRow.AddParagraph(expense.Date.ToString("t"));
        SetStyleBaseForExpenseInformation(hourRow);

        var paymentTypeRow = row.Cells[2];
        paymentTypeRow.AddParagraph(expense.PaymentType.PaymentTypeToString());
        SetStyleBaseForExpenseInformation(paymentTypeRow);

        var amountExpenseRow = row.Cells[3];
        AddAmountForExpense(amountExpenseRow, expense.Amount);

        if (string.IsNullOrWhiteSpace(expense.Description) is false)
        {
            AddDescriptionRow(table, expense.Description, amountExpenseRow);
        }

        AddWhiteSpace(table);
    }

    private void AddExpenseTitle(Cell cell, string expenseTitle)
    {
        cell.AddParagraph(expenseTitle);
        cell.Format.Font = new Font
        {
            Name = FontHelper.RALEWAY_BLACK,
            Size = 14,
            Color = ColorsHelper.BLACK,
        };
        cell.Shading.Color = ColorsHelper.RED_LIGHT;
        cell.VerticalAlignment = VerticalAlignment.Center;
        cell.MergeRight = 2;
        cell.Format.LeftIndent = 20;
    }

    private void AddAmountHeader(Cell cell)
    {
        cell.AddParagraph(ResourceReportGenerationMessages.AMOUNT);
        cell.Format.Font = new Font
        {
            Name = FontHelper.RALEWAY_BLACK,
            Size = 14,
            Color = ColorsHelper.WHITE,
        };
        cell.Shading.Color = ColorsHelper.RED_DARK;
        cell.VerticalAlignment = VerticalAlignment.Center;
    }

    private void SetStyleBaseForExpenseInformation(Cell cell)
    {
        cell.Format.Font = new Font
        {
            Name = FontHelper.WORKSANS_REGULAR,
            Size = 12,
            Color = ColorsHelper.BLACK,
        };
        cell.Shading.Color = ColorsHelper.GREEN_DARK;
        cell.VerticalAlignment = VerticalAlignment.Center;
    }

    private void AddAmountForExpense(Cell cell, decimal expenseAmount)
    {
        cell.AddParagraph($"- {CURRENCY_SYMBOL}{expenseAmount}");
        cell.Format.Font = new Font
        {
            Name = FontHelper.WORKSANS_BLACK,
            Size = 14,
            Color = ColorsHelper.BLACK,
        };
        cell.Shading.Color = ColorsHelper.WHITE;
        cell.VerticalAlignment = VerticalAlignment.Center;
    }

    private void AddWhiteSpace(Table table)
    {

        var row = table.AddRow();
        row.Height = 30;
        row.Borders.Visible = false;
    }

    private void AddDescriptionRow(Table table, string expenseDescription, Cell cellToMerge)
    {
        var descriptionRow = table.AddRow();
        descriptionRow.Height = HEIGHT_ROW_EXPENSE_TABLE;

        descriptionRow.Cells[0].AddParagraph(expenseDescription);

        descriptionRow.Cells[0].Format.Font = new Font
        {
            Name = FontHelper.WORKSANS_REGULAR,
            Size = 10,
            Color = ColorsHelper.BLACK,
        };
        descriptionRow.Cells[0].Shading.Color = ColorsHelper.GREEN_LIGHT;
        descriptionRow.Cells[0].VerticalAlignment = VerticalAlignment.Center;
        descriptionRow.Cells[0].MergeRight = 2;
        descriptionRow.Cells[0].Format.LeftIndent = 20;

        cellToMerge.MergeDown = 1;
    }

    private byte[] RenderDocument(Document document)
    {
        var renderer = new PdfDocumentRenderer
        {
            Document = document,
        };

        renderer.RenderDocument();
        
        using var file = new MemoryStream();
        renderer.PdfDocument.Save(file);

        return file.ToArray();
    }


}
