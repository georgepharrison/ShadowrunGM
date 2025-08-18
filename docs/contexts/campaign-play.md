# Campaign Play (MVP)

## Purpose
Deliver the solo play loop with minimal UI: chat-first GM, mission hub, job selection, debrief, evolving world.

## Key Model Elements
- **Campaign (AR)**: Id, AutoName, PC, GMPCs[], GMProfile, WorldState, Jobs[], Contacts, Clocks[], News[].
- **Job**: Source (Contact|Faction|Plot), Type, Pay, Difficulty, PostedAt, **ExpiresInRuns**, Status.
- **Contact**: OwnerCharacterId (PC or GMPC), Role, Loyalty, Connection, Status (Active/Busy/Severed/Transferred/Hostile).
- **Advancement/Training**: Availability (Available/Training/Recovery/Dead); TrainingTask progress.

## Lifecycle
1. **Start Campaign**: auto-name, SR6e fixed; bind one PC; choose number of GMPCs; seed initial jobs via PC contacts.
2. **Mission Hub**: PC and teammates (with availability), contacts (PC + GMPCs), job board (expiring by runs), news, clocks, actions (spend Karma/¥, start training).
3. **Select Job → Run**: accept → lightweight briefing → chat-run.
4. **Debrief**: summary card; emit RunSummaryCreated; **Planning Lock** set.
5. **World Evolution**: background updates (jobs expire/post, rep/contacts/clocks/economy/news, teammate advancement proposals); lock lifted when done.

## Policies
- **Jobs expire by runs** (not calendar); expiration processed post-run.
- **GMPC contacts** unavailable when owner unavailable; on GMPC death: sever/transfer/hostile per policy.
- **GM options** editable; effects apply at safe boundaries (Immediate / Next Beat / Next Run).

## UX
- Chat-first; mini drawers for sheets/brief/news.
- SSE stream for evolution progress (“planning started/completed”).
