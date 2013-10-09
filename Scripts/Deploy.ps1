[CmdletBinding(DefaultParameterSetName="FileOrDirectory")]
param (
	[Parameter(Mandatory=$true, Position=0, ParameterSetName="FileOrDirectory")]
    [ValidateNotNullOrEmpty()]
    [string]$Config,

	[Parameter(Mandatory=$false, Position=1, ParameterSetName="FileOrDirectory")]
	[ValidateNotNullOrEmpty()]
	[string]$ManagedAccount = "TREASURY\SP_WorkerProcess",

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

[xml]$ConfigXml = Get-Content $Config

& .\RestartSharePointServices.ps1
& .\UninstallBaristaSearchService.ps1
& .\UninstallBaristaServiceApplication.ps1
& .\Deploy-SPSolutions.ps1 
Deploy-SPSolutions $ConfigXml
& iisreset
& powershell.exe -NoExit "& .\Deploy-PostSolutionDeployment.ps1 '$ManagedAccount' '$SPApplicationPoolName' '$Uri'"