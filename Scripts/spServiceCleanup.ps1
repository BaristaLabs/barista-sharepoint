﻿<#
  .Synopsis
    Lists or Removes Orphaned SharePoint service application Objects from the SharePoint Configuration Database
	Has the option to remove orphaned SharePoint Web Services from an Application Server
  .Description
    Specify the Service name, service full type name, object type name and web service name to find a specific orphaned object
	Parameters include list all service objects, list orphaned services, remove orphan objects, remove IIS Web service application for the service.
	To see examples, from the working directory, type   Get-Help .\spServiceCleanup.ps1 -Detailed 
  .Example
    PS C:\> .\spServiceCleanup.ps1 -exportObjectInfo  
	This example will export details of all sppersisted objects in the SharePoint configuration database to the following file c:\Automation\Reports\sharepointObjects.txt
  .Example
    PS C:\> .\spServiceCleanup.ps1 -listOrphans -ServiceAppName "test service" -ServiceFullTypeName "Test.Test.Application -SPObjectType "Microsoft.SharePoint.SPConnectedServiceApplication" -serviceWebSvcName "WebServiceName.svc"
	This example will list details of the sppersisted objects that match the service type parameters
  .Example
    PS C:\> .\spServiceCleanup.ps1 -removeOrphans -removeIISWebSvc
	This example will remove orphaned service objects and corresponding web service application
  .Example
    PS C:\> .\spServiceCleanup.ps1 -listObjectTypes
	This example will list all object types from the SharePoint Configuration Database that match the SPObjectType parameter
  .Example
    PS C:\> .\spServiceCleanup.ps1 -showActiveService 
	This example will show the current active service application based on parameter settings passed to the script.
  .PARAMETER ServiceAppName
    The name of the SharePoint Service Application to match
  .PARAMETER ServiceFullTypeName
    The full Type name of the service in the service application assembly to match
  .PARAMETER SPObjectType
    The base object type of the service application to filter
  .PARAMETER serviceWebSvcName
    The name of the IIS web service application to match
  .PARAMETER exportObjectInfo
    Use to export all sppersisted object configuration information to a file
  .PARAMETER listOrphans
    Use to list matching objects that have been orphaned or are not the active service application
  .PARAMETER removeOrphans
    Will remove orphaned objects that match the active service application parameters
  .PARAMETER removeIISWebSvc
    Will remove the corresponding IIS Web Service to the managed service application or object
  .PARAMETER showActiveService
    Use to show the active service application
#>
param(
  [string] $serviceAppName = "Barista Service Application",
  [string] $serviceFullTypeName = "Barista.SharePoint.Services.BaristaServiceApplication",
  [string] $spObjectType = "Microsoft.SharePoint.SPConnectedServiceApplication",
  [string] $serviceWebSvcName = "barista.svc",
  [switch] $exportObjectInfo,
  [switch] $listOrphans,
  [switch] $listObjectTypes,
  [switch] $removeOrphans,
  [switch] $removeIISWebSvc,
  [switch] $showActiveService,
  [string] $logFilePath = "c:\Automation\Logs",
  [string] $reportFilePath = "c:\Automation\Reports"
)

#region Initialize Clean Up Script

