# Work log — 2026-11-02

Scope: one C# package, ready to publish; NuGet publication is deferred by the user's latest instruction.

Completed inventory: verified Ben-Scr GitHub identity and source repository ownership, live MathParserCS 1.0.7 owner,
actual legacy DLL API, source master/commit, .NET target, dependencies and license.
Created the family checkout by cloning existing master with its history; no new branch was created.

Implementation: migrated to the BenScr.MathParser identity, preserved the useful NUnit suite, added net10.0 beside net9.0.
Added bounded parsing and tree validation, positioned exceptions, callback failure propagation, immutable lexer settings,
and fixes for NaN/Value conversions. All legacy APIs are mapped in MIGRATION.md and docs/api-migration.json.

Initial behavior run: 101 tests passed on each of net9.0 and net10.0, none skipped.
This includes fixed-seed 5,000-input fuzzing, 1,000 independent arithmetic cases and concurrent calls per framework.
The first format pass exposed an SDK formatter conflict on multi-target source; corrected explicitly and builds pass with zero warnings.

Release verification is recorded in generated artifacts/verification-*/release-manifest.json, package-audit.json,
TRX files and isolated consumer output. These generated records contain the exact final commit and artifact hashes,
so they are intentionally outside their own source commit. artifacts/latest-verification.txt identifies the latest completed run.
Remote CI results and final source-link reachability are recorded in the final handoff, separately from configuration.

User correction: AGENTS.md lives at the BenScr parent root. Removed per-library agent/convention duplicates.
Solutions are ignored; all verification uses tracked .csproj files directly. Replaced the broad legacy gitignore with focused rules.

Archive audit passed for both frameworks: identity/version/commit, no runtime dependencies, README/license,
unchanged PNG, XML documentation, matching assembly/PDB identities and source checksums.
Icon visual QA passed at 32/128 CSS pixels on light/dark backgrounds using the extracted package image.

Full pre-commit verification completed: both targets, 101/101 tests each, API checks, examples, archive checks,
and consumers in an external temporary directory with a new cache and a local-only NuGet feed.
The final committed candidate is reverified independently and receives its own release manifest and hashes.
