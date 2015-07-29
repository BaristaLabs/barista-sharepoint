[CmdletBinding(DefaultParameterSetName="FileOrDirectory")]
param (
	[Parameter(Mandatory=$false, Position=0, ParameterSetName="FileOrDirectory")]
	[ValidateNotNullOrEmpty()]
	[string]$ManagedAccount = "TREASURY\SP_WorkerProcess"
)

## delete existing service
# have to use WMI for much of this, native cmdlets are incomplete
write-host 
write-host "[[STEP]] Removing existing Barista Search Service" -foregroundcolor Yellow
write-host 

$serviceLocation = Join-Path ([Microsoft.SharePoint.Utilities.SPUtility]::GetGenericSetupPath("ISAPI")) "BaristaServices\Search\SPBaristaSearchService.exe"

if (Get-Command "Remove-BaristaSearchService" -errorAction SilentlyContinue) {

	Try {
	Remove-BaristaSearchService -errorAction SilentlyContinue
	}
	Catch {
		# Do Nothing...
	}
}

$searchService = Get-WmiObject -Class Win32_Service -Filter "Name = 'BaristaSearchWindowsService'" -ComputerName $env:COMPUTERNAME

if ($searchService -ne $null) 
{ 
	& $serviceLocation stop --sudo
    & $serviceLocation uninstall --sudo
	Try {
	$searchService.Delete()
	}
	Catch {
		# Do Nothing...
	}
	write-host 
	write-host "Barista Search Service Removed..." -foregroundcolor Green
	write-host 
}

## run installutil
# 'frameworkdir' env var apparently isn't present on Win2003...
write-host 
write-host "[[STEP]] Installing Barista Search Service" -foregroundcolor Yellow
write-host 

& netsh http add urlacl url=http://+:8500/Barista user=$ManagedAccount | out-null
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
write-host "[[STEP]] Creating Barista Search Service Hooks." -foregroundcolor Yellow
write-host 
New-BaristaSearchService -ManagedAccount $ManagedAccount -ErrorAction SilentlyContinue | out-null
New-BaristaSearchServiceInstance -ErrorAction SilentlyContinue | out-null

write-host 
write-host "Successfully installed service $($searchService.name)" -foregroundcolor Green
write-host 