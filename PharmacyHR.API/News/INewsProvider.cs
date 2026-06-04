namespace PharmacyHR.API.News
{
    public interface INewsProvider
    {
        Task<IList<NewsArticleResult>> FetchNewsAsync(string keyword);
    }
}
