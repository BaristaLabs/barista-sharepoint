## delete existing service
# have to use WMI for much of this, native cmdlets are incomplete
write-host 
write-host "[[STEP]] Removing SharePoint Hooks" -foregroundcolor Yellow
write-host 

$installUtil = join-path $env:SystemRoot Microsoft.NET\Framework64\v2.0.50727\installutil.exe
$serviceLocation = Join-Path ([Microsoft.SharePoint.Utilities.SPUtility]::GetGenericSetupPath("ISAPI")) "BaristaHostService\SPBaristaHostService.exe"

# deactivate in SharePoint
Remove-BaristaSearchService
Remove-BaristaWebSocketsService

write-host 
write-host "[[STEP]] Removing existing Host Service" -foregroundcolor Yellow
write-host 
$searchService = Get-WmiObject -Class Win32_Service -Filter "Name = 'BaristaSearchWindowsService'"
$webSocketsService = Get-WmiObject -Class Win32_Service -Filter "Name = 'BaristaWebSocketsWindowsService'"
if ($searchService -ne $null -or $webSocketService -ne $null) 
{ 
    & $installUtil $serviceLocation /logfile="BaristaHostServiceUninstall.log" /u | write-verbose
	$searchService.Delete()
	$webSocketsService.Delete()
	write-host 
	write-host "Host Service Removed..." -foregroundcolor Green
	write-host 
}


write-host 
write-host "Successfully uninstalled service $($searchService.name)" -foregroundcolor Green
write-host 

write-host 
write-host "Successfully uninstalled service $($webSocketsService.name)" -foregroundcolor Green
write-host 