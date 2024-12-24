namespace SP_Shopping.Hubs;

public interface IProductHubClient
{
    public Task NotifyChangeInProductWithId(int id);
}
