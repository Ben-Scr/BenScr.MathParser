param([Parameter(Mandatory)][string]$ArtifactDirectory)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$projectRoot = Split-Path -Parent $PSScriptRoot
Push-Location $projectRoot
try {
    if (-not $env:NUGET_API_KEY) { throw 'Provide NUGET_API_KEY through the environment or NuGet Trusted Publishing.' }
    $manifest = Get-Content -Raw -LiteralPath (Join-Path $ArtifactDirectory 'release-manifest.json') | ConvertFrom-Json
    if ($manifest.status -ne 'ready_to_publish' -or $manifest.package_id -ne 'BenScr.MathParser' -or $manifest.version -ne '1.0.0') {
        throw 'Not a verified 1.0.0 release candidate.'
    }
    $commit = (& git rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0 -or $commit -ne $manifest.commit -or (& git status --porcelain)) { throw 'Source differs from verified commit.' }
    $tagCommit = & git rev-list -n 1 $manifest.release_tag
    if ($LASTEXITCODE -ne 0 -or $tagCommit -ne $commit) { throw 'Tag and verified commit differ.' }
    $remoteRefs = @(& git ls-remote 'https://github.com/Ben-Scr/BenScr.MathParser.git' "refs/tags/$($manifest.release_tag)" "refs/tags/$($manifest.release_tag)^{}")
    if ($LASTEXITCODE -ne 0 -or -not ($remoteRefs | Where-Object { $_.StartsWith($commit + [char]9) })) { throw 'Release tag is not published at the verified commit.' }
    foreach ($entry in @(@{ Name = $manifest.package_file; Hash = $manifest.package_sha256 }, @{ Name = $manifest.symbols_file; Hash = $manifest.symbols_sha256 })) {
        if ([IO.Path]::GetFileName($entry.Name) -ne $entry.Name) { throw 'Invalid artifact file name.' }
        $path = Join-Path $ArtifactDirectory $entry.Name
        if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() -ne $entry.Hash) { throw 'Artifact hash changed.' }
    }
    $indexUrl = 'https://api.nuget.org/v3-flatcontainer/benscr.mathparser/index.json'
    $response = Invoke-WebRequest -Uri $indexUrl -SkipHttpErrorCheck
    if ($response.StatusCode -eq 200) {
        $versions = ($response.Content | ConvertFrom-Json).versions
        if ($versions -contains $manifest.version) { throw 'Version already exists; inspect ownership/content instead of skipping duplicates.' }
        throw 'The package ID now exists; recheck ownership and version history before publishing this new package line.'
    }
    if ($response.StatusCode -ne 404) { throw "Cannot verify target availability: HTTP $($response.StatusCode)." }
    $package = Join-Path $ArtifactDirectory $manifest.package_file
    # NuGet automatically pushes the adjacent matching .snupkg as well.
    & dotnet nuget push $package --api-key $env:NUGET_API_KEY --source https://api.nuget.org/v3/index.json
    if ($LASTEXITCODE -ne 0) { throw 'NuGet push failed. Inspect server state before retrying.' }
    Write-Output 'Package submitted; indexing and published-consumer verification are still required. See RELEASE.md.'
} finally { Pop-Location }
