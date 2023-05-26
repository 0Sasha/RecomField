using RecomField.Models;
namespace RecomField.Services;

public interface IReviewService<TReview>
{
    public Task<TReview> LoadReviewAsync(int reviewId, bool deep, string? userId);

    public Task AddReviewAsync(TReview review, int score, string[] tags);

    public Task EditReviewAsync(int reviewId, string title, string body, int score, string[] tags);

    public Task<TReview[]> GetReviewsAsync(string search, bool byTag = false);

    public Task<TReview[]> GetReviewsAsync(string userId, string? search);

    public Task<TReview[]> GetSimilarReviewsAsync(int reviewId, int count);

    public TReview[] SortReviews(IEnumerable<TReview> reviews, string sortBy, bool ascOrder);

    public Task<byte[]> GetPdfVersionAsync(int reviewId);

    public Task ChangeLikeAsync(int reviewId, string userId);

    public Task AddCommentAsync(int reviewId, string userId, string comment);

    public Task<string[]> GetReviewTagsAsync(string search, int count);
}
