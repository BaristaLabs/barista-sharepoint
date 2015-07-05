[CmdletBinding(DefaultParameterSetName="FileOrDirectory")]
param (
	[Parameter(Mandatory=$false, Position=0, ParameterSetName="FileOrDirectory")]
	[ValidateNotNullOrEmpty()]
	[string]$Path,

	[Parameter(Mandatory=$false, Position=1, ParameterSetName="FileOrDirectory")]
	[ValidateNotNullOrEmpty()]
	[string]$ManagedAccount = "TREASURY\SPFarmServices",

	[Parameter(Mandatory=$false, Position=2, ParameterSetName="FileOrDirectory")]
	[ValidateNotNullOrEmpty()]
	[string]$SPApplicationPoolName = "Barista Application Pool",

	[Parameter(Mandatory=$false, Position=3, ParameterSetName="FileOrDirectory")]
	[ValidateNotNullOrEmpty()]
	[string]$Uri = "http://ofsdev"
)

$ver = $host | select version
if ($ver.Version.Major -gt 1)  {$Host.Runspace.ThreadOptions = "ReuseThread"}
if ( (Get-PSSnapin -Name  Microsoft.SharePoint.PowerShell -ErrorAction SilentlyContinue) -eq $null )
{
    Add-PsSnapin  Microsoft.SharePoint.PowerShell
}

$script1 = $Path + "\SetupBaristaSearchService.ps1"
$script2 = $Path + "\SetupBaristaServiceApplication.ps1"
$script3 = $Path + "\SetupBaristaServiceApplicationProxy.ps1"
$script4 = $Path + "\TestBaristaServiceApplication.ps1"
$script5 = $Path + "\WarmUpSharePointSites.ps1"

& $script1 $ManagedAccount
& $script2 $ManagedAccount $SPApplicationPoolName
& $script3
& $script4 $Uri
& $script5