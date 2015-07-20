function LoadSharePointPowerShellEnvironment
{
	$ver = $host | select version
	if ($ver.Version.Major -gt 1)  {$Host.Runspace.ThreadOptions = "ReuseThread"}
	if ( (Get-PSSnapin -Name  Microsoft.SharePoint.PowerShell -ErrorAction SilentlyContinue) -eq $null )
	{
		Add-PsSnapin  Microsoft.SharePoint.PowerShell
	}
}

function Block-SPDeployment($solution, [bool]$deploying, [string]$status, [int]$percentComplete) {
	do { 
		Start-Sleep 2
		Write-Progress -Activity "Uninstalling solution $($solution.Name)" -Status $status -PercentComplete $percentComplete
		$solution = Get-SPSolution $solution
		if ($solution.LastOperationResult -like "*Failed*") { throw "An error occurred during the solution retraction, deployment, or update." }
		if (!$solution.JobExists -and (($deploying -and $solution.Deployed) -or (!$deploying -and !$solution.Deployed))) { break }
	} while ($true)
	sleep 5
}

function RetractSolution($solution) {
	if ($solution -eq $null) {
		return;
	}

	#Retract the solution
	if ($solution.Deployed) {
		Write-Progress -Activity "Uninstalling solution $name" -Status "Retracting $name" -PercentComplete 0
		if ($solution.ContainsWebApplicationResource) {
			$solution | Uninstall-SPSolution -AllWebApplications -Confirm:$false
		} else {
			$solution | Uninstall-SPSolution -Confirm:$false
		}
		#Block until we're sure the solution is no longer deployed.
		Block-SPDeployment $solution $false "Retracting $name" 12
		Write-Progress -Activity "Uninstalling solution $name" -Status "Solution retracted" -PercentComplete 25
	}

	if ($solution.Added) {
		#Delete the solution
		Write-Progress -Activity "Uninstalling solution $name" -Status "Removing $name" -PercentComplete 30
		Get-SPSolution $name | Remove-SPSolution -Confirm:$false
		Write-Progress -Activity "Uninstalling solution $name" -Status "Solution removed" -PercentComplete 50
	}
}

write-host 
LoadSharePointPowerShellEnvironment

#Retract/Remove any associated solutions that may have dependencies on Barista.
$solution = Get-SPSolution "OFS.AMS.SharePoint.wsp" -ErrorAction SilentlyContinue
RetractSolution($solution);

$solution = Get-SPSolution "Barista.SharePoint.WebParts.wsp" -ErrorAction SilentlyContinue
RetractSolution($solution);

$solution = Get-SPSolution "Barista.SharePoint.WorkflowActivities.wsp" -ErrorAction SilentlyContinue
RetractSolution($solution);

$solution = Get-SPSolution "OFS.CAT.SharePoint.Core.wsp" -ErrorAction SilentlyContinue
RetractSolution($solution);

$solution = Get-SPSolution -id "90fb8db4-2b5f-4de7-882b-6faba092942c" -ErrorAction SilentlyContinue
RetractSolution($solution);

$solution = Get-SPSolution "Barista.SharePoint.wsp" -ErrorAction SilentlyContinue
RetractSolution($solution);


