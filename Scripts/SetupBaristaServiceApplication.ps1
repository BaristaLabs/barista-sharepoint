[CmdletBinding(DefaultParameterSetName="FileOrDirectory")]
param (
	[Parameter(Mandatory=$false, Position=0, ParameterSetName="FileOrDirectory")]
	[ValidateNotNullOrEmpty()]
	[string]$ManagedAccount = "TREASURY\SP_WorkerProcess",

	[Parameter(Mandatory=$false, Position=1, ParameterSetName="FileOrDirectory")]
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

write-host 
LoadSharePointPowerShellEnvironment

#
# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
# Ensure Barista Application Pool has been created.
# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
#

write-host 
write-host "[[STEP]] Ensuring Barista Application Pool Exists." -foregroundcolor Yellow
write-host 

$appPool = Get-SPServiceApplicationPool | Where {$_.Name -eq $SPApplicationPoolName}

if($appPool -eq $null) {
	write-host "Creating Barista Application Pool..." -foregroundcolor Gray
    $appPool = New-SPServiceApplicationPool -Name $SPApplicationPoolName -Account $ManagedAccount
	if ($appPool -ne $null) {
	    write-host "Barista Application Pool created.." -foregroundcolor Green
	}
}

#
# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
# Provision service app & start service app instance
# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
#

write-host 
write-host "[[STEP]] Creating Barista Service Application." -foregroundcolor Yellow
write-host 

write-host "Ensure service application not already created..." -foregroundcolor Gray
$serviceApp = Get-SPServiceApplication | where { $_.GetType().FullName -eq "Barista.SharePoint.Services.BaristaServiceApplication" -and $_.Name -eq "Barista Service Application" }
if ($serviceApp -eq $null){
	write-host "Creating service application..." -foregroundcolor Gray
	$guid = [Guid]::NewGuid()
	$serviceApp = New-BaristaServiceApplication -Name "Barista Service Application" -ApplicationPool $SPApplicationPoolName
    if ($serviceApp -ne $null) {
	    write-host "Barista Service Application created." -foregroundcolor Green
	}
}


# [[[[[[[[STEP]]]]]]]]


write-host 
write-host "[[STEP]] Configuring permissions on Barista Service Application." -foregroundcolor Yellow
write-host 

write-host "Configure permissions on the service app..." -foregroundcolor Gray
$user = $env:userdomain + '\' + $env:username

write-host "  Creating new claim for $user..." -foregroundcolor Gray
$userClaim = New-SPClaimsPrincipal -Identity $user -IdentityType WindowsSamAccountName
$security = Get-SPServiceApplicationSecurity $serviceApp

write-host "  Granting $user 'FULL CONTROL' to service application..." -foregroundcolor Gray
Grant-SPObjectSecurity $security $userClaim -Rights "Full Control"
Set-SPServiceApplicationSecurity $serviceApp $security

write-host "Barista Service Application permissions set." -foregroundcolor Green

# [[[[[[[[STEP]]]]]]]]

write-host 
write-host "[[STEP]] Starting Barista Service Application instance on local server." -foregroundcolor Yellow
write-host 

write-host "Ensure service instance is running on server $env:computername..." -foregroundcolor Gray

$serviceInstanceRetry = 0
do {
	if ($serviceInstanceRetry -gt 0) {
		write-host "Waiting"($serviceInstanceRetry * 10)"s for Barista Service Instance deployment..." -foregroundcolor Yellow
		Start-Sleep -s ($serviceInstanceRetry * 10)
	}
	$localServiceInstance = Get-SPServiceInstance -Server $env:computername | where { $_.GetType().FullName -eq "Barista.SharePoint.Services.BaristaServiceInstance" -and $_.Name -eq "BaristaServiceInstance" }
}
while(++$serviceInstanceRetry -le 6 -and $localServiceInstance -eq $null)

if ($localServiceInstance -eq $null) {
	throw "An instance of the Barista Service could not be located within the allocated time. Please redeploy the Barista Solution."
}

if ($localServiceInstance.Status -eq 'Provisioning') {
	$tj = Get-SPTimerJob | where { $_.Title -like "Provisioning Barista Service*" }
	if ($tj -ne $null) {
		$tj.Delete()
	}

	$localServiceInstance.Unprovision()
}

if ($localServiceInstance.Status -eq 'Unprovisioning') {
	$tj = Get-SPTimerJob | where { $_.Title -like "Disabling Barista Service*" }
	if ($tj -ne $null) {
		$tj.Delete()
	}
	$localServiceInstance.Unprovision()
}

if ($localServiceInstance.Status -ne 'Started'){
	write-host "Starting service instance on server $env:computername..." -foregroundcolor Gray
	#Start-SPServiceInstance $localServiceInstance
	$localServiceInstance.Provision()
	write-host "Barista Service Application instance started." -foregroundcolor Green
}

write-host "[[[[ Barista Service Application provisioned & instance started. ]]]]" -foregroundcolor Green

write-host 