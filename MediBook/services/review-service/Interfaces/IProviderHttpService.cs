namespace review_service.Interfaces
{
    public interface IProviderHttpService
    {
        Task UpdateProviderRatingAsync(int providerId, double avgRating);
    }
}
