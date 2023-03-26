using Route256.PriceCalculator.Domain.Entities;
using Route256.PriceCalculator.Domain.Models.PriceCalculator;
using Route256.PriceCalculator.Domain.Separated;
using Route256.PriceCalculator.Domain.Services.Interfaces;

namespace Route256.PriceCalculator.Domain.Services;

internal sealed class GoodPriceCalculatorService : IGoodPriceCalculatorService
{
    private readonly IGoodsRepository _repository;
    private readonly IPriceCalculatorService _service;

    public GoodPriceCalculatorService(
        IGoodsRepository repository,
        IPriceCalculatorService service)
    {
        _repository = repository;
        _service = service;
    }

    public decimal CalculatePrice(
        int goodId,
        decimal distance)
    {
        if (isDefault(goodId))
            throw new ArgumentException($"{nameof(goodId)} is default");

        if (isDefault(distance))
            throw new ArgumentException($"{nameof(distance)} is default");

        var good = _repository.Get(goodId);
        var model = GoodToModel(good);

        return _service.CalculatePrice(new[] { model }) * distance;
    }

    private bool isDefault(decimal value)
    {
        return value == default;
    }

    private static GoodModel GoodToModel(GoodEntity x)
        => new(x.Height, x.Length, x.Width, x.Weight);
}