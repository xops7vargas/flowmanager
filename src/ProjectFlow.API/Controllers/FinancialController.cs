using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectFlow.Application.DTOs;
using ProjectFlow.Application.Interfaces;
using ProjectFlow.Domain.Enums;

namespace ProjectFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FinancialController : ControllerBase
{
    private readonly IFinancialService _financialService;

    public FinancialController(IFinancialService financialService)
    {
        _financialService = financialService;
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories([FromQuery] bool? isIncome = null)
    {
        var categories = await _financialService.GetCategoriesAsync(isIncome);
        return Ok(categories);
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateExpenseCategoryDto dto)
    {
        var category = await _financialService.CreateCategoryAsync(dto);
        return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, category);
    }

    [HttpPut("categories/{id:guid}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CreateExpenseCategoryDto dto)
    {
        var category = await _financialService.UpdateCategoryAsync(id, dto);
        return Ok(category);
    }

    [HttpDelete("categories/{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        await _financialService.DeleteCategoryAsync(id);
        return NoContent();
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? projectId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] TransactionType? type = null,
        [FromQuery] Guid? categoryId = null)
    {
        var result = await _financialService.GetTransactionsAsync(page, pageSize, projectId, startDate, endDate, type, categoryId);
        return Ok(result);
    }

    [HttpPost("transactions")]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateFinancialTransactionDto dto)
    {
        var userId = Guid.Parse(User.Identity.Name);
        var transaction = await _financialService.CreateTransactionAsync(dto, userId);
        return CreatedAtAction(nameof(GetTransactions), new { id = transaction.Id }, transaction);
    }

    [HttpPut("transactions/{id:guid}")]
    public async Task<IActionResult> UpdateTransaction(Guid id, [FromBody] CreateFinancialTransactionDto dto)
    {
        var transaction = await _financialService.UpdateTransactionAsync(id, dto);
        return Ok(transaction);
    }

    [HttpDelete("transactions/{id:guid}")]
    public async Task<IActionResult> DeleteTransaction(Guid id)
    {
        await _financialService.DeleteTransactionAsync(id);
        return NoContent();
    }

    [HttpGet("report")]
    public async Task<IActionResult> GetReport(
        [FromQuery] Guid? projectId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var report = await _financialService.GetReportAsync(projectId, startDate, endDate);
        return Ok(report);
    }
}
