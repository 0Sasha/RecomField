﻿using Microsoft.EntityFrameworkCore;
using RecomField.Data;
using RecomField.Models;
namespace RecomField.Services;

public class ProductService : IProductService<Product>
{
    private readonly ApplicationDbContext context;

    public ProductService(ApplicationDbContext context) => this.context = context;

    public async Task AddProductAsync(Product product)
    {
        if (await CheckExistenceAsync(product))
            throw new ArgumentException("This product already exists in the database", nameof(product));
        await context.AddAsync(product);
        await context.SaveChangesAsync();
    }

    public async Task<Product> LoadProductAsync(int productId, bool deep, string? userId)
    {
        var prod = await GetProductAsync(productId);
        if (userId != null)
            await context.ProductScores.SingleOrDefaultAsync(s => s.SenderId == userId && s.EntityId == productId);
        if (deep) await context.Reviews.Where(r => r.ProductId == productId)
                .Include(r => r.Score).Include(r => r.Author).LoadAsync();
        return prod;
    }

    public async Task<Product[]> GetProductsAsync(int count, string? search, IEnumerable<Product>? except)
    {
        except ??= Array.Empty<Product>();
        if (string.IsNullOrEmpty(search)) return await context.Products.Except(except).Take(count).ToArrayAsync();
        
        var request = "\"" + search + "*\" OR \"" + search + "\"";
        var prods = (await context.Products.Where(x => EF.Functions.Contains(x.Title, request) ||
        EF.Functions.Contains(x.Description, request)).ToArrayAsync()).Except(except).Take(count).ToArray();
        if (prods.Length == count) return prods;

        var byAuthor = await context.Books.Where(p => EF.Functions.Contains(p.Author, request)).ToArrayAsync();
        var byYear = int.TryParse(search, out var number) ? await context.Products
            .Where(p => p.ReleaseYear == number).ToArrayAsync() : Array.Empty<Product>();
        return prods.Union(byAuthor).Union(byYear).Except(except).Take(count).ToArray();
    }

    public async Task ChangeUserScoreAsync(int productId, string userId, int newScore)
    {
        if (newScore < 1 || newScore > 5) throw new Exception("Score must be between 1 and 5");
        var prod = await GetProductAsync(productId);
        await context.Entry(prod).Collection(p => p.UserScores).LoadAsync();
        var s = prod.UserScores.SingleOrDefault(s => s.SenderId == userId);
        if (s != null) s.Value = newScore;
        else prod.UserScores.Add(new(await context.Users.FindAsync(userId) ??
            throw new Exception("User is not found"), prod, newScore));
        prod.AverageUserScore = Math.Round(prod.UserScores.Select(s => s.Value).Average(), 2);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAverageScoresAsync(int productId)
    {
        var prod = await GetProductAsync(productId);
        await context.Entry(prod).Collection(p => p.UserScores).LoadAsync();
        await context.Reviews.Where(r => r.ProductId == productId).Include(r => r.Score).LoadAsync();
        prod.AverageUserScore =
            prod.UserScores.Count > 0 ? Math.Round(prod.UserScores.Select(s => s.Value).Average(), 2) : 0;
        prod.AverageReviewScore =
            prod.Reviews.Count > 0 ? Math.Round(prod.Reviews.Select(s => s.Score.Value).Average(), 2) : 0;
        await context.SaveChangesAsync();
    }

    private async Task<bool> CheckExistenceAsync(Product product)
    {
        var prods = await context.Products
            .Where(p => p.Title == product.Title && p.ReleaseYear == product.ReleaseYear).ToArrayAsync();
        return prods.Any(p => p.GetType() == product.GetType());
    }

    private async Task<Product> GetProductAsync(int productId) =>
        await context.Products.FindAsync(productId) ?? throw new Exception("Product is not found");
}