[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SharePoint") 

Import-Module "WebAdministration"
	    
		#Include the SharePoint cmdlets
		if ((Get-PSSnapin "Microsoft.SharePoint.PowerShell" -ErrorAction SilentlyContinue) -eq $null)
		{
			Add-PSSnapin "Microsoft.SharePoint.PowerShell" -ErrorAction Inquire -WarningAction Inquire
		} 
		


#Set path for log files 
$ERROR_LOG = $logFilePath + "\serviceCleanupErr.log"

$SERVICE_LIST_LOG = $logFilePath + "\serviceList.log"

$OBJECT_INFO_EXPORT = $reportFilePath + "\sharepointPersistedObjects.txt"

#endregion 

#region Methods
 
 
 function Get-InternalValue($obj, $propertyName)  
 {  
    #use reflection to get property values
	  if ($obj)  
       {  
            $type = $obj.GetType()  
            $property = $type.GetProperties([Reflection.BindingFlags] "Static,NonPublic,Instance,Public") | ? { $_.Name -eq $propertyName }       
            if ($property)  
            {  
                 $property.GetValue($obj, $null);  
            }  
       }  
}

function GetActiveService {
 param(
     $filteredObjs
  )
 
		$actService = $null;
		
		# Get Internal Properties for active sharepoint service
		foreach($obj in $filteredObjs)
		{
			  $fObj = $farm.GetObject($obj.Id.Guid);
			  $id =  Get-InternalValue -obj $fObj -propertyName "id"
			  
			  $appAddresses = Get-InternalValue -obj $fObj -propertyName "ApplicationAddresses"
			  
			  if ($activeServiceApp.Id -ieq $obj.Name.Substring(0,36))
			  {
			 	  Write-Host "Active service application " $appAddresses
				  Write-Host $obj
				  Write-Host 
				  
				  $actService = $fobj;
				  break;
			  }
		  
		}

    return $actService

}



function CleanUpServiceApps {
 param(
     $filteredObjs
 )

#TODO: break regions into methods 

 #region Find Active Valid Service Applications
 
        #matching variables
		$classIDMatch = ""
		$parentIDMatch = ""
		$originalString = ""

		# Get Internal Properties for active sharepoint service
		foreach($obj in $filteredObjs)
		{
			  $fObj = $farm.GetObject($obj.Id.Guid);
			  $id =  Get-InternalValue -obj $fObj -propertyName "id"
			  
			  $appAddresses = Get-InternalValue -obj $fObj -propertyName "ApplicationAddresses"
			  	  
			  $classId = $fObj.GetType().GUID
			  
			  $parentId = $fObj.get_Parent().Id
			  
			  if ($activeServiceApp.Id -ieq $obj.Name.Substring(0,36))
			  {
			 	  Write-Host "Found active service application "
				  Write-Host $obj | Format-List 
				  
				  #set values for match condition for finding orphaned services
				  $classIDMatch = $classId
				  $parentIDMatch = $parentId
			      $originalString = $appAddresses	  
				  break;
			  }
		  
		}

  #endregion
  
 
 #region Find List or Remove Orphaned Service apps
  
# Find orphan services
# Get Internal Properties for SharePoint object
foreach($obj in $filterObjs)
{
	  $fObj = $farm.GetObject($obj.Id.Guid);
	  $id =  Get-InternalValue -obj $fObj -propertyName "id"
	  
	  $classId = $fObj.GetType().GUID
	  
	  $parentId = $fObj.get_Parent().Id
	  
	  #get application addresses from internal property in assembly
	  $appAddresses = Get-InternalValue -obj $fObj -propertyName "ApplicationAddresses"
      $wildCardSvcName = "*{0}*" -f $serviceWebSvcName 

	  if ($activeServiceApp.Id -ieq $obj.Name.Substring(0,36)) {
	 	  Write-Host
		  Write-Host "Active service "
		  Write-Host $appAddresses 
		  Write-Host $obj 
		  Write-Host
		  
	  } elseif($classId -eq $classIDMatch -and $parentId -eq $parentIDMatch -and $appAddresses -like $wildCardSvcName ) {
	     #else if matches objects by class id, parent id and service name
		 
		 $msg =  "Orphaned service application {0} {1} {2}" -f $id, $obj.Name, $appAddresses
		
		 if($listOrphans -eq $true) {
		    
		    Write-Host $msg -ForegroundColor Yellow;
			Write-Host
			
		  } else {
			
			 if ($removeOrphans) {
				 $fObj.Delete()
				 $fObj.Unprovision()  #TODO: 
				 
				  Write-Host "Removing " $msg -ForegroundColor Red;
			      Write-Host
			 }
		 }
		
		 #Log or remove IIS Web application
		 #Get IIS web application
		 $webApp = Get-WebApplication | Where {$_.Path -eq ("/" + $obj.Name.Substring(0,36).Replace("-",""))}
		
		if($webApp -ne $null) { 
		 
		  if($removeIISWebSvc) {
			 Write-Host "Removing $webApp.path"
			 $webAppName = $webApp.path.Replace("/","")
		     Remove-WebApplication -Site "SharePoint Web Services" -Name $webAppName
		   }
		 }
	  }
}

 #endregion

}

#endregion

#region Main Entry Point

#region backup prompts

if ($removeIISWebSvc) {

    Write-Host "Backing up the IIS Metabase.."
	#backup IIS Metabase 
	$folderName = "IISBackup{0}" -f [DateTime]::Today.Ticks
	Backup-WebConfiguration -Name $folderName
}

Write-Host "SharePoint Service Applications Utility...."

#Get Farm
$farm = [Microsoft.SharePoint.Administration.SPFarm]::Local

#Get all SharePoint Configuration Databse SPPersisted Objects
$type = 'Microsoft.SharePoint.Administration.SPPersistedTypeCollection`1' -as "Type"  
$type = $type.MakeGenericType( ([Microsoft.SharePoint.Administration.SPPersistedObject]) )

#Set reflection binding flags
$flags = [Reflection.BindingFlags] "Static,NonPublic,Instance,Public"

#Get active service application
$activeServiceApp = Get-SPServiceApplication | Where { $_.GetType().FullName -ieq $serviceFullTypeName -and $_.Name -ieq $serviceAppName }

#Get active ID

$activeId = $NULL

IF ($activeServiceApp -ne $NULL)
{  
	$activeId = $activeServiceApp.Id.ToString().Replace("-","").Trim()
}

#Create Instance collection of sppersisted objects
$instance = [Activator]::CreateInstance($type,$flags,$null,$farm, [System.Globalization.CultureInfo]::CurrentCulture);


#Filter objects by script parameter SPObjectType
$filterObjs = $instance | Select * | Where {$_.TypeName -ieq $spObjectType}

#List matching object types
if($listObjectTypes -eq $true) {
   Write-Host "List filtered objects..."
   
   foreach($o in $filterObjs)
   {
   
      $farmObj = $farm.GetObject($o.Id.Guid);
      $appUrl = Get-InternalValue -obj $farmObj -propertyName "ApplicationAddresses"
	  $module = Get-InternalValue -obj $farmObj -propertyName "Name"
	  
	  Write-Host $appUrl 
	  Write-Host "Name: " $farmObj.DisplayName
	  Write-Host "ID:" $farmObj.Id
	  Write-Host "Base Type:" $farmObj.TypeName
	  Write-Host "Version:" $farmObj.Version
	  Write-Host "Status:" $o.Status
	  Write-Host 
	   
   }
}

#Export configuration of all persisted objects to a file
if($exportObjectInfo -eq $true) {  
	Write-Host "Exporting SharePoint Objects List...." -ForegroundColor DarkYellow
	$instance | Out-File $OBJECT_INFO_EXPORT
	
	Write-Host "SharePoint Persisted Object information has been written to " $OBJECT_INFO_EXPORT
}

#Call function to remove or list orphaned objects
if($removeOrphans -eq $true -or $listOrphans -eq $true) {
    CleanUpServiceApps -filteredObjs $filterObjs
}

#Return / show the active service
if($showActiveService) {

   $svc = GetActiveService -filteredObjs $filterObjs

}

Write-Host "Script execution completed!"
 
 
#endregion

