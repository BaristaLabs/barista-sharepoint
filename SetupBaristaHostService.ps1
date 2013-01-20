[CmdletBinding(DefaultParameterSetName="FileOrDirectory")]
param (
	[Parameter(Mandatory=$false, Position=0, ParameterSetName="FileOrDirectory")]
	[ValidateNotNullOrEmpty()]
	[string]$ManagedAccount = "TREASURY\SP_WorkerProcess"
)

## delete existing service
# have to use WMI for much of this, native cmdlets are incomplete
write-host 
write-host "[[STEP]] Removing existing Host Service" -foregroundcolor Yellow
write-host 

$installUtil = join-path $env:SystemRoot Microsoft.NET\Framework64\v2.0.50727\installutil.exe
$serviceLocation = Join-Path ([Microsoft.SharePoint.Utilities.SPUtility]::GetGenericSetupPath("ISAPI")) "BaristaHostService\SPBaristaHostService.exe"

$searchService = Get-WmiObject -Class Win32_Service -Filter "Name = 'BaristaSearchWindowsService'"
$webSocketsService = Get-WmiObject -Class Win32_Service -Filter "Name = 'BaristaWebSocketsWindowsService'"
if ($searchService -ne $null -or $webSocketService -ne $null) 
{ 
	# deactivate in SharePoint
	Remove-BaristaSearchService
	Remove-BaristaWebSocketsService

    & $installUtil $serviceLocation /logfile="BaristaHostServiceUninstall.log" /u | write-verbose
	$searchService.Delete()
	$webSocketsService.Delete()
	write-host 
	write-host "Host Service Removed..." -foregroundcolor Green
	write-host 
}

## run installutil
# 'frameworkdir' env var apparently isn't present on Win2003...
write-host 
write-host "[[STEP]] Installing Host Service" -foregroundcolor Yellow
write-host 

& $installUtil $serviceLocation /logfile="BaristaHostServiceInstall.log" | write-verbose

$searchService = Get-WmiObject -Class Win32_Service -Filter "Name = 'BaristaSearchWindowsService'"
$webSocketsService = Get-WmiObject -Class Win32_Service -Filter "Name = 'BaristaWebSocketsWindowsService'"

if ($searchService -eq $null -or $webSocketsService -eq $null)
{
	write-host 
	write-host "Barista Host Service was not installed correctly." -foregroundcolor Red
	write-host 
	exit
}

# activate in SharePoint
write-host 
write-host "[[STEP]] Creating Barista Service Hooks." -foregroundcolor Yellow
write-host 
New-BaristaSearchService -ManagedAccount $ManagedAccount
New-BaristaSearchServiceInstance
New-BaristaWebSocketsService -ManagedAccount $ManagedAccount
New-BaristaWebSocketsServiceInstance

write-host 
write-host "Successfully installed service $($searchService.name)" -foregroundcolor Green
write-host 

write-host 
write-host "Successfully installed service $($webSocketsService.name)" -foregroundcolor Green
write-host 