param([string]$ProjectDir, [string]$ProjectPath);
$currentDate = (Get-Date).ToUniversalTime().ToString("yyyy.MM.dd.HHmm");
#$currentDate = get-date -format yyyy.MM.dd.HHmm;
$find = "<Version>(.|\n)*?</Version>";
$replace = "<Version>" + $currentDate + "</Version>";
$csproj = Get-Content $ProjectPath
$csprojUpdated = $csproj -replace $find, $replace

Set-Content -Path $ProjectPath -Value $csprojUpdated