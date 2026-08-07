// [Fact]
// public void Engine_Throws_When_Airframe_Missing_RequiredPhase()
// {
//     var mockAirframe = new Mock<IAirframe>();
//     mockAirframe.Setup(a => a.FlowRates).Returns(new Dictionary<FlightPhase, double>());

//     var engine = new FuelCalculationEngine();

//     Assert.Throws<InvalidOperationException>(() =>
//         engine.CalculateLeg(mockAirframe.Object, FlightPhase.Cruise, distanceNm: 200));
// }