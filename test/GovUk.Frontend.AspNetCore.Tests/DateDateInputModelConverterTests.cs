#pragma warning disable CS0618 // Type or member is obsolete
using System;
using Xunit;

namespace GovUk.Frontend.AspNetCore.Tests
{
    public class DateDateInputModelConverterTests
    {
        [Theory]
        [MemberData(nameof(CreateDateFromElementsData))]
        public void CreateDateFromComponents_ReturnsExpectedResult(
            Type modelType,
            DateOnly date,
            object expectedResult)
        {
            // Arrange
            var converter = new DateDateInputModelConverter();

            // Act
            var result = converter.CreateModelFromDate(modelType, date);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [MemberData(nameof(GetDateFromModelData))]
        public void GetDateFromModel_ReturnsExpectedResult(
           Type modelType,
           object model,
           DateOnly? expectedResult)
        {
            // Arrange
            var converter = new DateDateInputModelConverter();

            // Act
            var result = converter.GetDateFromModel(modelType, model);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        public static TheoryData<Type, DateOnly?, object> CreateDateFromElementsData { get; } = new()
        {
            { typeof(Date), new DateOnly(2020, 4, 1), new Date(2020, 4, 1) },
            { typeof(Date?), new DateOnly(2020, 4, 1), (Date?)new Date(2020, 4, 1) }
        };

        public static TheoryData<Type, object, DateOnly?> GetDateFromModelData { get; } = new()
        {
            { typeof(Date), new Date(2020, 4, 1), new DateOnly(2020, 4, 1) },
            { typeof(Date?), (Date?)new Date(2020, 4, 1), new Date(2020, 4, 1) },
        };
    }
}
