[CmdletBinding()]
Param(
    [int]$p,
    [int]$j,
    [int]$c
)

Set-Location "$PSScriptRoot"
dotnet run -- -p $p -j $j -c $c