using Microsoft.Data.SqlClient;
using Tutorial9.Model;

namespace Tutorial9.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IConfiguration _configuration;

    public WarehouseService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task<int> addNewProductToWarehouse(Warehouse warehouse)
    {
        string isProductExists = @"SELECT 1 FROM Product WHERE IdProduct = @id";
        string isWarehouseExists = @"SELECT 1 FROM Warehouse WHERE IdWarehouse = @id";
        string isOrderExists = @"Select * from [Order] where IdProduct = @id and Amount = @amount and CreatedAt < @createdAt";
        string isOrderRealised = @"SELECT 1 FROM Product_Warehouse WHERE IdOrder = @id";
        string updateFullfilledAt = @"UPDATE [Order] SET FulfilledAt = GETDATE() WHERE IdOrder = @id";
        
        
        string insertProduct_Warehouse = @"INSERT INTO Product_Warehouse (IdProduct,IdWarehouse, IdOrder, Amount,Price,CreatedAt) VALUES (@IdProduct,@IdWarehouse,@IdOrder,@Amount,@Price,GETDATE());SELECT SCOPE_IDENTITY();";
        string getPrice = @"Select 1 from Product where IdProduct = @id";
        string getIdOrder = @"Select 1 from [Order] where IdProduct = @id";
        
        
        using (var conn = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            await conn.OpenAsync();
            using (var idProductExistsCmd = new SqlCommand(isProductExists, conn))
            {
                idProductExistsCmd.Parameters.AddWithValue("@id", warehouse.IdProduct);
                var exists = await idProductExistsCmd.ExecuteScalarAsync();
                if (exists == null)
                    throw new Exception($"Produkt {warehouse.IdProduct} nie istnieje");
            }

            using (var isWarehouseExistsCmd = new SqlCommand(isWarehouseExists, conn))
            {
                isWarehouseExistsCmd.Parameters.AddWithValue("@id", warehouse.IdWarehouse);
                var exists = await isWarehouseExistsCmd.ExecuteScalarAsync();
                if (exists == null)
                    throw new Exception($"Warehouse {warehouse.IdWarehouse} nie istnieje");
            }
            
            if(!(warehouse.Amount >0))
                throw new Exception($"Ilość produktów musi byc większa od 0");

            using (var isOrderExistsCmd = new SqlCommand(isOrderExists, conn))
            {
                isOrderExistsCmd.Parameters.AddWithValue("@id", warehouse.IdProduct);
                isOrderExistsCmd.Parameters.AddWithValue("@amount", warehouse.Amount);
                isOrderExistsCmd.Parameters.AddWithValue("@createdAt", warehouse.CreatedAt);
                var exists = await isOrderExistsCmd.ExecuteScalarAsync();
                if (exists == null)
                    throw new Exception($"Nie ma takiego zamówienia lub podana ilość nie zgadza się z zamówieniem");
            }

            var idOrder = 0;
            using (var getIdOrderCmd = new SqlCommand(getIdOrder, conn))
            {
                getIdOrderCmd.Parameters.AddWithValue("@id", warehouse.IdProduct);
                idOrder =(int) await getIdOrderCmd.ExecuteScalarAsync();
            }
            
            using (var isOrderRealisedCmd = new SqlCommand(isOrderRealised, conn))
            {
                isOrderRealisedCmd.Parameters.AddWithValue("@id", idOrder);
                var exists = await isOrderRealisedCmd.ExecuteScalarAsync();
                if (exists == null)
                    throw new Exception("Zamówienie zostało zrealizowane");
            }

            using (var updateWarehouseCmd = new SqlCommand(updateFullfilledAt, conn))
            {
                updateWarehouseCmd.Parameters.AddWithValue("@id", warehouse.IdWarehouse);
                var exists = await updateWarehouseCmd.ExecuteScalarAsync();
                if (exists == null)
                    throw new Exception("Nie ma takiego id dla Warehouse");
            }

            int price=0;
            using (var getPriceCmd = new SqlCommand(getPrice, conn))
            {
                getPriceCmd.Parameters.AddWithValue("@id", warehouse.IdProduct);
                price = (int)await getPriceCmd.ExecuteScalarAsync();
            }


            
            
            using (var insertProduct_WarehouseCmd = new SqlCommand(insertProduct_Warehouse, conn))
            {   
                insertProduct_WarehouseCmd.Parameters.AddWithValue("@IdProduct", warehouse.IdProduct);
                insertProduct_WarehouseCmd.Parameters.AddWithValue("@IdWarehouse", warehouse.IdWarehouse);
                insertProduct_WarehouseCmd.Parameters.AddWithValue("@IdOrder", idOrder);
                insertProduct_WarehouseCmd.Parameters.AddWithValue("@Amount", warehouse.Amount);
                insertProduct_WarehouseCmd.Parameters.AddWithValue("@Price", (price*warehouse.Amount));
                
                var insertedId = await insertProduct_WarehouseCmd.ExecuteScalarAsync();
                int idProductWarehouse = Convert.ToInt32(insertedId);
                
                return idProductWarehouse;
            }
            
            
        }
    }
}