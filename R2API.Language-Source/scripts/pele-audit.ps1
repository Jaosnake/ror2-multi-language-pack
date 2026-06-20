param(
    [string]$LanguageRoot = (Join-Path $PSScriptRoot '..\..\PELE\Language'),
    [string]$BaselineLanguage = 'uk',
    [switch]$FailOnWarnings
)

$ErrorActionPreference = 'Stop'
$script:Messages = New-Object System.Collections.Generic.List[string]

function Get-JsonTokenMap {
    param([string]$Root)

    $result = @{}
    if (-not (Test-Path -LiteralPath $Root)) {
        throw "Language root not found: $Root"
    }

    Get-ChildItem -LiteralPath $Root -Directory | ForEach-Object {
        $lang = $_.Name
        $result[$lang] = @{}

        Get-ChildItem -LiteralPath $_.FullName -Filter '*.json' -File | ForEach-Object {
            $file = $_.FullName
            $raw = Get-Content -LiteralPath $file -Raw -Encoding UTF8
            $matches = [regex]::Matches($raw, '"(?<key>(?:\\"|[^"])*)"\s*:')
            $seenInFile = @{}

            foreach ($match in $matches) {
                $key = $match.Groups['key'].Value
                if ($key -eq 'language' -or $key -eq 'strings' -or $key.StartsWith('_')) { continue }
                if ($key -match '\s') { continue }

                if (-not $seenInFile.ContainsKey($key)) {
                    $seenInFile[$key] = 0
                }
                $seenInFile[$key]++

                if (-not $result[$lang].ContainsKey($key)) {
                    $result[$lang][$key] = @{
                        Files = New-Object System.Collections.Generic.List[string]
                        Values = New-Object System.Collections.Generic.List[string]
                    }
                }

                $result[$lang][$key].Files.Add($file)
            }

            foreach ($key in $seenInFile.Keys) {
                if ($seenInFile[$key] -gt 1) {
                    $script:Messages.Add("DUPLICATE_IN_FILE|$lang|$key|$file|$($seenInFile[$key])")
                }
            }

            $json = $null
            try {
                $json = $raw | ConvertFrom-Json
            }
            catch {
                $script:Messages.Add("INVALID_JSON|$lang|$file|$($_.Exception.Message)")
                return
            }

            try {
                $object = if ($null -ne $json.strings) { $json.strings } else { $json }
                foreach ($property in $object.PSObject.Properties) {
                    $key = $property.Name
                    if ($key -eq 'language' -or $key -eq 'strings' -or $key.StartsWith('_')) { continue }
                    if (-not $result[$lang].ContainsKey($key)) { continue }
                    $result[$lang][$key].Values.Add([string]$property.Value)
                }
            }
            catch {
                $script:Messages.Add("VALUE_PARSE_FAILED|$lang|$file|$($_.Exception.Message)")
            }
        }
    }

    return ,$result
}

function Get-Placeholders {
    param([string]$Value)
    return @([regex]::Matches($Value, '\{[0-9]+\}') | ForEach-Object { $_.Value } | Sort-Object -Unique)
}

function Get-StyleTags {
    param([string]$Value)
    return @([regex]::Matches($Value, '</?(style|color|sprite|nobr|i|b)(?:=[^>]*)?>') | ForEach-Object { $_.Value } | Sort-Object)
}

$tokenMap = Get-JsonTokenMap -Root $LanguageRoot
$languages = @($tokenMap.Keys | Sort-Object)

if (-not $tokenMap.ContainsKey($BaselineLanguage)) {
    $fallbackBaseline = @($tokenMap.Keys | Sort-Object | Select-Object -First 1)
    if ($fallbackBaseline.Count -eq 0) {
        throw "No language folders found under: $LanguageRoot"
    }
    $script:Messages.Add("BASELINE_NOT_FOUND|requested=$BaselineLanguage|using=$($fallbackBaseline[0])")
    $BaselineLanguage = $fallbackBaseline[0]
}

