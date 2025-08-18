# Character Builder (MVP, 100 CP)

## Purpose
Create characters via **Point Buy (100 CP)**. Builder allows temporary invalid states; **Finalize()** enforces SR6e rules and produces a campaign-ready Character.

## Concepts
- **CharacterBuilder (AR)**: mutable draft; Status: InProgress|Finalized.
- **IBuildRuleSet (Strategy)**: MVP uses PointBuy100CP; later: Priority, etc.
- **Finalize(ruleSet)**: validates and emits Character or returns structured errors.

## Data Tracked
- Metadata (name, alias, metatype)
- Attributes, skills, qualities, powers/spells, gear/resources
- Budgets: **CP**, **Karma**, **Nuyen** (with separate CP vs Karma spend tracking)

## Finalization Rules (enforced)
- CP ≤ 100
- All Karma spent
- Starting Nuyen ≤ ¥5,000 remaining
- Attribute/Skill caps by metatype
- **Karma-after-CP** order for raises (prevent CP/Karma exploit)
- Essence ≥ 0; starting gear Availability ≤ 6

## UX (MVP)
- Use the same **ChatPanel** outside campaigns with the **Overseer** persona.
- You type intents (“Body 4, Pistols 3, Wired Reflexes 1”), Overseer issues commands; AR validates/updates; live totals returned.
- Finalize returns domain validation errors; Overseer renders them and can offer fix buttons that trigger commands.
