function LoadSharePointPowerShellEnvironment
{
	$ver = $host | select version
	if ($ver.Version.Major -gt 1)  {$Host.Runspace.ThreadOptions = "ReuseThread"}
	if ( (Get-PSSnapin -Name  Microsoft.SharePoint.PowerShell -ErrorAction SilentlyContinue) -eq $null )
	{
		Add-PsSnapin  Microsoft.SharePoint.PowerShell
	}
}

function GetPersistedObjectsForName() {
param(
	[string]$name
)
	#Get all SharePoint Configuration Database SPPersisted Objects
	$type = 'Microsoft.SharePoint.Administration.SPPersistedTypeCollection`1' -as "Type"  
	$type = $type.MakeGenericType( ([Microsoft.SharePoint.Administration.SPPersistedObject]) )

	#Set reflection binding flags
	$flags = [Reflection.BindingFlags] "Static,NonPublic,Instance,Public"

	#Create Instance collection of sppersisted objects
	$instance = [Activator]::CreateInstance($type, $flags, $null, $farm, [System.Globalization.CultureInfo]::CurrentCulture);

	#Filter objects by script parameter SPObjectType
	$filteredObjs = $instance | Select * | Where {$_.Name -ieq $name}

	return $filteredObjs
}

function GetPersistedObjectsForType() {
param(
	[string]$typeName
)
	#Get all SharePoint Configuration Database SPPersisted Objects
	$type = 'Microsoft.SharePoint.Administration.SPPersistedTypeCollection`1' -as "Type"  
	$type = $type.MakeGenericType( ([Microsoft.SharePoint.Administration.SPPersistedObject]) )

	#Set reflection binding flags
	$flags = [Reflection.BindingFlags] "Static,NonPublic,Instance,Public"

	#Create Instance collection of sppersisted objects
	$instance = [Activator]::CreateInstance($type, $flags, $null, $farm, [System.Globalization.CultureInfo]::CurrentCulture);

	#Filter objects by script parameter SPObjectType
	$filteredObjs = $instance | Select * | Where {$_.GetType().FullName -ieq $typeName}

	return $filteredObjs
}

function CleanUpPersistedObjects {
param(
	$objs
)
	if ($objs -eq $null) {
		Write-Host "No Objects Found.";
		return;
	}

	Write-Host "Cleaning up " $objs.Length "objects..."

	foreach($fObj in $objs)
	{
		Write-Host "     Cleaning Up" $fObj.Id
		if ($fObj.Delete) {
			$fObj.Delete()
		}

		if ($fObj.Unprovision) {
			$fObj.Unprovision()
		}

		if ($fObj.Uncache) {
			$fObj.Uncache()
		}

		stsadm -o deleteconfigurationobject -id $fObj.Id
		$fObj = $null
	}
}

Write-Host
LoadSharePointPowerShellEnvironment

$farm = [Microsoft.SharePoint.Administration.SPFarm]::Local

Write-Host "Ensuring all instances of 'Barista Search Service Instance' are removed."
$objects = GetPersistedObjectsForName("BaristaSearchServiceInstance");
CleanUpPersistedObjects($objects);

Write-Host "Ensuring all instances of 'Barista Search Windows Service' are removed."
$objects = GetPersistedObjectsForName("BaristaSearchWindowsService");
CleanUpPersistedObjects($objects);

Write-Host "Ensuring all instances of 'Barista Service Instance' are removed."
$objects = GetPersistedObjectsForName("BaristaServiceInstance");
CleanUpPersistedObjects($objects);

Write-Host "Ensuring all instances of 'Barista Service Instance' are removed (by type)."
$objects = GetPersistedObjectsForType("Barista.SharePoint.Services.BaristaServiceInstance");
CleanUpPersistedObjects($objects);

Write-Host "Ensuring all instances of 'Barista Service Application Proxy' are removed."
$objects = GetPersistedObjectsForName("Barista Service Application Proxy");
CleanUpPersistedObjects($objects);

Write-Host "Ensuring all instances of 'Barista Service Proxy' are removed."
$objects = GetPersistedObjectsForName("BaristaServiceProxy");
CleanUpPersistedObjects($objects);

Write-Host "Ensuring all instances of 'Barista Service Application' are removed."
$objects = GetPersistedObjectsForName("Barista Service Application");
CleanUpPersistedObjects($objects);

Write-Host "Ensuring all instances of 'Barista Service Application' are removed (by type)."
$objects = GetPersistedObjectsForType("Barista.SharePoint.Services.BaristaServiceApplication");
CleanUpPersistedObjects($objects);

Write-Host "Removing Barista Service Application"
$sa = Get-SPServiceApplication | Where {$_.Name -eq "Barista Service Application"}
if ($sa -ne $null) {
	Remove-SPServiceApplication $sa -Confirm:$false
	$sa.Uncache()
	Write-Host "Removed."
} else {
	Write-Host "Service Application Not found."
}

Write-Host "Ensuring all instances of 'Barista Service' are removed."
$objects = GetPersistedObjectsForName("BaristaService");
CleanUpPersistedObjects($objects);

Write-Host "Ensuring all instances of 'Barista Service' are removed (by type)."
$objects = GetPersistedObjectsForType("Barista.SharePoint.Services.BaristaService");
CleanUpPersistedObjects($objects);

Write-Host "Removing Barista Application Pool"
$appPool = Get-SPServiceApplicationPool | Where {$_.Name -eq "Barista Application Pool"} 
if ($appPool -ne $null) {
	Remove-SPServiceApplicationPool $appPool -Confirm:$false
	$appPool.Uncache()
	Write-Host "Removed."
} else {
	Write-Host "Application Pool Not found."
}