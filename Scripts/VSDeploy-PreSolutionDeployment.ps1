[CmdletBinding(DefaultParameterSetName="FileOrDirectory")]
param (
	[Parameter(Mandatory=$false, Position=0, ParameterSetName="FileOrDirectory")]
	[ValidateNotNullOrEmpty()]
	[string]$Path
)

$ver = $host | select version
if ($ver.Version.Major -gt 1)  {$Host.Runspace.ThreadOptions = "ReuseThread"}
if ( (Get-PSSnapin -Name  Microsoft.SharePoint.PowerShell -ErrorAction SilentlyContinue) -eq $null )
{
    Add-PsSnapin  Microsoft.SharePoint.PowerShell
}

$script1 = $Path + "\UninstallBaristaSearchService.ps1"
$script2 = $Path + "\UninstallBaristaServiceApplication.ps1"
$script3 = $Path + "\spServiceCleanup.ps1"
& $script1
& $script2
& $script3 -removeOrphans
iisreset /noforce /TIMEOUT:60