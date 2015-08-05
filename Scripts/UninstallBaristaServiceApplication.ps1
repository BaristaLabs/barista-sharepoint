#SQL to locate "Orphaned" Barista Service Application Proxy instances:
#SELECT [Id]
#      ,[ClassId]
#      ,[ParentId]
#      ,[Name]
#      ,[Status]
#      ,[Version]
#      ,CAST([Properties] AS XML) AS Properties
#  FROM [SharePoint_Config].[dbo].[Objects] WITH (NOLOCK)
# WHERE [ClassId] LIKE 'FE65EF27-73F5-47C3-B23E-3D4FE5E10079'
#   AND Properties LIKE '%Barista.svc%'

[CmdletBinding(DefaultParameterSetName="FileOrDirectory")]
param (
	[Parameter(Mandatory=$false, Position=0, ParameterSetName="FileOrDirectory")]
	[ValidateNotNullOrEmpty()]
	[string]$ManagedAccount = "TREASURY\SP_WorkerProcess",

	[Parameter(Mandatory=$false, Position=2, ParameterSetName="FileOrDirectory")]
	[ValidateNotNullOrEmpty()]
	[string]$SPApplicationPoolName = "Barista Application Pool"
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

function WaitForJobToFinish([string]$JobTitle)
{ 
    
    $job = Get-SPTimerJob | where { $_.Title -like $JobTitle }
    if ($job -eq $null) 
    {
        #Write-Host 'Timer job not found'
    }
    elseif ($job.LastRunTime -ne [DateTime]::MinValue)
    {
        $JobLastRunTime = $job.LastRunTime
		$JobTitle = $job.Title
        Write-Host -NoNewLine "Waiting to finish job $JobTitle last run on $JobLastRunTime"
        
        while ($job.LastRunTime -eq $JobLastRunTime) 
        {
            Write-Host -NoNewLine .
            Start-Sleep -Seconds 2
        }
        Write-Host  "Finished waiting for job.."
    }
	return $job;
}

LoadSharePointPowerShellEnvironment

#
# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
# Unprovision service app proxies
# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
#

write-host 
write-host "[[STEP]] Uninstalling Barista Service Application Proxies." -foregroundcolor Yellow
write-host 

$serviceAppProxies = Get-SPServiceApplicationProxy | Where { $_.GetType().FullName -eq "Barista.SharePoint.Services.BaristaServiceApplicationProxy"}
if ($serviceAppProxy -ne $null) {
	foreach ($serviceAppProxy in $serviceAppProxies)
	{
		write-host "Unprovisioning service application proxy" $serviceAppProxy.DisplayName "..." -foregroundcolor Gray

		Remove-SPServiceApplicationProxy $serviceAppProxy -Confirm:$false
		#$serviceAppProxy.Unprovision()
		#$serviceAppProxy.Delete()
		$serviceAppProxy.Uncache()
		write-host 
	}
}

write-host "[[[[ Barista Service Application Proxies removed. ]]]]" -foregroundcolor Green

# [[[[[[[[STEP]]]]]]]]

write-host 
write-host "[[STEP]] Uninstalling Barista Service Application Instances." -foregroundcolor Yellow
write-host 

# Wait for any provisioning timer jobs to complete.
$tj = WaitForJobToFinish("Provisioning Barista Service*")
if ($tj -ne $null) {
	$tj.Delete()
}

$tj = WaitForJobToFinish("Disabling Barista Service*")
if ($tj -ne $null) {
	$tj.Delete()
}

$serviceInstances = Get-SPServiceInstance | Where { $_.GetType().FullName -eq "Barista.SharePoint.Services.BaristaServiceInstance" }
if ($serviceInstances -ne $null) {
	foreach ($serviceInstance in $serviceInstances) {

		write-host "Stopping service instance" $serviceInstance.DisplayName "on" $serviceInstance.Server.Address "..." -foregroundcolor Gray
		Stop-SPServiceInstance $serviceInstance -Confirm:$false
		write-host "Barista Service Application instance stopped." -foregroundcolor Green

		$serviceInstance.Unprovision()
		write-host "Barista Service Application instance unprovisioned." -foregroundcolor Green

		$tj = WaitForJobToFinish("Provisioning Barista Service*")
		if ($tj -ne $null) {
			$tj.Delete()
		}

		$tj = WaitForJobToFinish("Disabling Barista Service*")
		if ($tj -ne $null) {
			$tj.Delete()
		}

		$serviceInstance.Unprovision()
		$serviceInstance.Delete()
		$serviceInstance.Uncache()
		write-host "Barista Service Application instance" $serviceInstance.DisplayName "on" $serviceInstance.Server.Address "removed." -foregroundcolor Green
	}
}

write-host "[[[[ Barista Service Application instances removed. ]]]]" -foregroundcolor Green

#
# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
# Unprovision service apps
# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
#

write-host 
write-host "[[STEP]] Uninstalling Barista Service Applications." -foregroundcolor Yellow
write-host 

$serviceApps = Get-SPServiceApplication | Where { $_.GetType().FullName -eq "Barista.SharePoint.Services.BaristaServiceApplication" }
if ($serviceApps -ne $null) {
	foreach ($serviceApp in $serviceApps) {
			write-host "Unprovisioning service application" $serviceApp.DisplayName "..." -foregroundcolor Gray

			Remove-SPServiceApplication $serviceApp -RemoveData -Confirm:$false
			#$serviceApp.Unprovision()
			#$serviceApp.Delete()
			$serviceApp.Uncache()
			write-host 
	}
}


write-host "[[[[ Barista Service Applications uninstalled. ]]]]" -foregroundcolor Green

write-host 
write-host "[[STEP]] Uninstalling Barista Service Application Pool" -foregroundcolor Yellow
write-host 

$appPool = Get-SPServiceApplicationPool | Where {$_.Name -eq $SPApplicationPoolName}
if ($appPool -ne $null) {
	Remove-SPServiceApplicationPool $appPool -Confirm:$false
	#$serviceApp.Unprovision()
	#$serviceApp.Delete()
	$appPool.Uncache()

	write-host 
}

write-host "[[[[ Barista Service Application Pool uninstalled. ]]]]" -foregroundcolor Green