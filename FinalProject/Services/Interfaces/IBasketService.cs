namespace FinalProject.Services.Implementations
{
    public interface IBasketService
    {
        public Task<List<BasketItemVM>> GetBasketAsync();
    }
}
