Add-PSSnapin "Microsoft.SharePoint.PowerShell" -ErrorAction SilentlyContinue
$farm = Get-SPFarm

$Currentvalue = $farm.Properties["BaristaTrustedLocations"];

Write-Host -foregroundcolor Green "The current value of the Barista Trusted Locations is: " $Currentvalue

$trustedLocations = "[{""Url"":""http://ofs.treasuryecm.gov"",""Description"":"""",""LocationType"":""Web"",""TrustChildren"":true},{""Url"":""http://ofs.treasuryecm.gov/ams/"",""Description"":"""",""LocationType"":""Web"",""TrustChildren"":true},{""Url"":""https://ofs.treasuryecm.gov/ams-test"",""Description"":"""",""LocationType"":""Web"",""TrustChildren"":true}]"
$farm.Properties["BaristaTrustedLocations"] = $trustedLocations;
$farm.Update();

$UpdatedValue =  $farm.Properties["BaristaTrustedLocations"]
Write-Host -foregroundcolor Green "Value of the Barista Trusted Locations has updated with " $UpdatedValue


Write-Host -foregroundcolor Green "Script has finished executing"