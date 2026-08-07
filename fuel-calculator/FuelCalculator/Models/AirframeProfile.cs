public class AirframeProfile : IAirframe
{
    public AirframeType Identity { get; }
    public double CapacityLbs { get; }
    public IReadOnlyDictionary<FlightPhase, double> FlowRates { get; }
    public IReadOnlyList<Capability> Capabilities { get; }

    public AirframeProfile(
        AirframeType identity,
        double capacityLbs,
        Dictionary<FlightPhase, double> flowRates,
        IReadOnlyList<Capability> capabilities)
    {
        if (capacityLbs <= 0)
        { throw new ArgumentException("Capacity must be a positive value.", nameof(capacityLbs)); }

        if (flowRates is null)
        { throw new ArgumentNullException(nameof(flowRates)); }

        if (capabilities is null)
        { throw new ArgumentNullException(nameof(capabilities)); }

        var missingPhases = Enum.GetValues<FlightPhase>().Except(flowRates.Keys).ToList();

        if (missingPhases.Any())
        {
            throw new ArgumentException(
                $"Flow rates missing for phase(s): {string.Join(", ", missingPhases)}",
                nameof(flowRates));
        }

        Identity = identity;
        CapacityLbs = capacityLbs;
        FlowRates = flowRates;
        Capabilities = capabilities;
    }
}