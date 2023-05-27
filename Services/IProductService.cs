namespace RecomField.Services;

public interface IProductService<TProduct>
{
    public Task AddProductAsync(TProduct product);

    public Task<TProduct> LoadProductAsync(int productId, bool deep, string? userId);

    public Task<TProduct[]> GetProductsAsync(int count, string? search, IEnumerable<TProduct>? except);

    public Task ChangeUserScoreAsync(int productId, string userId, int newScore);

    public Task UpdateAverageScoresAsync(int productId);
}
