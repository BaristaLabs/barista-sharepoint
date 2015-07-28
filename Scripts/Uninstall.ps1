[CmdletBinding(DefaultParameterSetName="FileOrDirectory")]
param (

	[Parameter(Mandatory=$false, Position=0, ParameterSetName="FileOrDirectory")]
	[ValidateNotNullOrEmpty()]
	[string]$SPApplicationPoolName = "Barista Application Pool"
)

Write-Host "                    Barista Uninstaller Script v0.1             " -foregroundcolor Green
Write-Host "===============================================================" -foregroundcolor Green

if ( (Get-PSSnapin -Name Microsoft.SharePoint.PowerShell -ErrorAction SilentlyContinue) -ne $null )
{
    Write-Error "This deployment script must be executed from a Windows Powershell, NOT a SharePoint 2010 Management Shell."
	Write-Error "If this is a Windows Powershell window, please close this window and run this script from a new Windows Powershell window."
    return
}

Write-Host "Uninstalling Barista from from $env:COMPUTERNAME"

& powershell.exe -Version 2 "& .\UninstallBaristaSearchService.ps1"
& powershell.exe -Version 2 "& .\UninstallBaristaServiceApplication.ps1 '$SPApplicationPoolName'"
& powershell.exe -Version 2 "& .\UninstallBaristaSolutions.ps1"

#Cleanup
& powershell.exe -Version 2 "& .\RemoveBaristaTypes.ps1"

Write-Host "Completed."