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
	write-host 
	write-host "Setting up PowerShell environment for SharePoint" -foregroundcolor Yellow
	write-host 
	Add-PSSnapin "Microsoft.SharePoint.PowerShell" -ErrorAction SilentlyContinue
	write-host "SharePoint PowerShell Snapin loaded." -foregroundcolor Green
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

write-host 
LoadSharePointPowerShellEnvironment


# [[[[[[[[STEP]]]]]]]]

write-host 
write-host "[[STEP]] Stopping Barista Service Application instance on local server." -foregroundcolor Yellow
write-host 

write-host "Stopping the service on  $env:computername..." -foregroundcolor Gray
$localServiceInstance = Get-SPServiceInstance -Server $env:computername | where { $_.GetType().FullName -eq "Barista.SharePoint.Services.BaristaServiceInstance" -and $_.Name -eq "" }
if ($localServiceInstance -ne $null){

	$tj = WaitForJobToFinish("Provisioning Barista Service*")
	if ($tj -ne $null) {
		$tj.Delete()
	}

	$tj = WaitForJobToFinish("Disabling Barista Service*")
	if ($tj -ne $null) {
		$tj.Delete()
	}

	write-host "Stopping service instance on server $env:computername..." -foregroundcolor Gray
	Stop-SPServiceInstance $localServiceInstance -Confirm:$false
	write-host "Barista Service Application instance stopped." -foregroundcolor Green

	$localServiceInstance.Unprovision()
	write-host "Barista Service Application instance unprovisioned." -foregroundcolor Green

	$tj = WaitForJobToFinish("Provisioning Barista Service*")
	if ($tj -ne $null) {
		$tj.Delete()
	}

	$tj = WaitForJobToFinish("Disabling Barista Service*")
	if ($tj -ne $null) {
		$tj.Delete()
	}

	$localServiceInstance.Delete()
	write-host "Barista Service Application instance removed." -foregroundcolor Green
}

write-host "[[[[ Barista Service Application stopped. ]]]]" -foregroundcolor Green

#
# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
# Unprovision service app proxy
# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
#

write-host 
write-host "[[STEP]] Uninstalling Barista Service Application Proxy." -foregroundcolor Yellow
write-host 

$serviceAppProxy = Get-SPServiceApplicationProxy | where { $_.GetType().FullName -eq "Barista.SharePoint.Services.BaristaServiceApplicationProxy" -and $_.Name -eq "Barista Service Application Proxy" }
if ($serviceAppProxy -ne $null)
{
	write-host "Unprovisioning service application proxy..." -foregroundcolor Gray

	$serviceAppProxy.Unprovision()
	$serviceAppProxy.Delete()
	write-host 
}

#
# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
# Unprovision service app
# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
#

write-host 
write-host "[[STEP]] Uninstalling Barista Service Application." -foregroundcolor Yellow
write-host 

write-host "Ensure service application is removed..." -foregroundcolor Gray
$serviceApp = Get-SPServiceApplication | where { $_.GetType().FullName -eq "Barista.SharePoint.Services.BaristaServiceApplication" -and $_.Name -eq "Barista Service Application" }
if ($serviceApp -ne $null) {
	write-host "Unprovisioning service application..." -foregroundcolor Gray

	$serviceApp.Unprovision()
	$serviceApp.Delete()
	#Remove-SPServiceApplication $serviceApp -RemoveData -Confirm:$false

	write-host 
}


write-host "[[[[ Barista Service Application uninstalled. ]]]]" -foregroundcolor Green

write-host 