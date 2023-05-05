using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using RecomField.Models;
using RecomField.Data;
using Microsoft.EntityFrameworkCore;
namespace RecomField.Services;

public static class DataGenerator
{
    public static async Task GenerateProducts(ApplicationDbContext context)
    {
        var movies = System.IO.File.ReadAllLines("wwwroot/files/movies.txt");
        var series = System.IO.File.ReadAllLines("wwwroot/files/series.txt");
        var books = System.IO.File.ReadAllLines("wwwroot/files/books.txt");
        var games = System.IO.File.ReadAllLines("wwwroot/files/games.txt");
        for (int i = 0; i < movies.Length; i += 5)
        {
            if (!context.Movies.Any(m => m.Title == movies[i]))
            {
                Movie m = new()
                {
                    Title = movies[i],
                    ReleaseYear = int.Parse(movies[i + 1]),
                    Description = movies[i + 2],
                    Cover = movies[i + 3],
                    Trailer = movies[i + 4]
                };
                await context.Movies.AddAsync(m);
            }
        }
        for (int i = 0; i < series.Length; i += 5)
        {
            if (!context.Series.Any(s => s.Title == series[i]))
            {
                Series s = new()
                {
                    Title = series[i],
                    ReleaseYear = int.Parse(series[i + 1]),
                    Description = series[i + 2],
                    Cover = series[i + 3],
                    Trailer = series[i + 4]
                };
                await context.Series.AddAsync(s);
            }
        }
        for (int i = 0; i < games.Length; i += 5)
        {
            if (!context.Games.Any(g => g.Title == games[i]))
            {
                Game g = new()
                {
                    Title = games[i],
                    ReleaseYear = int.Parse(games[i + 1]),
                    Description = games[i + 2],
                    Cover = games[i + 3],
                    Trailer = games[i + 4]
                };
                await context.Games.AddAsync(g);
            }
        }
        for (int i = 0; i < books.Length; i += 5)
        {
            if (!context.Books.Any(g => g.Title == books[i]))
            {
                Book b = new()
                {
                    Title = books[i],
                    ReleaseYear = int.Parse(books[i + 1]),
                    Description = books[i + 2],
                    Cover = books[i + 3],
                    Author = books[i + 4]
                };
                await context.Books.AddAsync(b);
            }
        }
        await context.SaveChangesAsync();
    }

    public static async Task GenerateUsers(ApplicationDbContext context,
        UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore)
    {
        Random r = new();
        var names = System.IO.File.ReadAllLines("wwwroot/files/names.txt");
        var surnames = System.IO.File.ReadAllLines("wwwroot/files/surnames.txt");
        for (int i = 0; i < 0; i++)
        {
            var user = new ApplicationUser();
            var name = names[r.Next(names.Length)] + " " + surnames[r.Next(surnames.Length)];
            await userStore.SetUserNameAsync(user, name, CancellationToken.None);
            await userManager.CreateAsync(user);
        }
    }

    public static async Task GenerateReviews(ApplicationDbContext context)
    {
        for (int i = 1; i < 3; i++)
        {
            var rev = System.IO.File.ReadAllLines("wwwroot/files/reviews/" + i + ".txt");
            if (await context.Reviews.AnyAsync(r => r.Title == rev[1])) continue;
            var revBody = System.IO.File.ReadAllText("wwwroot/files/reviews/" + i + "body.txt");
            var prod = await context.Products.SingleAsync(p => p.Title == rev[0]);
            var author = await context.Users.FirstAsync(u => !u.Reviews.Any(r => r.ProductId == prod.Id));
            Review review = new()
            {
                Title = rev[1],
                Author = author,
                Product = prod,
                PublicationDate = DateTime.UtcNow,
                Body = revBody
            };
            review.Score = new(author, review, int.Parse(rev[3]));
            foreach (var tag in rev[2].Split(",")) review.Tags.Add(new(tag, review));
            await context.Reviews.AddAsync(review);
        }
        await context.SaveChangesAsync();
    }

    public static async Task GenerateScores(ApplicationDbContext context)
    {
        var scores = await context.ProductScores.Include(s => s.Sender).ToArrayAsync();
        context.ProductScores.RemoveRange(scores);
        await context.SaveChangesAsync();

        Random r = new();
        var users = await context.Users.ToArrayAsync();
        var prods = await context.Products.ToArrayAsync();
        foreach (var user in users)
        {
            var count = r.Next(5);
            List<Product> pr = new();
            while (count > 0)
            {
                var p = prods[r.Next(prods.Length)];
                if (!pr.Contains(p)) pr.Add(p);
                count--;
            }
            foreach (var p in pr)
            {
                await context.Entry(p).Collection(p => p.UserScores).LoadAsync();
                int score = r.Next(1, 11);
                if (score == 1) score = r.Next(1, 3);
                if (score < 10) score = r.Next(4, 5);
                else score = r.Next(3, 5);
                p.UserScores.Add(new(user, p, score));
            }
        }
        await context.SaveChangesAsync();
    }

    public static async Task GenerateLikes(ApplicationDbContext context)
    {
        var likes = await context.ReviewLikes.Include(s => s.Sender).Include(s => s.Entity).ToArrayAsync();
        context.ReviewLikes.RemoveRange(likes);
        await context.SaveChangesAsync();

        Random rand = new();
        var users = await context.Users.ToArrayAsync();
        var reviews = await context.Reviews.ToArrayAsync();
        foreach (var user in users)
        {
            var count = rand.Next(10);
            List<Review> selected = new();
            while (count > 0)
            {
                var rev = reviews[rand.Next(reviews.Length)];
                if (!selected.Contains(rev)) selected.Add(rev);
                count--;
            }
            foreach (var r in selected) r.Likes.Add(new(user, r));
        }
        await context.SaveChangesAsync();
    }

    public static async Task GenerateComments(ApplicationDbContext context)
    {
        Random rand = new();
        var comments = System.IO.File.ReadAllLines("wwwroot/files/comments.txt");
        var users = await context.Users.ToArrayAsync();
        var reviews = await context.Reviews.ToArrayAsync();
        for (int i = 0; i < reviews.Length; i++)
        {
            var count = rand.Next(0, 7);
            while (count > 0)
            {
                var user = users[rand.Next(users.Length)];
                var body = comments[rand.Next(comments.Length)];
                reviews[i].Comments.Add(new(user, reviews[i], body));
                count--;
            }
        }
        await context.SaveChangesAsync();
    }
}
