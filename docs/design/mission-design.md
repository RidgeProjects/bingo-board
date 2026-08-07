# Mission — Functional Design

## Purpose

The aggregate that ties everything together: an assigned airframe, an ordered sequence of legs, and the mission-specific fuel planning parameters that only make sense in context of *this* flight (starting fuel, bingo, joker). This is the thing a mission designer actually builds, saves, and runs calculations against — everything else in the system exists to support it.

## Consumers

| Consumer | Uses |
|---|---|
| Fuel Calculation Engine | Assigned airframe, ordered leg sequence, starting fuel |
| Validation Engine | Fuel timeline (from the engine) plus this mission's bingo/joker thresholds, to evaluate rules |
| Persistence Layer | The whole Mission as the unit of save/load and export/import |
| Fuel Planner SPA | Builds and displays the Mission — leg builder, timeline, results dashboard |

## Required Information

A Mission must define:

- **Identity** — a name/title for the mission, used for save/load and display
- **Assigned airframe** — a reference to a single Airframe Profile for this mission
- **Ordered leg sequence** — the legs that make up the mission, in flight order
- **Starting fuel** — the fuel load at mission start (may default to the assigned airframe's full capacity, or be set lower)
- **Bingo threshold** — this mission's committed-return fuel level
- **Joker threshold** — this mission's caution fuel level, above bingo

## Business Rules

1. **A mission has exactly one assigned airframe at a time.** Legs added to the mission must be compatible with that airframe's capabilities — enforced at selection time by the Leg Library offering only legal legs, and re-checked by the Validation Engine's capability rule.
2. **Joker must be strictly greater than bingo.** Both are mission-specific values entered or derived when the mission is built, not fixed defaults from the airframe.
3. **Starting fuel cannot exceed the assigned airframe's capacity.** A mission can start below capacity (reduced load) but never above it.
4. **Leg order is significant and user-controlled.** The Mission preserves the sequence as built; reordering legs is a valid edit operation and must trigger recalculation of the fuel timeline.
5. **An empty mission (no legs) is a valid draft state.** It can be saved and returned to, but is not a complete, exportable plan — see open question below on where that gate lives.

## Open Questions for Product Decision

- **Changing airframe after legs exist** — if a mission designer swaps the assigned airframe after legs are already added, and some of those legs are no longer compatible (capability mismatch), what happens? Options: block the airframe change until incompatible legs are removed, or allow the change and let those legs surface as validation errors.
- **Bingo/joker entry method** — direct manual entry by the mission designer, or computed from a divert-distance input plus the airframe's flow rate and a standard reserve policy? (Same open question carried over from the Airframe Profile doc — this is where it actually gets resolved.)
- **"Complete" vs "draft" state** — is there a formal distinction between a mission that's just a work-in-progress and one that's finished/exportable, or is "zero validation errors" the only signal of completeness? Affects whether Mission needs a status field at all.
- **Repeated leg types** — is a mission allowed to contain more than one leg of the same type (e.g. two separate AAR legs)? Assumed yes, but worth confirming explicitly since it affects how the Leg Library / builder UI treats "already used" legs.
- **Mission versioning** — does the system need to retain edit history, or is a Mission just current-state, fully overwritten on each save?
- **External fuel tanks** — not modelled. Internal capacity (`AirframeProfile.CapacityLbs`) reflects airframe-fixed internal fuel only. External tank capacity, drag/flow-rate impact, and jettison behaviour are mission/loadout-specific and need their own design once the loadout-variant question above is resolved.

## Dependencies

- **Airframe Profile** — a Mission references exactly one
- **Leg** — a Mission holds an ordered collection; each entry is either a Leg Library instance or (pending the open question in the Leg design doc) a custom-built leg
