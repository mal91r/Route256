using Route256.PriceCalculator.Domain.Entities;
using Route256.PriceCalculator.Domain.Models.PriceCalculator;
using Route256.PriceCalculator.Domain.Separated;
using Route256.PriceCalculator.Domain.Services.Interfaces;

namespace Route256.PriceCalculator.Domain.Services;

internal sealed class PriceCalculatorService : IPriceCalculatorService
{
    private readonly decimal _volumeToPriceRatio;
    private readonly decimal _weightToPriceRatio;
    private readonly IGoodsRepository _repository;
    private const decimal DistanceToPriceRatio = 1.0m;

    private readonly IStorageRepository _storageRepository;

    public PriceCalculatorService(
        PriceCalculatorOptions options,
        IGoodsRepository repository,
        IStorageRepository storageRepository)
    {
        _volumeToPriceRatio = options.VolumeToPriceRatio;
        _weightToPriceRatio = options.WeightToPriceRatio;
        _storageRepository = storageRepository;
        _repository = repository;
    }

    public decimal CalculatePrice(IReadOnlyList<GoodModel> goods)
    {

        if (!goods.Any())
        {
            throw new ArgumentOutOfRangeException(nameof(goods));
        }

        var volumePrice = CalculatePriceByVolume(goods, out var volume);
        var weightPrice = CalculatePriceByWeight(goods, out var weight);

        var resultPrice = Math.Max(volumePrice, weightPrice);

        _storageRepository.Save(new StorageEntity(
            DateTime.UtcNow,
            volume,
            weight,
            0,
            resultPrice));

        return resultPrice;
    }

    public decimal CalculatePrice(CalculateRequest request)
    {
        var goods = request.Goods;

        if (!goods.Any())
        {
            throw new ArgumentOutOfRangeException(nameof(goods));
        }

        var volumePrice = CalculatePriceByVolume(goods, out var volume);
        var weightPrice = CalculatePriceByWeight(goods, out var weight);

        var resultPrice = ApplyDistanceToPrice(Math.Max(volumePrice, weightPrice), request.Distance);

        _storageRepository.Save(new StorageEntity(
            DateTime.UtcNow,
            volume,
            weight,
            request.Distance,
            resultPrice));

        return resultPrice;
    }


    private decimal CalculatePriceByVolume(
        IReadOnlyList<GoodModel> goods,
        out decimal volume)
    {
        volume = goods
            .Select(x => x.Height * x.Width * x.Length / 1000)
            .Sum();

        return volume * _volumeToPriceRatio;
    }

    private decimal CalculatePriceByWeight(
        IReadOnlyList<GoodModel> goods,
        out decimal weight)
    {
        weight = goods
            .Select(x => x.Weight / 1000)
            .Sum();

        return weight * _weightToPriceRatio;
    }

    public CalculationLogModel[] QueryLog(int take)
    {
        if (take == 0)
        {
            return Array.Empty<CalculationLogModel>();
        }

        var log = _storageRepository.Query()
            .OrderByDescending(x => x.At)
            .Take(take)
            .ToArray();

        return log
            .Select(x => new CalculationLogModel(
                x.Volume,
                x.Weight,
                x.Distance,
                x.Price))
            .ToArray();
    }

    private static decimal ApplyDistanceToPrice(decimal price, decimal distance)
    {
        if (distance == 0)
            return price;

        return price * distance * DistanceToPriceRatio;
    }
}