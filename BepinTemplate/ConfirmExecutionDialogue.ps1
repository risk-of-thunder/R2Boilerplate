Add-Type -AssemblyName PresentationCore,PresentationFramework
$ButtonType = [System.Windows.MessageBoxButton]::YesNo
$MessageboxTitle = “CONFIRM RoR2 INSTALLATION FINDER EXECUTION”
$Messageboxbody = “You are about to execute a binary file which will search your SteamLibraries for a RoR2 Installation. This will automatically place your Debug build into '/Bepinex/Plugins/dev/'.`nDo you want to continue?”
$MessageIcon = [System.Windows.MessageBoxImage]::Warning
$Result = [System.Windows.MessageBox]::Show($Messageboxbody,$MessageboxTitle,$ButtonType,$messageicon, "No")
if($Result -eq "YES")
{
    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    $parent = Split-Path -parent $scriptDir
    & "$parent\QuerySteamGameLoc.exe" @('Risk of Rain 2', '632360') | Out-File -FilePath $scriptDir\bin\Debug\netstandard2.0\rorDirectory.txt -encoding ASCII 
}