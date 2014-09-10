Write-Host
Write-Host Removing Orphaned IIS Barista Endpoints.

Import-Module "WebAdministration"

#Include the SharePoint cmdlets
if ((Get-PSSnapin "Microsoft.SharePoint.PowerShell" -ErrorAction SilentlyContinue) -eq $null)
{
	Add-PSSnapin "Microsoft.SharePoint.PowerShell" -ErrorAction Inquire -WarningAction Inquire
} 

$sa = Get-SPServiceApplication | Where { $_.Name -ieq "Barista Service Application" }
if ($sa -ne $null) {
    $webApps = Get-WebApplication | Where { $_.PhysicalPath.EndsWith("Barista") -and $_.Path -ne ("/" + $sa.Id.ToString().Substring(0,36).Replace("-",""))}

    if($webApps -ne $null) {
		Write-Host
		Write-Host "Backing up the IIS Metabase.."
		
		#backup IIS Metabase 
		$folderName = "IISBackup{0}" -f [DateTime]::Today.Ticks
		Backup-WebConfiguration -Name $folderName

        foreach($webApp in $webApps) {
	        Write-Host "Removing " $webApp.Path
	        $webAppName = $webApp.path.Replace("/","")
	        Remove-WebApplication -Site "SharePoint Web Services" -Name $webAppName
        }
    }
    else {
		Write-Host
        Write-Host "No Orphan Web Applications Found."
    }
}