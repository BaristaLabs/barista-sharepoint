Add-PSSnapin "Microsoft.SharePoint.PowerShell" -ErrorAction SilentlyContinue
$farm = Get-SPFarm

$Currentvalue = $farm.Properties["BaristaTrustedLocations"];

Write-Host -foregroundcolor Green "The current value of the Barista Trusted Locations is: " $Currentvalue

$trustedLocations = "[{""Url"":""http://thegreen.treas.gov/do/ofs/"",""Description"":"""",""LocationType"":""Web"",""TrustChildren"":true},{""Url"":""http://thegreen.treas.gov/do/ofsteam/"",""Description"":"""",""LocationType"":""Web"",""TrustChildren"":true},{""Url"":""https://apps.treasury.ecm.gov/"",""Description"":"""",""LocationType"":""Web"",""TrustChildren"":true}]"
$farm.Properties["BaristaTrustedLocations"] = $trustedLocations;
$farm.Update();

$UpdatedValue =  $farm.Properties["BaristaTrustedLocations"]
Write-Host -foregroundcolor Green "Value of the Barista Trusted Locations has updated with " $UpdatedValue


Write-Host -foregroundcolor Green "Script has finished executing"