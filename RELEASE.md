# Release 1.0.0

The user subsequently requested publication. On 2026-11-02, the exact verified package and symbols were
submitted to NuGet under ben.scr. NuGet validation and indexing are pending.

- Package: https://www.nuget.org/packages/BenScr.MathParser/1.0.0
- Source commit: 50779d5071e0ef4c0ca9c147d0c097cf18d01e9f
- Immutable tag: packages/BenScr.MathParser/1.0.0
- GitHub release and exact artifacts: https://github.com/Ben-Scr/BenScr.MathParser/releases/tag/packages/BenScr.MathParser/1.0.0
- Successful Windows and Ubuntu CI: https://github.com/Ben-Scr/BenScr.MathParser/actions/runs/41497373390
- nupkg SHA-256: df0341e3cfd6c73a7cabdb99dc179ce78eee185642a32e33e23c2b61ea5434ea
- snupkg SHA-256: ca0f665b18a2299edb3411bcf2a68eb75d43dbde49df0b70492fd0b990bfa4ee

The 1.0.0 artifacts are frozen at that commit. Later documentation updates do not change the released
package or move its tag. The package's embedded README retains the wording from its verified candidate.
The generated release-manifest.json records exact artifact hashes, source commit, tag, tests and consumers.

## Historical verification and publishing procedure

The following describes the original candidate procedure. Do not republish or rebuild 1.0.0 for upload.
Future versions need updated metadata, their own tag and a fresh verification. From a tagged, clean checkout:

```powershell
pwsh -File tools/Verify.ps1 -ReleaseTag packages/BenScr.MathParser/1.0.0
$artifactDirectory = (Get-Content artifacts/latest-verification.txt).Trim()
# Supply a suitably scoped NuGet API key through NUGET_API_KEY, never in a committed file.
pwsh -File tools/Publish.ps1 -ArtifactDirectory $artifactDirectory
```

Publish.ps1 refuses dirty sources, mismatched tags/hashes, an unpublished source tag or an already-existing target ID/version.
It pushes only the exact named 1.0.0 package and its adjacent, hash-checked symbol package.
Do not rebuild after verification without verifying the new artifacts again.
The completed 1.0.0 submission used NuGet's web upload for both archives; no API key was created or stored.

The source tag packages/BenScr.MathParser/1.0.0 must point to the manifest commit in the public repository.
After package acceptance, create the corresponding GitHub release for that existing tag,
attaching the exact nupkg, snupkg and release-manifest.json. Never move a released tag.

## GitHub Trusted Publishing

The manually dispatched release.yml is ready to build, test and publish one specified release tag.
It is never triggered automatically by pushes or pull requests.

Before invoking it, configure a NuGet Trusted Publishing policy for profile ben.scr:
repository owner Ben-Scr, repository BenScr.MathParser, workflow file release.yml, no environment.
A policy/credential is an external publishing prerequisite; none is claimed to have been configured in this task.
The workflow obtains a short-lived key through NuGet/login and passes it only through the environment.
No additional approval environment is introduced.

For a future release, dispatch the workflow with that version's new tag. It rebuilds and fully verifies its
own artifacts before pushing those same bytes. Do not dispatch it for the already submitted 1.0.0.

## Verify the published package

NuGet submission and indexing are separate states. After indexing:

1. Use a new directory outside this repository and a new globalPackagesFolder.
2. Copy examples/Program.cs and create a console project targeting net9.0 and net10.0 with only
   PackageReference BenScr.MathParser Version="[1.0.0]".
3. Use a NuGet.Config with only https://api.nuget.org/v3/index.json, clear fallback folders,
   restore with --no-cache and the new package cache, and run both targets.
4. Check the fetched package's README, original license, icon hash, target assemblies and repository commit.
   NuGet signing may alter the ZIP hash; compare entry payloads and metadata with the tested local archive.
5. Download the matching portable PDBs from NuGet's symbol server, compare their bytes with the verified
   symbol archive and verify Source Link source checksums. Inspect the rendered NuGet icon. Record package
   and CI URLs, and change the release record to published_verified only after these checks succeed.
   Record interactive debugger stepping separately if it is performed.

Submission has been confirmed on the package page. Public-feed consumer verification is pending indexing.

The earlier v1.0.0 and nuget/v1.0.0 tags are preserved as validation candidates. Fresh CI runners found
that isolated consumer restore also needs Microsoft reference packs, including WindowsDesktop on Windows. The corrected release is tagged packages/BenScr.MathParser/1.0.0;
no published package or historical tag is replaced. The consumer source mapping confines BenScr.MathParser to
the verified local feed and permits only named Microsoft SDK packs from nuget.org.
