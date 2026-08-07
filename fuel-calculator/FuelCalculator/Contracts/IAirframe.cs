public interface IAirframe
{
    AirframeType Identity { get; }
    double CapacityLbs { get; }
    IReadOnlyDictionary<FlightPhase, double> FlowRates { get; }
    IReadOnlyList<Capability> Capabilities { get; }
}