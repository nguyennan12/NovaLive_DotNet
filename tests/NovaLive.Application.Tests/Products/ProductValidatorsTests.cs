using FluentAssertions;
using NovaLive.Application.Common.Helpers;
using NovaLive.Application.UseCases.Products.Commands.CreateSpu;
using NovaLive.Application.UseCases.Products.Commands.UpdateSkuPrice;
using NovaLive.Application.UseCases.Products.Commands.UpdateSpu;
using NovaLive.Contracts.V1.Products;
using Xunit;

namespace NovaLive.Application.Tests.Products;

public class ProductValidatorsTests
{
    [Fact]
    public void CreateSpuCommandValidator_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var validator = new CreateSpuCommandValidator();
        var request = new CreateSpuRequest(
            Name: "Áo Polo Nam Cotton Cao Cấp",
            Description: "Chất liệu cotton thoáng mát",
            CategoryId: Guid.NewGuid(),
            Brand: "NovaFashion",
            ThumbnailUrl: "https://minio.novalive.vn/products/polo.jpg",
            AttributesConfigJson: "[{\"name\":\"Màu\",\"values\":[\"Đen\",\"Trắng\"]}]",
            Skus:
            [
                new CreateSkuDto("POLO-BLACK-M", "{\"Màu\":\"Đen\",\"Size\":\"M\"}", 250000m, 199000m, 200, 50, null),
                new CreateSkuDto("POLO-WHITE-L", "{\"Màu\":\"Trắng\",\"Size\":\"L\"}", 250000m, 199000m, 200, 30, null)
            ],
            Attributes:
            [
                new ProductAttributeDto("Chất liệu", "100% Cotton"),
                new ProductAttributeDto("Xuất xứ", "Việt Nam")
            ]);

        var command = new CreateSpuCommand(request);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateSpuCommandValidator_WithInvalidJson_ShouldFailValidation()
    {
        // Arrange
        var validator = new CreateSpuCommandValidator();
        var request = new CreateSpuRequest(
            Name: "Áo Polo Nam Cotton Cao Cấp",
            Description: "Mô tả",
            CategoryId: Guid.NewGuid(),
            Brand: "NovaFashion",
            ThumbnailUrl: "https://minio.novalive.vn/products/polo.jpg",
            AttributesConfigJson: "{invalid_json_format",
            Skus:
            [
                new CreateSkuDto("POLO-01", "{bad_attributes_json", 200000m, 150000m, 200, 10, null)
            ],
            Attributes: null);

        var command = new CreateSpuCommand(request);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("JSON hợp lệ"));
    }

    [Fact]
    public void CreateSpuCommandValidator_WithDuplicateSkuCodes_ShouldFailValidation()
    {
        // Arrange
        var validator = new CreateSpuCommandValidator();
        var request = new CreateSpuRequest(
            Name: "Áo Polo Nam Cotton Cao Cấp",
            Description: "Chất liệu cotton thoáng mát",
            CategoryId: Guid.NewGuid(),
            Brand: "NovaFashion",
            ThumbnailUrl: "https://minio.novalive.vn/products/polo.jpg",
            AttributesConfigJson: null,
            Skus:
            [
                new CreateSkuDto("DUPLICATE-SKU", "{\"Màu\":\"Đen\"}", 200000m, 150000m, 200, 50, null),
                new CreateSkuDto("DUPLICATE-SKU", "{\"Màu\":\"Trắng\"}", 200000m, 150000m, 200, 30, null)
            ],
            Attributes: null);

        var command = new CreateSpuCommand(request);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("trùng lặp"));
    }

    [Fact]
    public void CreateSpuCommandValidator_WithInvalidPriceOrWeight_ShouldFailValidation()
    {
        // Arrange
        var validator = new CreateSpuCommandValidator();
        var request = new CreateSpuRequest(
            Name: "Áo Polo Nam Cotton",
            Description: "Mô tả",
            CategoryId: Guid.NewGuid(),
            Brand: "Nova",
            ThumbnailUrl: "https://minio.novalive.vn/products/polo.jpg",
            AttributesConfigJson: null,
            Skus:
            [
                new CreateSkuDto("SKU-01", "{}", -100m, -50m, 0, -10, null)
            ],
            Attributes: null);

        var command = new CreateSpuCommand(request);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void UpdateSkuPriceCommandValidator_WithNegativePrice_ShouldFailValidation()
    {
        // Arrange
        var validator = new UpdateSkuPriceCommandValidator();
        var request = new UpdateSkuPriceRequest(-1000m, 200000m, 200, true);
        var command = new UpdateSkuPriceCommand(Guid.NewGuid(), request);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Request.SellPrice");
    }

    [Theory]
    [InlineData("Áo Polo Nam Cotton Cao Cấp", "ao-polo-nam-cotton-cao-cap")]
    [InlineData("Điện thoại iPhone 15 Pro Max 256GB - Đen", "dien-thoai-iphone-15-pro-max-256gb-den")]
    [InlineData("   Váy   Đầm Dạ Hội  ", "vay-dam-da-hoi")]
    public void SlugHelper_ShouldNormalizeVietnameseCorrectly(string input, string expected)
    {
        // Act
        var slug = SlugHelper.GenerateSlug(input);

        // Assert
        slug.Should().Be(expected);
    }
}
