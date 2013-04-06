[CmdletBinding(DefaultParameterSetName="FileOrDirectory")]
param (
	[Parameter(Mandatory=$false, Position=0, ParameterSetName="FileOrDirectory")]
	[ValidateNotNullOrEmpty()]
	[string]$ManagedAccount = "TREASURY\SP_WorkerProcess"
)

## delete existing service
# have to use WMI for much of this, native cmdlets are incomplete
write-host 
write-host "[[STEP]] Removing existing Search Service" -foregroundcolor Yellow
write-host 

$serviceLocation = Join-Path ([Microsoft.SharePoint.Utilities.SPUtility]::GetGenericSetupPath("ISAPI")) "BaristaServices\Search\SPBaristaSearchService.exe"

$searchService = Get-WmiObject -Class Win32_Service -Filter "Name = 'BaristaSearchWindowsService'"

if ($searchService -ne $null) 
{ 
	# deactivate in SharePoint
	Remove-BaristaSearchService

	& $serviceLocation stop --sudo
    & $serviceLocation uninstall --sudo
	$searchService.Delete()
	write-host 
	write-host "Search Service Removed..." -foregroundcolor Green
	write-host 
}

## run installutil
# 'frameworkdir' env var apparently isn't present on Win2003...
write-host 
write-host "[[STEP]] Installing Search Service" -foregroundcolor Yellow
write-host 

& $serviceLocation install --sudo

$searchService = Get-WmiObject -Class Win32_Service -Filter "Name = 'BaristaSearchWindowsService'"

if ($searchService -eq $null)
{
	write-host 
	write-host "Barista Search Service was not installed correctly." -foregroundcolor Red
	write-host 
	exit
}

# activate in SharePoint
write-host 
write-host "[[STEP]] Creating Barista Service Hooks." -foregroundcolor Yellow
write-host 
New-BaristaSearchService -ManagedAccount $ManagedAccount -ErrorAction SilentlyContinue
New-BaristaSearchServiceInstance -ErrorAction SilentlyContinue

write-host 
write-host "Successfully installed service $($searchService.name)" -foregroundcolor Green
write-host 