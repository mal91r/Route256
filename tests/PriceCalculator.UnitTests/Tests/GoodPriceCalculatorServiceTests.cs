using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Moq;
using Route256.PriceCalculator.Domain;
using Route256.PriceCalculator.Domain.Entities;
using Route256.PriceCalculator.Domain.Models.PriceCalculator;
using Route256.PriceCalculator.Domain.Separated;
using Route256.PriceCalculator.Domain.Services;
using Route256.PriceCalculator.Domain.Services.Interfaces;
using Xunit;

namespace PriceCalculator.UnitTests.Tests;

public class GoodPriceCalculatorServiceTests
{
    [Fact]
    public void GoodPriceCalculatorService_WhenGoodIdIsDefault_ShouldThrow()
    {
        // Arrange
        var repositoryMock = new Mock<IGoodsRepository>(MockBehavior.Strict);
        var serviceMock = new Mock<IPriceCalculatorService>(MockBehavior.Strict);
        var cut = new GoodPriceCalculatorService(repositoryMock.Object, serviceMock.Object);
        var goodId = default(int);
        var distance = 1m;

        // Act, Assert
        Assert.Throws<ArgumentException>(() => cut.CalculatePrice(goodId, distance));
    }

    [Fact]
    public void GoodPriceCalculatorService_WhenDistanceIsDefault_ShouldThrow()
    {
        // Arrange
        var repositoryMock = new Mock<IGoodsRepository>(MockBehavior.Strict);
        var serviceMock = new Mock<IPriceCalculatorService>(MockBehavior.Strict);
        var cut = new GoodPriceCalculatorService(repositoryMock.Object, serviceMock.Object);
        var goodId = 1;
        var distance = default(decimal);

        // Act, Assert
        Assert.Throws<ArgumentException>(() => cut.CalculatePrice(goodId, distance));
    }

    [Fact]
    public void GoodPriceCalculatorService_WhenGoodIdIsInvalid_ShouldThrow()
    {
        // Arrange
        var repositoryMock = new Mock<IGoodsRepository>(MockBehavior.Default);
        var serviceMock = new Mock<IPriceCalculatorService>(MockBehavior.Strict);
        var cut = new GoodPriceCalculatorService(repositoryMock.Object, serviceMock.Object);
        var goodId = -1;
        var distance = 1m;

        // Act, Assert
        Assert.Throws<NullReferenceException>(() => cut.CalculatePrice(goodId, distance));
    }

    [Theory]
    [MemberData(nameof(CalculateMemberData))]
    public void GoodPriceCalculatorService_WhenCalc_ShouldSuccess(
    GoodEntity entity,
    decimal distance,
    int expected)
    {
        // Arrange
        var options = new PriceCalculatorOptions
        {
            VolumeToPriceRatio = 1,
            WeightToPriceRatio = 1,
        };
        var goodsRepositoryMock = new Mock<IGoodsRepository>(MockBehavior.Strict);
        var storageRepositoryMock = new Mock<IStorageRepository>(MockBehavior.Strict);


        goodsRepositoryMock
            .Setup(x => x.Get(entity.Id))
            .Returns(entity);

        storageRepositoryMock
            .Setup(x => x.Save(It.IsAny<StorageEntity>()));

        var serviceMock = new PriceCalculatorService(options, goodsRepositoryMock.Object, storageRepositoryMock.Object);

        var cut = new GoodPriceCalculatorService(goodsRepositoryMock.Object, serviceMock);

        // Act
        var result = cut.CalculatePrice(entity.Id, distance);

        // Assert
        Assert.Equal(expected, result);
    }

    public static IEnumerable<object[]> CalculateMemberData => CalculateData();
    private static IEnumerable<object[]> CalculateData()
    {
        yield return new object[]
        {
            new GoodEntity("username", 1, 1000, 1, 1, 1, 1, 10), 1000, 1000
        };
    }
}