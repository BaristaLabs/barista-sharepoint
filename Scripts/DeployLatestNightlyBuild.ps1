function Get-LatestDropLocation {
	param(
		[Parameter(Position=0,Mandatory=$true)] [string]$tfsLocation,
		[Parameter(Position=1,Mandatory=$true)] [string]$projectName,
		[Parameter(Position=3,Mandatory=$true)] [string]$buildDefinitionName
	)
 
	Add-Type -AssemblyName "Microsoft.TeamFoundation.Client, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL"
	Add-Type -AssemblyName "Microsoft.TeamFoundation.Build.Client, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL"
	$tfsUri = New-object Uri($tfsLocation)
	$teamProjectCollection = [Microsoft.TeamFoundation.Client.TfsTeamProjectCollectionFactory]::GetTeamProjectCollection($tfsUri)
	$service = $teamProjectCollection.GetService([Type]"Microsoft.TeamFoundation.Build.Client.IBuildServer")
	$spec = $service.CreateBuildDetailSpec($projectName, $buildDefinitionName)
 
	$spec.MaxBuildsPerDefinition = 1
	$spec.QueryOrder = [Microsoft.TeamFoundation.Build.Client.BuildQueryOrder]::FinishTimeDescending
	$spec.Status = [Microsoft.TeamFoundation.Build.Client.BuildStatus]::Succeeded
 
	$results = $service.QueryBuilds($spec)
 
	if ($results.Builds.Length -eq 1) { Write-Output $results.Builds[0].DropLocation } else { Write-Error "No builds found." }
	return $results.Builds[0].DropLocation
}
 
Get-LatestDropLocation "https://sixconcepts.visualstudio.com/defaultcollection" "Barista" "Barista - Nightly"