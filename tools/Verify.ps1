param([string]$ReleaseTag, [switch]$AllowDirty)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$projectRoot = Split-Path -Parent $PSScriptRoot
Push-Location $projectRoot
function Invoke-DotNet {
    & dotnet @args
    if ($LASTEXITCODE -ne 0) { throw "dotnet failed: $($args -join ' ')" }
}
try {
    $commit = (& git rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0) { throw 'Cannot resolve Git commit.' }
    if (-not $AllowDirty -and (& git status --porcelain)) { throw 'Release verification requires a clean checkout.' }
    if ($ReleaseTag) {
        $tagCommit = & git rev-list -n 1 $ReleaseTag
        if ($LASTEXITCODE -ne 0 -or $tagCommit -ne $commit) { throw 'Release tag does not identify HEAD.' }
        if ($ReleaseTag -ne 'packages/BenScr.MathParser/1.0.0') { throw 'Tag does not match package version.' }
    }
    if (-not (Test-Path -LiteralPath 'docs/PublicApi.txt')) { throw 'Reviewed public API baseline is missing.' }
    $runId = [Guid]::NewGuid().ToString('N')
    $output = Join-Path $projectRoot "artifacts/verification-$runId"
    New-Item -ItemType Directory -Path $output -Force | Out-Null
    foreach ($project in @('src/BenScr.MathParser.csproj', 'tests/BenScr.MathParser.Tests.csproj', 'examples/Calculator.csproj', 'tools/PackageAudit/PackageAudit.csproj')) {
        Invoke-DotNet restore $project --locked-mode
        Invoke-DotNet build $project -c Release --no-restore
        Invoke-DotNet format whitespace $project --no-restore --verify-no-changes
        Invoke-DotNet format style $project --no-restore --verify-no-changes --severity error
    }
    Invoke-DotNet test tests/BenScr.MathParser.Tests.csproj -c Release --no-build --no-restore --results-directory $output --logger 'trx;LogFilePrefix=tests'
    foreach ($framework in @('net9.0', 'net10.0')) {
        Invoke-DotNet run --project tools/PackageAudit -c Release --no-build -- "src/bin/Release/$framework/BenScr.MathParser.dll" docs/PublicApi.txt
        Invoke-DotNet run --project examples/Calculator.csproj -f $framework -c Release --no-build
    }
    Invoke-DotNet pack src/BenScr.MathParser.csproj -c Release --no-build --no-restore -o $output
    $package = Join-Path $output 'BenScr.MathParser.1.0.0.nupkg'
    $symbols = Join-Path $output 'BenScr.MathParser.1.0.0.snupkg'
    Invoke-DotNet run --project tools/PackageAudit -c Release --no-build -- package $package $symbols $commit $projectRoot

    # The library comes only from the local feed. A fresh cache may need Microsoft's SDK reference packs.
    $consumerRoot = Join-Path ([IO.Path]::GetTempPath()) "benscr-mathparser-$runId"
    New-Item -ItemType Directory -Path $consumerRoot -Force | Out-Null
    Copy-Item -LiteralPath global.json -Destination $consumerRoot
    Copy-Item -LiteralPath examples/Program.cs -Destination $consumerRoot
    $escapedFeed = [Security.SecurityElement]::Escape($output)
    $escapedCache = [Security.SecurityElement]::Escape((Join-Path $consumerRoot 'packages'))
    @"
<configuration>
  <packageSources>
    <clear />
    <add key="verified-local" value="$escapedFeed" />
    <add key="sdk-packs" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="verified-local"><package pattern="BenScr.MathParser" /></packageSource>
    <packageSource key="sdk-packs">
      <package pattern="Microsoft.NETCore.App.Ref" />
      <package pattern="Microsoft.AspNetCore.App.Ref" />
      <package pattern="Microsoft.WindowsDesktop.App.Ref" />
      <package pattern="Microsoft.NETCore.App.Host.*" />
    </packageSource>
  </packageSourceMapping>
  <fallbackPackageFolders><clear /></fallbackPackageFolders>
  <config><add key="globalPackagesFolder" value="$escapedCache" /></config>
</configuration>
"@ | Set-Content -LiteralPath (Join-Path $consumerRoot 'NuGet.Config') -Encoding utf8
    @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType><TargetFrameworks>net9.0;net10.0</TargetFrameworks>
    <ImplicitUsings>disable</ImplicitUsings><Nullable>enable</Nullable>
    <RestoreFallbackFolders></RestoreFallbackFolders><NuGetAudit>false</NuGetAudit>
  </PropertyGroup>
  <ItemGroup><PackageReference Include="BenScr.MathParser" Version="[1.0.0]" /></ItemGroup>
</Project>
'@ | Set-Content -LiteralPath (Join-Path $consumerRoot 'Consumer.csproj') -Encoding utf8
    Push-Location $consumerRoot
    try {
        Invoke-DotNet restore Consumer.csproj --configfile NuGet.Config --no-cache
        foreach ($framework in @('net9.0', 'net10.0')) {
            Invoke-DotNet run --project Consumer.csproj -f $framework -c Release --no-restore
        }
    } finally { Pop-Location }

    $testReports = @(Get-ChildItem -LiteralPath $output -Filter '*.trx' | ForEach-Object {
        [xml]$report = Get-Content -Raw -LiteralPath $_.FullName
        $counters = $report.TestRun.ResultSummary.Counters
        if ([int]$counters.total -le 0 -or [int]$counters.passed -ne [int]$counters.total) { throw 'Incomplete test execution.' }
        [ordered]@{ file = $_.Name; total = [int]$counters.total; passed = [int]$counters.passed; failed = [int]$counters.failed }
    })
    if ($testReports.Count -ne 2) { throw 'Expected test reports for both frameworks.' }
    $audit = Get-Content -Raw -LiteralPath (Join-Path $output 'package-audit.json') | ConvertFrom-Json
    $manifest = [ordered]@{
        schema_version = 1
        status = $(if ($AllowDirty) { 'tested_local_dirty' } elseif ($ReleaseTag) { 'ready_to_publish' } else { 'tested_local' })
        verified_at_utc = [DateTime]::UtcNow.ToString('o')
        repository = 'https://github.com/Ben-Scr/BenScr.MathParser'
        commit = $commit
        release_tag = $ReleaseTag
        package_id = $audit.package_id
        version = $audit.version
        package_file = 'BenScr.MathParser.1.0.0.nupkg'
        package_sha256 = $audit.package_sha256
        symbols_file = 'BenScr.MathParser.1.0.0.snupkg'
        symbols_sha256 = $audit.symbols_sha256
        tests = $testReports
        consumer_frameworks = @('net9.0', 'net10.0')
        consumer_directory = $consumerRoot
        platform = [System.Runtime.InteropServices.RuntimeInformation]::OSDescription
        architecture = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString()
        archive_manifest_checked = $true
        published = $false
    }
    $manifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $output 'release-manifest.json') -Encoding utf8
    $output | Set-Content -LiteralPath artifacts/latest-verification.txt -Encoding utf8
    Write-Output "Verified artifacts: $output"
} finally { Pop-Location }
