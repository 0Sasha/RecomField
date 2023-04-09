namespace RecomField.Models;

public class Movie : Product
{
    public Movie(string title, int releaseYear) : base(ProductType.Movie, title, releaseYear)
    {

    }
}
