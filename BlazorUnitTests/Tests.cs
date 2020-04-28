﻿using AngleSharp.Dom;
using BlazorDemoApp.Components;
using BlazorDemoApp.Data;
using BlazorDemoApp.Pages;
using Bunit;
using Bunit.Mocking.JSInterop;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Telerik.Blazor.Components;
using Telerik.JustMock;
using Xunit;

namespace BlazorUnitTests
{
    public class Tests : ComponentTestFixture
    {
        [Fact]
        public void TestCounter()
        {
            // Arrange
            var cut = RenderComponent<Counter>();
            cut.Find("p").MarkupMatches("<p>Current count: 0</p>");

            // Act
            var element = cut.Find("button");
            element.Click();

            //Assert
            cut.Find("p").MarkupMatches("<p>Current count: 1</p>");
        }

        [Fact]
        public void TestFetchData_ForecastIsNull()
        {
            // Arrange
            var weatherForecastServiceMock = Mock.Create<IWeatherForecastService>();
            Mock.Arrange(() => weatherForecastServiceMock.GetForecastAsync(Arg.IsAny<DateTime>()))
                .Returns(new TaskCompletionSource<WeatherForecast[]>().Task);
            Services.AddSingleton<IWeatherForecastService>(weatherForecastServiceMock);

            // Act
            var cut = RenderComponent<FetchData>();

            // Assert - that it renders the initial loading message
            var initialExpectedHtml = 
                        @"<h1>Weather forecast</h1>
                        <p>This component demonstrates fetching data from a service.</p>
                        <p><em>Loading...</em></p>";
            cut.MarkupMatches(initialExpectedHtml);
        }

        [Fact]
        public void TestFetchData_PredefinedForecast()
        {
            // Arrange
            var forecasts = new[] { new WeatherForecast { Date = DateTime.Now, Summary = "Testy", TemperatureC = 42 } };

            var weatherForecastServiceMock = Mock.Create<IWeatherForecastService>();
            Mock.Arrange(() => weatherForecastServiceMock.GetForecastAsync(Arg.IsAny<DateTime>()))
                .Returns(Task.FromResult<WeatherForecast[]>(forecasts));

            Services.AddSingleton<IWeatherForecastService>(weatherForecastServiceMock);

            // Act - render the FetchData component
            var cut = RenderComponent<FetchData>();
            var actualForcastDataTable = cut.FindComponent<ForecastDataTable>(); // find the component

            // Assert
            var expectedDataTable = RenderComponent<ForecastDataTable>((nameof(ForecastDataTable.Forecasts), forecasts));
            actualForcastDataTable.MarkupMatches(expectedDataTable.Markup);
        }

        [Fact]
        public void TestMasterDetail_CorrectValues()
        {
            // Arrange
            var forecasts = new[] { new WeatherForecast { Date = DateTime.Now, Summary = "Testy", TemperatureC = 42 } };

            var weatherForecastServiceMock = Mock.Create<IWeatherForecastService>();
            Mock.Arrange(() => weatherForecastServiceMock.GetForecastAsync(Arg.IsAny<DateTime>()))
                .Returns(Task.FromResult<WeatherForecast[]>(forecasts));

            Services.AddMockJsRuntime();
            Services.AddSingleton<IWeatherForecastService>(weatherForecastServiceMock);
            Services.AddTelerikBlazor();

            var rootComponentMock = Mock.Create<TelerikRootComponent>();

            var cut = RenderComponent<MasterDetail>(
                CascadingValue(rootComponentMock)
            );
            
            // Act
            IElement plusSymbol = cut.Find("tr.k-master-row td[data-col-index=\"0\"]");
            plusSymbol.Click();

            // Assert
            var expectedForecastDetail = RenderComponent<WeatherForecastDetail>((nameof(WeatherForecastDetail.WeatherForecast), forecasts[0]));

            var actualForecastDetailElement = cut.FindComponent<WeatherForecastDetail>(); // find the component
            actualForecastDetailElement.MarkupMatches(expectedForecastDetail);
        }
    }
}
