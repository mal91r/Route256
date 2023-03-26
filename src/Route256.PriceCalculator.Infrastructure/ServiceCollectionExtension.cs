using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Route256.PriceCalculator.Domain;
using Route256.PriceCalculator.Domain.Separated;
using Route256.PriceCalculator.Domain.Services.Interfaces;
using Route256.PriceCalculator.Domain.Services;
using Route256.PriceCalculator.Infrastructure.Dal.Repositories;
using Route256.PriceCalculator.Infrastructure.External;
using Microsoft.Extensions.Options;

namespace Route256.PriceCalculator.Infrastructure
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddDomain(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<PriceCalculatorOptions>(configuration.GetSection("PriceCalculatorOptions"));
            services.AddScoped<IPriceCalculatorService, PriceCalculatorService>(x =>
            {
                var options = x.GetRequiredService<IOptionsSnapshot<PriceCalculatorOptions>>().Value;
                return new PriceCalculatorService(
                    options,
                    x.GetRequiredService<IGoodsRepository>(),
                    x.GetRequiredService<IStorageRepository>()
                    );
            });
            return services;
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IGoodPriceCalculatorService, GoodPriceCalculatorService>();
            services.AddSingleton<IStorageRepository, StorageRepository>();
            services.AddSingleton<IGoodsRepository, GoodsRepository>();
            services.AddScoped<IGoodsService, GoodsService>();
            return services;
        }
    }
}
