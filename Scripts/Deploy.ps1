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
	[string]$Uri = "http://ofsdev",

	[Parameter(Mandatory=$false, Position=4, ParameterSetName="FileOrDirectory")]
	[bool]$restartServices = $true
)

if ( (Get-PSSnapin -Name Microsoft.SharePoint.PowerShell -ErrorAction SilentlyContinue) -ne $null )
{
    Remove-PsSnapin Microsoft.SharePoint.PowerShell
}

if ($restartServices -eq $true) {
	& powershell.exe "& .\RestartSharePointServices.ps1"
}

& powershell.exe "& .\UninstallBaristaSearchService.ps1"
& powershell.exe "& .\UninstallBaristaServiceApplication.ps1"
$cmd = @"
& Add-PSSnapin "Microsoft.SharePoint.PowerShell" -ErrorAction SilentlyContinue
& .\Deploy-SPSolutions.ps1
& Deploy-SPSolutions $Config
"@
& powershell.exe $cmd
& iisreset
& powershell.exe "& .\Deploy-PostSolutionDeployment.ps1 '$ManagedAccount' '$SPApplicationPoolName' '$Uri'"

if ( (Get-PSSnapin -Name  Microsoft.SharePoint.PowerShell -ErrorAction SilentlyContinue) -eq $null )
{
    Add-PsSnapin Microsoft.SharePoint.PowerShell
}