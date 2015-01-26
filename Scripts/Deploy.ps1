[CmdletBinding(DefaultParameterSetName="FileOrDirectory")]
param (
	[Parameter(Mandatory=$true, Position=0, ParameterSetName="FileOrDirectory")]
    [ValidateNotNullOrEmpty()]
    [string]$Config,

	[Parameter(Mandatory=$false, Position=1, ParameterSetName="FileOrDirectory")]
	[ValidateNotNullOrEmpty()]
	[string]$ManagedAccount = "TREASURY\SPPortalAppPool",

	[Parameter(Mandatory=$false, Position=2, ParameterSetName="FileOrDirectory")]
	[ValidateNotNullOrEmpty()]
	[string]$SPApplicationPoolName = "Barista Application Pool",

	[Parameter(Mandatory=$false, Position=3, ParameterSetName="FileOrDirectory")]
	[ValidateNotNullOrEmpty()]
	[string]$Uri = "http://ofsdev",

	[Parameter(Mandatory=$false, Position=4, ParameterSetName="FileOrDirectory")]
	[bool]$restartServices = $true
)

Write-Host "                    Barista Deployment Script v1.1 (SP2013)    " -foregroundcolor Green
Write-Host "===============================================================" -foregroundcolor Green

if ( (Get-PSSnapin -Name Microsoft.SharePoint.PowerShell -ErrorAction SilentlyContinue) -ne $null )
{
    Write-Error "This deployment script must be executed from a Windows Powershell, NOT a SharePoint 2010 Management Shell."
    return
}

Write-Host "Please ensure the following before continuing with this deployment:" -foregroundcolor Yellow
Write-Host "`t1) Ensure that all users are logged out of all servers in the farm." -foregroundcolor Yellow
Write-Host "`t2) Please close any other instances of the SharePoint 2013 Management Shell on all servers in the farm" -foregroundcolor Yellow
Write-Host "" -foregroundcolor Yellow
Write-Host "If your are certain that these prerequisites have been met, please press enter to proceed." -foregroundcolor Yellow
$a = Read-Host

if ($restartServices -eq $true) {
	& powershell.exe -Version 3 "& .\RestartSharePointServices.ps1"
}

& powershell.exe -Version 3 "& .\UninstallBaristaSearchService.ps1"
& powershell.exe -Version 3 "& .\UninstallBaristaServiceApplication.ps1"
$cmd = @"
& Add-PSSnapin "Microsoft.SharePoint.PowerShell" -ErrorAction SilentlyContinue
& .\Deploy-SPSolutions.ps1
& Deploy-SPSolutions $Config
"@
& powershell.exe -Version 3 $cmd
& iisreset
& powershell.exe -Version 3 "& .\Deploy-PostSolutionDeployment.ps1 '$ManagedAccount' '$SPApplicationPoolName' '$Uri'"