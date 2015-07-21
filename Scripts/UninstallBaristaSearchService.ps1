## delete existing service
# have to use WMI for much of this, native cmdlets are incomplete
write-host 
write-host "[[STEP]] Removing SharePoint Hooks" -foregroundcolor Yellow
write-host 

$ver = $host | select version
if ($ver.Version.Major -gt 1)  {$Host.Runspace.ThreadOptions = "ReuseThread"}
if ( (Get-PSSnapin -Name  Microsoft.SharePoint.PowerShell -ErrorAction SilentlyContinue) -eq $null )
{
    Add-PsSnapin Microsoft.SharePoint.PowerShell
}

$serviceLocation = Join-Path ([Microsoft.SharePoint.Utilities.SPUtility]::GetGenericSetupPath("ISAPI")) "BaristaServices\Search\SPBaristaSearchService.exe"

# deactivate in SharePoint
if (Get-Command "Remove-BaristaSearchService" -errorAction SilentlyContinue) {
	try {
		Remove-BaristaSearchService -ErrorAction SilentlyContinue
	}
	catch [System.Management.Automation.CommandNotFoundException] {
	  write-host 'Barista Search Service Not Installed.' -foregroundcolor Yellow
	}
}

write-host 
write-host "[[STEP]] Removing existing Barista Search Service" -foregroundcolor Yellow
write-host 
$searchService = Get-WmiObject -Class Win32_Service -Filter "Name = 'BaristaSearchWindowsService'" -ComputerName $env:COMPUTERNAME | out-null
if ($searchService -ne $null) 
{ 
	$searchService.Unprovision()
	$searchService.Delete()
	$searchService.Uncache()
	& $serviceLocation stop --sudo
    & $serviceLocation uninstall --sudo
	write-host 
	write-host "Barista Search Service Removed..." -foregroundcolor Green
	write-host 
}


write-host 
write-host "Successfully uninstalled service $($searchService.name)" -foregroundcolor Green
write-host 