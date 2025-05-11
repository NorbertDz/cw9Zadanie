using Tutorial9.Model;

namespace Tutorial9.Services;

public interface IWarehouseService
{
    Task<int> addNewProductToWarehouse(Warehouse warehouse);
}