using RecomField.Models;
namespace RecomField.Services;

public interface IReviewService<TReview>
{
    public Task<TReview[]> GetReviewsAsync(string search, bool byTag = false);

    public Task<TReview> LoadReviewAsync(int reviewId, bool deep, string? userId);

    public Task<byte[]> GetPdfVersionAsync(int reviewId);

    public Task ChangeLikeAsync(int reviewId, string userId);
}
