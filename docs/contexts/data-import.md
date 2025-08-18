# Data Import (PDF → Reference Tables)

## Purpose
Utility pipeline (not DDD). Parse SR6e PDFs into structured reference tables (skills, gear, spells, training, availability).

## Flow
- Upload PDFs → quick scan → staging by category → resolve conflicts → **Commit** (atomic).
- Provenance on every row: (pdfId, page range, importedAt).
- Sanity checks: uniqueness, numeric ranges, referential integrity.
- Rollback last batch.

## Usage
- Campaign Play and Character Builder **read** these tables; do not mutate them.
