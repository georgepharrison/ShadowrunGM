# Context Map (text)

- **Data Import →** publishes reference tables (skills/gear/spells/training/availability).
- **Character Builder →** consumes reference tables; outputs a validated Character on Finalize.
- **Campaign Play →** consumes Characters + reference tables; owns runs, jobs, contacts, clocks, news.
- **World Evolution (inside Campaign Play)**: background worker triggered by Run Summary; updates jobs/rep/clocks/news; proposes teammate advancement.
- **User Preferences (global)** constrain GM behavior; campaign options cannot exceed globals.
