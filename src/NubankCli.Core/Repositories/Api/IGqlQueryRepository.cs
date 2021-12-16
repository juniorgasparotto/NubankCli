namespace NubankSharp.Repositories.Api
{
    public interface IGqlQueryRepository
    {
        string GetGql(string queryName);
    }
}