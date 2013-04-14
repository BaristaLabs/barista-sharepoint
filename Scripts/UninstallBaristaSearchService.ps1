## delete existing service
# have to use WMI for much of this, native cmdlets are incomplete
write-host 
write-host "[[STEP]] Removing SharePoint Hooks" -foregroundcolor Yellow
write-host 

$serviceLocation = Join-Path ([Microsoft.SharePoint.Utilities.SPUtility]::GetGenericSetupPath("ISAPI")) "BaristaServices\Search\SPBaristaSearchService.exe"

# deactivate in SharePoint
Remove-BaristaSearchService -ErrorAction SilentlyContinue

write-host 
write-host "[[STEP]] Removing existing Search Service" -foregroundcolor Yellow
write-host 
$searchService = Get-WmiObject -Class Win32_Service -Filter "Name = 'BaristaSearchWindowsService'" -ComputerName $env:COMPUTERNAME
if ($searchService -ne $null) 
{ 
	$searchService.Delete()
	& $serviceLocation stop --sudo
    & $serviceLocation uninstall --sudo
	write-host 
	write-host "Search Service Removed..." -foregroundcolor Green
	write-host 
}


write-host 
write-host "Successfully uninstalled service $($searchService.name)" -foregroundcolor Green
write-host 