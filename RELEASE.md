# Release 1.0.0

This repository prepares BenScr.MathParser for publication. No NuGet package has been pushed by this task.
The generated release-manifest.json identifies exact artifact hashes, source commit, tag, tests and consumers.

## Verify and publish the existing artifacts

From the tagged, clean checkout:

```powershell
pwsh -File tools/Verify.ps1 -ReleaseTag packages/BenScr.MathParser/1.0.0
$artifactDirectory = (Get-Content artifacts/latest-verification.txt).Trim()
# Supply a suitably scoped NuGet API key through NUGET_API_KEY, never in a committed file.
pwsh -File tools/Publish.ps1 -ArtifactDirectory $artifactDirectory
```

Publish.ps1 refuses dirty sources, mismatched tags/hashes, an unpublished source tag or an already-existing target ID/version.
It pushes only the exact named 1.0.0 package and its adjacent, hash-checked symbol package.
Do not rebuild after verification without verifying the new artifacts again.
NuGet package IDs are not reserved by this preparation.

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

Dispatch the workflow with tag packages/BenScr.MathParser/1.0.0. It rebuilds and fully verifies its own artifacts before pushing those same bytes.

## Verify the published package

NuGet submission and indexing are separate states. After indexing:

1. Use a new directory outside this repository and a new globalPackagesFolder.
2. Copy examples/Program.cs and create a console project targeting net9.0 and net10.0 with only
   PackageReference BenScr.MathParser Version="[1.0.0]".
3. Use a NuGet.Config with only https://api.nuget.org/v3/index.json, clear fallback folders,
   restore with --no-cache and the new package cache, and run both targets.
4. Check the fetched package's README, original license, icon hash, target assemblies and repository commit.
   NuGet signing may alter the ZIP hash; compare entry payloads and metadata with the tested local archive.
5. Verify source stepping and the rendered NuGet icon, then record package URL and CI URL and change the
   release record to published_verified only after these checks succeed.

Expected future package page: https://www.nuget.org/packages/BenScr.MathParser/1.0.0.
It is not evidence of an existing publication.

The earlier v1.0.0 and nuget/v1.0.0 tags are preserved as validation candidates. Fresh CI runners found
that isolated consumer restore also needs Microsoft reference packs, including WindowsDesktop on Windows. The corrected release is tagged packages/BenScr.MathParser/1.0.0;
no published package or historical tag is replaced. The consumer source mapping confines BenScr.MathParser to
the verified local feed and permits only named Microsoft SDK packs from nuget.org.
