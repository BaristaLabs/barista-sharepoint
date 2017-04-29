[CmdletBinding(DefaultParameterSetName="FileOrDirectory")]
param (
	[Parameter(Mandatory=$false, Position=0, ParameterSetName="FileOrDirectory")]
	[ValidateNotNullOrEmpty()]
	[string]$ManagedAccount = "TREASURY\SP_Farm",

	[Parameter(Mandatory=$false, Position=1, ParameterSetName="FileOrDirectory")]
	[ValidateNotNullOrEmpty()]
	[string]$SPApplicationPoolName = "Barista Application Pool",

	[Parameter(Mandatory=$false, Position=2, ParameterSetName="FileOrDirectory")]
	[ValidateNotNullOrEmpty()]
	[string]$Uri = "http://ofs.treasuryecm.gov"
)

$ver = $host | select version
if ($ver.Version.Major -gt 1)  {$Host.Runspace.ThreadOptions = "ReuseThread"}
if ( (Get-PSSnapin -Name  Microsoft.SharePoint.PowerShell -ErrorAction SilentlyContinue) -eq $null )
{
    Add-PsSnapin  Microsoft.SharePoint.PowerShell
}

& .\SetupBaristaSearchService.ps1 $ManagedAccount
& .\SetupBaristaServiceApplication.ps1 $ManagedAccount $SPApplicationPoolName
& .\SetupBaristaServiceApplicationProxy.ps1
& .\TestBaristaServiceApplication.ps1 $Uri