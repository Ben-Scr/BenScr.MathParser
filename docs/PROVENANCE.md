# Provenance and inventory

Created by Ben Schneider — https://github.com/Ben-Scr

- Source repository: https://github.com/Ben-Scr/MathParser (public, owner Ben-Scr).
- Source branch: master; exact source commit: 2d35f9ec1c92913884e0151a9467393a4d88f535.
- Local source: existing MathParser checkout, clean at inspection. Its old origin URL was https://github.com/rrainix/Parser.git.
  GitHub metadata and git ls-remote verified the canonical repository and the identical master commit.
- Full reachable source history is retained in the new repository; the existing master branch is continued.
- Published input: MathParserCS 1.0.7, https://www.nuget.org/packages/MathParserCS/1.0.7.
- Live NuGet owner displayed: ben.scr. Package had only net9.0 and no runtime dependencies.
- Published nuspec records commit f0b521e3886e472f9d99fe8bcf8fbd0aaaf03324, but its repository URL incorrectly points to ko-fi.com/benscr.
  The project URL, live source inspection and actual binary API were used instead of trusting that repository field.
- Source contains the custom Ben Schneider 2025 attribution license. LICENSE.txt is retained unchanged;
  this package does not relabel it as MIT. The author attribution above satisfies the visible notice.
- No third-party implementation code, native binaries, submodules or runtime package dependencies are introduced.
- Tests/build use pinned NUnit 5.0.0, NUnit3TestAdapter 6.3.1 and Microsoft.NET.Test.Sdk 18.11.0.
  NUnit is retained because the existing behavior tests are useful; introducing a second framework would add no evidence.
- The SDK includes Source Link; no runtime Source Link dependency is added.
- Target repository/ID were checked before creation; no existing BenScr.MathParser repository or NuGet ID was found.
  Package availability is rechecked by the publication script; availability does not reserve an ID.

## Branding

The master PNG is the original BenScr brand asset, preserved unchanged.
SHA-256: a5dcabceb766e13c1840a95130ee98a458e12e3713fd9b07f3ceae75a1f5b993.
Dimensions 1254 × 1254, RGBA, 795819 bytes. The exact same bytes are packed as icon.png.
No asset resampling, cleanup, recoloring or generated replacement is used.
The QA page tools/IconPreview.html displays the packaged icon at 32 and 128 CSS pixels on light/dark backgrounds.

## Scope decision

This repository contains the C# expression parser family. BenScr.MathParser 1.0.0 is published on NuGet;
RELEASE.md records its immutable source commit and verification evidence. Legacy packages and source
repositories are preserved. WORK_LOG.md and execution-manifest.json track the release state.

## Technical references

- https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/sourcelink
- https://github.com/dotnet/sourcelink
- https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing
- https://learn.microsoft.com/en-us/nuget/reference/msbuild-targets

Visual QA completed on the actual packaged PNG at 32px and 128px on white and near-black backgrounds.
The silhouette, central opening and three separated elements remain legible; transparency is preserved.
Original edge texture is retained without cleanup. Byte/hash checks confirm the embedded asset is unchanged.
