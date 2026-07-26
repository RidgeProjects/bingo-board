# Fuel Calculation Engine — Functional Design

## Purpose

Given a Mission (an Airframe plus an ordered sequence of Legs), calculate the fuel consumed by each leg and the fuel remaining at every point in the mission timeline. This is a **pure calculation** component — it produces numbers. It does not decide whether those numbers are acceptable; that's the Validation Engine's job.

## Scope

**In scope**
- Calculating fuel burned per leg
- Calculating fuel gained per leg (aerial refuelling)
- Producing a cumulative fuel timeline across the whole mission
- Reporting total mission fuel required

**Out of scope**
- Judging whether the mission is flyable (Validation Engine)
- Selecting which legs are available for an airframe (Leg Library's job, upstream of this)
- Persisting or displaying results (Persistence Layer / SPA)

## Inputs

| Input | Description |
|---|---|
| **Airframe** | Fuel capacity, fuel flow rates by phase, capability list |
| **Ordered Leg sequence** | The mission's legs in flight order, each carrying its fuel strategy and parameters |
| **Starting fuel** | The fuel load at engine start — may be full capacity or a partial/user-specified load |

## Outputs

| Output | Description |
|---|---|
| **Fuel timeline** | Fuel remaining at each point in the sequence: before leg 1, after leg 1, after leg 2, ... after the final leg |
| **Per-leg fuel delta** | How much fuel each individual leg consumed or added |
| **Total fuel required** | Sum of all consumption across the mission (ignoring any AAR gains), i.e. what you'd need with no tanking |
| **Fuel remaining at recovery** | The final value in the fuel timeline — what's left when the mission ends |

## Business Rules

1. **Two fuel strategies exist per leg, and every leg uses exactly one:**
   - **Fixed** — a static fuel cost, independent of route specifics (e.g. a Case III departure always costs the same regardless of how the rest of the mission is planned)
   - **Computed** — a fuel cost derived from the leg's own parameters (distance, altitude, speed) combined with the airframe's fuel flow rate for the relevant phase

2. **Legs are evaluated strictly in sequence.** Fuel remaining after leg N is the input fuel state for leg N+1. There is no reordering or optimization — the engine respects the mission designer's leg order exactly as built.

3. **Aerial refuelling legs add fuel rather than consume it.** The engine must support both directions (burn and gain) within the same timeline, not just subtraction.

4. **Fuel gained from an AAR leg cannot exceed airframe capacity.** If a tanking leg would push fuel above 100%, the engine caps the result at capacity rather than reporting a value that exceeds the tank.

5. **The engine does not clamp fuel at zero.** If a leg's consumption would take the running total below zero, the engine reports the true (negative) value. Suppressing or flooring it at zero would hide a planning problem from the Validation Engine — that judgement belongs downstream, not here.

6. **Starting fuel is a mission-level input, not always "full tanks."** The engine must accept a starting fuel value below capacity (e.g. a deliberately reduced load for weight/performance reasons) and calculate from that baseline.

## Edge Cases to Define Before Build

These are open questions for product decision, not yet answered by this spec:

- **Zero-duration/zero-distance legs** (e.g. an instantaneous "on-station" marker) — do these have a fuel cost at all, or are they purely structural/labelling legs with no burn?
- **Missing fuel flow data** — if a Computed leg needs a fuel flow rate for a flight phase the airframe profile doesn't define, what should happen? (Options: hard error and block calculation, or fall back to a default/cruise rate with a warning.)
- **Reserve/divert fuel** — is there a fuel amount that should be tracked separately as "untouchable" (e.g. fixed divert reserve), distinct from the bingo/joker *warning* thresholds already on the Airframe? This affects whether the engine needs a second reserved-fuel input.
- **Bolter/wave-off (failed recovery attempt)** — a real carrier-ops scenario where a missed approach costs extra fuel and adds a loop back into the pattern. Decide whether this is modelled now as a leg type, or deferred to a later version.

## Acceptance Criteria

- Given a mission with N legs, the engine returns a fuel timeline of N+1 values (starting fuel, then one value after each leg).
- Given a mix of Fixed and Computed legs, each leg's delta reflects its own strategy — a Fixed leg's cost does not change when its position or neighbouring legs change; a Computed leg's cost changes correctly when its own parameters change.
- Given an AAR leg, fuel increases in the timeline at that point, capped at airframe capacity even if the leg's nominal transfer amount would exceed it.
- Given a mission where cumulative consumption exceeds starting fuel, the engine returns a negative fuel-remaining value rather than clamping to zero.
- Given a starting fuel value below full capacity, all timeline values calculate from that reduced baseline, not from capacity.
- Total fuel required equals the sum of all consumption legs' deltas, independent of any AAR gains in the mission.

## Dependencies

- Requires a finalized **Airframe Profile** structure (capacity, per-phase flow rates) before fuel-flow lookups can be defined precisely.
- Requires each **Leg** to expose enough data to identify its strategy (Fixed vs Computed) and the relevant parameters — this engine consumes that data but doesn't define the Leg schema itself.
