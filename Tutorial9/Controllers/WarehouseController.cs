using Microsoft.AspNetCore.Mvc;
using Tutorial9.Model;
using Tutorial9.Services;

namespace Tutorial9.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;

    public WarehouseController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    [HttpPost("/api/product")]
    public async Task<IActionResult> addNewProductToWarehouse([FromBody] Warehouse warehouse)
    {
        try
        {
            int id = await _warehouseService.addNewProductToWarehouse(warehouse);
            return Ok(new
            {
                IdProductWarehouse = id
            });
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("nie istnieje"))
                return NotFound(ex.Message); 
            if (ex.Message.Contains("musi być większa"))
                return BadRequest(ex.Message); 
            if (ex.Message.Contains("już zrealizowane"))
                return Conflict(ex.Message); 
            return StatusCode(500, "Błąd serwera: " + ex.Message);
        }
    }
}