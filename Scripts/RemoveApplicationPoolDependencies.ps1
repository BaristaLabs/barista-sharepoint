[CmdletBinding(DefaultParameterSetName="FileOrDirectory")]
param (
	[Parameter(Mandatory=$false, Position=0, ParameterSetName="FileOrDirectory")]
	[ValidateNotNullOrEmpty()]
	[string]$SPApplicationPoolName = "Barista Application Pool",
	
	[Parameter(Mandatory=$false, Position=1, ParameterSetName="FileOrDirectory")]
	[ValidateNotNullOrEmpty()]
	[bool]$remove = $false
)

function LoadSharePointPowerShellEnvironment
{
	$ver = $host | select version
	if ($ver.Version.Major -gt 1)  {$Host.Runspace.ThreadOptions = "ReuseThread"}
	if ( (Get-PSSnapin -Name  Microsoft.SharePoint.PowerShell -ErrorAction SilentlyContinue) -eq $null )
	{
		Add-PsSnapin  Microsoft.SharePoint.PowerShell
	}
}

function GetPersistedObjectDependencies {
param(
	[Guid]$id
)
	#Get all SharePoint Configuration Database SPPersisted Objects
	$type = 'Microsoft.SharePoint.Administration.SPPersistedTypeCollection`1' -as "Type"  
	$type = $type.MakeGenericType( ([Microsoft.SharePoint.Administration.SPPersistedObject]) )

	#Set reflection binding flags
	$flags = [Reflection.BindingFlags] "Static,NonPublic,Instance,Public"

	#Create Instance collection of sppersisted objects
	$instance = [Activator]::CreateInstance($type, $flags, $null, $farm, [System.Globalization.CultureInfo]::CurrentCulture);

	#Filter objects by script parameter SPObjectType
	$filteredObjs = $instance | Select * | Where {$_.Parent.Id -eq $id}

	return $filteredObjs
}

LoadSharePointPowerShellEnvironment
$farm = [Microsoft.SharePoint.Administration.SPFarm]::Local

$appPool = Get-SPServiceApplicationPool | Where {$_.Name -eq $SPApplicationPoolName}
$deps = GetPersistedObjectDependencies $appPool.Id
if ($deps -eq $null) {
	Write-Host "No Dependencies Detected."
}
else {
	Write-Host $deps
	
	if ($remove) {
		foreach ($dep in $deps) {
			$dep.Unprovision()
			$dep.Delete()
			$dep.Uncache()
		}
	}
}