$baselineTokens = @($tokenMap[$BaselineLanguage].Keys)
foreach ($lang in $languages) {
    if ($lang -eq $BaselineLanguage) { continue }

    $langTokens = @($tokenMap[$lang].Keys)
    $missing = @($baselineTokens | Where-Object { -not $tokenMap[$lang].ContainsKey($_) })
    foreach ($token in $missing) {
        $script:Messages.Add("MISSING_TOKEN|$lang|$token")
    }

    $extra = @($langTokens | Where-Object { -not $tokenMap[$BaselineLanguage].ContainsKey($_) })
    foreach ($token in $extra) {
        $script:Messages.Add("EXTRA_TOKEN|$lang|$token")
    }
}

foreach ($token in $baselineTokens) {
    $baselineValue = if ($tokenMap[$BaselineLanguage][$token].Values.Count -gt 0) { $tokenMap[$BaselineLanguage][$token].Values[0] } else { '' }
    $baselinePlaceholders = @(Get-Placeholders $baselineValue)
    $baselineTags = @(Get-StyleTags $baselineValue)

    foreach ($lang in $languages) {
        if ($lang -eq $BaselineLanguage) { continue }
        if (-not $tokenMap[$lang].ContainsKey($token)) { continue }
        if ($tokenMap[$lang][$token].Values.Count -eq 0) { continue }

        $value = $tokenMap[$lang][$token].Values[0]
        $placeholders = @(Get-Placeholders $value)
        if (($baselinePlaceholders -join ',') -ne ($placeholders -join ',')) {
            $script:Messages.Add("PLACEHOLDER_MISMATCH|$lang|$token|baseline=$($baselinePlaceholders -join ',')|value=$($placeholders -join ',')")
        }

        $tags = @(Get-StyleTags $value)
        if (($baselineTags -join '') -ne ($tags -join '')) {
            $script:Messages.Add("TAG_MISMATCH|$lang|$token")
        }
    }
}

$eoPath = Join-Path $LanguageRoot 'eo'
if (Test-Path -LiteralPath $eoPath) {
    $esperantoChars = [char[]]@(
        [char]0x0108, [char]0x0109, [char]0x011C, [char]0x011D,
        [char]0x0124, [char]0x0125, [char]0x0134, [char]0x0135,
        [char]0x015C, [char]0x015D, [char]0x016C, [char]0x016D
    )
    $mojibakeMarkers = @('Ä', 'Å')

    Get-ChildItem -LiteralPath $eoPath -Filter '*.json' -File | ForEach-Object {
        $file = $_.FullName
        $lineNumber = 0
        Get-Content -LiteralPath $file -Encoding UTF8 | ForEach-Object {
            $lineNumber++
            $line = $_
            foreach ($char in $line.ToCharArray()) {
                if ($esperantoChars -contains $char) {
                    $script:Messages.Add("ESPERANTO_NON_X_SYSTEM|$file|$lineNumber|$char")
                    break
                }
            }
            foreach ($marker in $mojibakeMarkers) {
                if ($line.Contains($marker)) {
                    $script:Messages.Add("ESPERANTO_MOJIBAKE|$file|$lineNumber|$marker")
                    break
                }
            }
        }
    }
}

Write-Output "PELE audit"
Write-Output "Language root: $LanguageRoot"
Write-Output "Baseline: $BaselineLanguage"
Write-Output "Languages: $($languages -join ', ')"
Write-Output "Issues: $($script:Messages.Count)"

$summary = $script:Messages |
    ForEach-Object { ($_ -split '\|', 2)[0] } |
    Group-Object |
    Sort-Object Name

foreach ($entry in $summary) {
    Write-Output "Summary.$($entry.Name): $($entry.Count)"
}

foreach ($message in $script:Messages) {
    Write-Output $message
}

if ($script:Messages.Count -gt 0 -and $FailOnWarnings) {
    exit 1
}
