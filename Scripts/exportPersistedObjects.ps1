#Get all SharePoint Configuration Database SPPersisted Objects
$farm = [Microsoft.SharePoint.Administration.SPFarm]::Local

$type = 'Microsoft.SharePoint.Administration.SPPersistedTypeCollection`1' -as "Type"  
$type = $type.MakeGenericType( ([Microsoft.SharePoint.Administration.SPPersistedObject]) )

#Set reflection binding flags
$flags = [Reflection.BindingFlags] "Static,NonPublic,Instance,Public"

#Create Instance collection of sppersisted objects
$instance = [Activator]::CreateInstance($type, $flags, $null, $farm, [System.Globalization.CultureInfo]::CurrentCulture);

$instance | Out-File ".\persistedObjects.txt"