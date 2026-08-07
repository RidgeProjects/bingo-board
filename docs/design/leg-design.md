# Leg — Functional Design

## Purpose

The atomic building block of a mission. A Leg represents one segment of flight — navigation, tanking, departure, recovery, holding — and carries everything needed to determine its fuel impact. Legs are airframe-agnostic; the same Leg definition can be offered to any airframe whose capabilities satisfy its requirements.

## Consumers

| Consumer | Uses |
|---|---|
| Fuel Calculation Engine | Fuel strategy and its parameters, to compute this leg's fuel delta |
| Leg Library | The reusable template shape — stores and offers predefined Legs |
| Validation Engine | Leg type/identity, for sequence rules (e.g. recovery can't precede departure) and capability matching |
| Fuel Planner SPA | Display name/description, for the leg builder UI |

## Required Information

A Leg must define:

- **Type/identity** — what kind of leg this is (waypoint navigation, AAR, Case I/III departure, Case I/III recovery, holding, loiter, etc.)
- **Display name and description** — human-readable label for the leg builder and mission timeline UI
- **Fuel strategy** — exactly one of:
  - **Fixed** — a static fuel cost, independent of route specifics
  - **Computed** — a fuel cost derived from this leg's own parameters and the assigned airframe's flow rate
- **Fuel direction** — whether this leg consumes fuel or adds it (only aerial refuelling legs add; everything else consumes)
- **Strategy-specific parameters**:
  - Fixed legs carry a single static fuel value
  - Computed legs carry the inputs needed for calculation (e.g. distance, altitude, speed, and which flight phase's flow rate applies)
- **Required capabilities** — which airframe capabilities must be present for this leg to be legal (e.g. a Case I recovery requires carrier operations capability)

## Business Rules

1. **A leg has exactly one fuel strategy — never both, never neither.** Fixed and Computed are mutually exclusive; a leg can't be "sometimes fixed, sometimes computed."
2. **The parameters a leg carries are determined by its strategy.** A Fixed leg doesn't need distance/speed/altitude; a Computed leg doesn't need (and shouldn't carry) a static fuel value. Carrying the wrong parameter set for the declared strategy is a data error.
3. **Fuel direction is intrinsic to leg type, not user-configurable per instance.** An AAR leg always adds fuel; every other current leg type always consumes. This shouldn't be a free toggle when a mission designer builds a leg — it's a property of the leg type itself.
4. **Required capabilities may be empty but must be explicit.** A generic waypoint navigation leg has no special requirement (empty list); that's different from the field being undefined.
5. **A Leg definition in the Library is a template, not a mission instance.** The same "Case III Recovery" template can be placed into many different missions; placing it doesn't consume or alter the template itself.

## Open Questions for Product Decision

- **Refuelling amount** — does an AAR leg always top off to full capacity, or can it specify a partial transfer (e.g. "take on 2,000 lbs" rather than "fill up")? This affects what parameters an AAR leg needs to carry.
- **Multi-phase legs** — can a single Computed leg span more than one flight phase (e.g. a long nav leg that includes both climb and cruise), or is one leg always one phase, with multi-phase routes built from multiple legs chained together?
- **Custom/ad-hoc legs** — beyond the predefined Library templates, can a mission designer build a one-off Computed leg from scratch within a mission, or must every leg originate from the Library?

## Dependencies

- Depends on **Airframe Profile** only indirectly — a Leg's `RequiredCapabilities` are checked against an Airframe's capability list elsewhere (Leg Library, Validation Engine); the Leg itself doesn't reference a specific airframe.
