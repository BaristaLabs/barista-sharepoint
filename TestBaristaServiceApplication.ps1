[CmdletBinding(DefaultParameterSetName="FileOrDirectory")]
param (
	[Parameter(Mandatory=$false, Position=0, ParameterSetName="FileOrDirectory")]
	[ValidateNotNullOrEmpty()]
	[string]$Uri = "http://ofsdev"
)

function LoadSharePointPowerShellEnvironment
{
	write-host 
	write-host "Setting up PowerShell environment for SharePoint" -foregroundcolor Yellow
	write-host 
	Add-PSSnapin "Microsoft.SharePoint.PowerShell" -ErrorAction SilentlyContinue
	write-host "SharePoint PowerShell Snapin loaded." -foregroundcolor Green
}

#
# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
# Test the custom service application
# +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
#

write-host 
LoadSharePointPowerShellEnvironment

write-host "Testing Barista Service Application..." -foregroundcolor Gray

write-host "   Using service context of site $SiteUri..." -foregroundcolor Gray

write-host

write-host "Testing the service application..." -foregroundcolor Yellow
write-host "Evaling 6*7..." -foregroundcolor Gray
$result = Invoke-BaristaService -ServiceContext $Uri -Eval "6*7"
write-host "Result of 6*7 = $result" -foregroundcolor Gray

if ($result -eq "42") {
	write-host "Barista Service Application working." -foregroundcolor Green
}
else {
	write-host "An error occurred while executing the Barista Service." -foregroundcolor Yellow
}