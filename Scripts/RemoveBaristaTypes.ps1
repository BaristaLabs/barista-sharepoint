function LoadSharePointPowerShellEnvironment
{
	$ver = $host | select version
	if ($ver.Version.Major -gt 1)  {$Host.Runspace.ThreadOptions = "ReuseThread"}
	if ( (Get-PSSnapin -Name  Microsoft.SharePoint.PowerShell -ErrorAction SilentlyContinue) -eq $null )
	{
		Add-PsSnapin  Microsoft.SharePoint.PowerShell
	}
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
	$instance = [Activator]::CreateInstance($type,$flags,$null,$farm, [System.Globalization.CultureInfo]::CurrentCulture);

	#Filter objects by script parameter SPObjectType
	$filterObjs = $instance | Select * | Where {$_.TypeName -ieq $typeName}

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
		$fObj.Delete()
		$fObj.Unprovision()
	}
}

Write-Host
LoadSharePointPowerShellEnvironment

$farm = [Microsoft.SharePoint.Administration.SPFarm]::Local

Write-Host "Ensuring all instances of 'Barista Search Service' are removed."
$objects = GetPersistedObjectsForType("Barista Search Service");
CleanUpPersistedObjects($objects);

Write-Host "Ensuring all instances of 'Barista Service Application' are removed."
$objects = GetPersistedObjectsForType("Barista Service Application");
CleanUpPersistedObjects($objects);

Write-Host "Ensuring all instances of 'Barista.SharePoint.Services.BaristaService' are removed."
$objects = GetPersistedObjectsForType("Barista.SharePoint.Services.BaristaService");
CleanUpPersistedObjects($objects);

