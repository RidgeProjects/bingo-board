# Airframe Profile — Functional Design

## Purpose

The canonical definition of an airframe's fuel-relevant characteristics. This is reference data, not a calculation — it describes *what an airframe is capable of*, and other components consume it for different purposes.

## Consumers

| Consumer | Uses |
|---|---|
| Fuel Calculation Engine | Capacity, fuel flow rates by phase |
| Leg Library | Capability list, to filter which legs are offered for this airframe |
| Validation Engine | Bingo/joker thresholds, capability list (for capability-mismatch checks) |

## Required Information

An Airframe Profile must define:

- **Identity** — a name/type distinguishing this airframe from others (e.g. F/A-18C vs S-3)
- **Fuel capacity** — the maximum internal fuel the airframe can carry
- **Fuel flow rates by phase** — consumption rate for each distinct flight phase the Fuel Calculation Engine will need to look up (e.g. taxi, climb, cruise, tactical/combat, loiter, descent)
- **Bingo threshold** — the fuel level at which the mission is considered committed to return
- **Joker threshold** — the intermediate caution fuel level, above bingo, that prompts a decision point
- **Capability list** — the set of tactical/procedural capabilities this airframe has (e.g. carrier operations, probe refuelling, boom refuelling), used to gate which legs it can be assigned

## Business Rules

1. **Capacity must be a positive value.** An airframe with zero or undefined capacity cannot be used in a mission.
2. **Joker threshold must sit between bingo and full capacity.** Bingo is the hard floor; joker is the earlier warning above it. If joker isn't strictly greater than bingo, the profile is invalid.
3. **Flow rates must cover every phase the system's leg types can reference.** If a Computed leg needs a phase's flow rate and the airframe profile doesn't define one, that's a data-completeness failure to catch at profile-authoring time, not at calculation time.
4. **Capability list may be empty but must be explicit.** An airframe with no special capabilities (e.g. land-based, no refuelling receptacle) still has a defined (empty) list — absence of the field entirely is a data error, not "assume nothing."

## Open Questions for Product Decision

- **Units** — are fuel values expressed in absolute weight (lbs/kg) or percentage of capacity? This affects every other component that reads these numbers, so it should be settled once, here, rather than per-consumer.
- **Flow rate precision** — is one flow rate per phase sufficient, or do some airframes need finer distinction (e.g. military power vs afterburner within "combat" phase)?
- **Multiple loadout variants** — does a single airframe type need more than one profile depending on stores/configuration (e.g. clean vs loaded F/A-18C having different flow rates), or is that out of scope for now?

## Dependencies

None — this is foundational reference data that other components depend on, not the reverse.
