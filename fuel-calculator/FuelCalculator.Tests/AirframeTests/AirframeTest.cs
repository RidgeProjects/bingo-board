/// <summary>
/// Required Information
/// An Airframe Profile must define:
/// 
/// Identity — a name/type distinguishing this airframe from others (e.g. F/A-18C vs S-3)
/// Fuel capacity — the maximum internal fuel the airframe can carry
/// Fuel flow rates by phase — consumption rate for each distinct flight phase the Fuel Calculation Engine will need to look up (e.g. taxi, climb, cruise, tactical/combat, loiter, descent)
/// Capability list — the set of tactical/procedural capabilities this airframe has (e.g. carrier operations, probe refuelling, boom refuelling), used to gate which legs it can be assigned
/// Note: Bingo and joker fuel are not airframe characteristics — they're mission-specific. The same airframe has a different bingo number depending on divert distance and mission reserve requirements. These belong on the Mission (see Open Questions), using the airframe's fuel flow rate as one input to that calculation, not as a fixed value stored here.
/// 
/// Business Rules
/// Capacity must be a positive value. An airframe with zero or undefined capacity cannot be used in a mission.
/// Flow rates must cover every phase the system's leg types can reference. If a Computed leg needs a phase's flow rate and the airframe profile doesn't define one, that's a data-completeness failure to catch at profile-authoring time, not at calculation time.
/// Capability list may be empty but must be explicit. An airframe with no special capabilities (e.g. land-based, no refuelling receptacle) still has a defined (empty) list — absence of the field entirely is a data error, not "assume nothing."
/// </summary>
public class AirframeTest
{
    public class AirframeProfileTests
    {
        [Fact]
        public void Constructor_Throws_When_CapacityIsZero()
        {
            Assert.Throws<ArgumentException>(() => new AirframeProfile(AirframeType.F18, capacityLbs: 0, flowRates: new(), capabilities: ValidCapabilities()));
        }

        [Fact]
        public void Constructor_Throws_When_CapacityIsNegative()
        {
            Assert.Throws<ArgumentException>(() => new AirframeProfile(AirframeType.F18, capacityLbs: -100, flowRates: new(), capabilities: ValidCapabilities()));
        }

        // Rule 1: capacity must be positive
        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public void Constructor_Throws_When_CapacityIsNotPositive(double invalidCapacity)
        {
            Assert.Throws<ArgumentException>(() =>
                new AirframeProfile(AirframeType.F18, invalidCapacity, CompleteFlowRates(), new List<Capability>()));
        }

        [Fact]
        public void Constructor_Succeeds_When_CapacityIsPositive()
        {
            var profile = new AirframeProfile(AirframeType.F18, 10500, CompleteFlowRates(), new List<Capability>());
            Assert.Equal(10500, profile.CapacityLbs);
        }

        // Rule 2: flow rates must cover every phase
        [Fact]
        public void Constructor_Throws_When_FlowRates_MissingAPhase()
        {
            var incomplete = CompleteFlowRates();
            incomplete.Remove(FlightPhase.Loiter);

            var ex = Assert.Throws<ArgumentException>(() =>
                new AirframeProfile(AirframeType.F18, 10500, incomplete, new List<Capability>()));

            Assert.Contains("Loiter", ex.Message);
        }

        [Fact]
        public void Constructor_Throws_When_FlowRatesIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new AirframeProfile(AirframeType.F18, 10500, null, new List<Capability>()));
        }

        // Rule 3: capability list must be explicit (not null), empty is fine
        [Fact]
        public void Constructor_Throws_When_CapabilitiesIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new AirframeProfile(AirframeType.F18, 10500, CompleteFlowRates(), null));
        }

        [Fact]
        public void Constructor_Allows_EmptyCapabilityList()
        {
            var profile = new AirframeProfile(AirframeType.F18, 10500, CompleteFlowRates(), new List<Capability>());
            Assert.Empty(profile.Capabilities);
        }

        [Fact]
        public void Constructor_Succeeds_WithCapabilities()
        {
            var caps = new List<Capability> { Capability.CarrierOperations, Capability.ProbeRefuelling };
            var profile = new AirframeProfile(AirframeType.F18, 10500, CompleteFlowRates(), caps);
            Assert.Equal(caps, profile.Capabilities);
        }

        private static Dictionary<FlightPhase, double> CompleteFlowRates() =>
            new()
            {
            { FlightPhase.Taxi, 400 },
            { FlightPhase.Climb, 3000 },
            { FlightPhase.Cruise, 1200 },
            { FlightPhase.Tactical, 4500 },
            { FlightPhase.Loiter, 900 },
            { FlightPhase.Descent, 600 }
            };

        private IReadOnlyList<Capability> ValidCapabilities() =>
            new List<Capability>
            {
                Capability.ProbeRefuelling,
                Capability.CarrierOperations
            };

        private Dictionary<FlightPhase, double> ValidFlowRates() =>
            new Dictionary<FlightPhase, double>
            {
                { FlightPhase.Taxi, 400 },
                { FlightPhase.Climb, 3000 }
                // missing Cruise, Tactical, Loiter, Descent
            };
    }
}