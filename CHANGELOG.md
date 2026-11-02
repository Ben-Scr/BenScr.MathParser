# Changelog

## 1.0.0 — prepared 2026-11-02

- Migrate MathParserCS to the BenScr.MathParser package/assembly/namespace.
- Preserve established math, session, customization, token and syntax-tree functionality.
- Retain .NET 9 and add .NET 10.
- Add positioned syntax errors, input/depth bounds and reliable callback exception behavior.
- Fix NaN propagation and nullable/compatible Value conversions.
- Remove host-specific console/process commands and mutable global lexer configuration; see MIGRATION.md.
- Include XML docs, unchanged license and logo, Portable PDB symbols, Source Link, locked tooling,
  regression/fuzz tests, API inventories, CI and isolated package verification.
- NuGet publication is intentionally a separate action.
