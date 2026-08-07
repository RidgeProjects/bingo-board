# Airframe Profile — Functional Design

## Purpose

The canonical definition of an airframe's fuel-relevant characteristics. This is reference data, not a calculation — it describes *what an airframe is capable of*, and other components consume it for different purposes.

## Consumers

| Consumer | Uses |
|---|---|
| Fuel Calculation Engine | Capacity, fuel flow rates by phase |
| Leg Library | Capability list, to filter which legs are offered for this airframe |
| Validation Engine | Capability list (for capability-mismatch checks). Bingo/joker thresholds now come from the Mission, not this profile. |

## Required Information

An Airframe Profile must define:

- **Identity** — a name/type distinguishing this airframe from others (e.g. F/A-18C vs S-3)
- **Fuel capacity** — the maximum internal fuel the airframe can carry
- **Fuel flow rates by phase** — consumption rate for each distinct flight phase the Fuel Calculation Engine will need to look up (e.g. taxi, climb, cruise, tactical/combat, loiter, descent)
- **Capability list** — the set of tactical/procedural capabilities this airframe has (e.g. carrier operations, probe refuelling, boom refuelling), used to gate which legs it can be assigned

> **Note:** Bingo and joker fuel are *not* airframe characteristics — they're mission-specific. The same airframe has a different bingo number depending on divert distance and mission reserve requirements. These belong on the Mission (see Open Questions), using the airframe's fuel flow rate as one input to that calculation, not as a fixed value stored here.

## Business Rules

1. **Capacity must be a positive value.** An airframe with zero or undefined capacity cannot be used in a mission.
2. **Flow rates must cover every phase the system's leg types can reference.** If a Computed leg needs a phase's flow rate and the airframe profile doesn't define one, that's a data-completeness failure to catch at profile-authoring time, not at calculation time.
3. **Capability list may be empty but must be explicit.** An airframe with no special capabilities (e.g. land-based, no refuelling receptacle) still has a defined (empty) list — absence of the field entirely is a data error, not "assume nothing."

## Open Questions for Product Decision

- **Bingo/joker fuel calculation** — now that these live at the Mission level, how are they derived? Options: mission designer enters them directly as a planning decision, or the system computes them from a divert-distance input plus the assigned airframe's flow rate plus a standard reserve policy. This needs its own decision before the Mission entity can be spec'd.
- **Units** — are fuel values expressed in absolute weight (lbs/kg) or percentage of capacity? This affects every other component that reads these numbers, so it should be settled once, here, rather than per-consumer.
- **Flow rate precision** — is one flow rate per phase sufficient, or do some airframes need finer distinction (e.g. military power vs afterburner within "combat" phase)?
- **Multiple loadout variants** — does a single airframe type need more than one profile depending on stores/configuration (e.g. clean vs loaded F/A-18C having different flow rates), or is that out of scope for now?

## Dependencies

None — this is foundational reference data that other components depend on, not the reverse.
