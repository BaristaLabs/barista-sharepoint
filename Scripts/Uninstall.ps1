
Write-Host "                    Barista Uninstaller Script v0.1             " -foregroundcolor Green
Write-Host "===============================================================" -foregroundcolor Green

if ( (Get-PSSnapin -Name Microsoft.SharePoint.PowerShell -ErrorAction SilentlyContinue) -ne $null )
{
    Write-Error "This deployment script must be executed from a Windows Powershell, NOT a SharePoint 2010 Management Shell."
    return
}

Write-Host "Uninstalling Barista from from $env:COMPUTERNAME"

& powershell.exe -Version 2 "& .\UninstallBaristaSearchService.ps1"
& powershell.exe -Version 2 "& .\UninstallBaristaServiceApplication.ps1"
& powershell.exe -Version 2 "& .\UninstallBaristaSolutions.ps1"

#Cleanup
& powershell.exe -Version 2 "& .\RemoveBaristaTypes.ps1"

Write-Host "Completed."