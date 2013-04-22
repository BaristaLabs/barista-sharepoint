function LoadSharePointPowerShellEnvironment
{
	write-host 
	write-host "Setting up PowerShell environment for SharePoint" -foregroundcolor Yellow
	write-host 
	Add-PSSnapin "Microsoft.SharePoint.PowerShell" -ErrorAction SilentlyContinue
	write-host "SharePoint PowerShell Snapin loaded." -foregroundcolor Green
}

function Get-WebPage([string]$url)
{
    $wc = new-object net.webclient;
    $wc.credentials = [System.Net.CredentialCache]::DefaultCredentials;
    $pageContents = $wc.DownloadStringAsync($url);
	# Wait for a small delay to let the request process...
	Start-Sleep -Second 5
    $wc.Dispose();
    return $pageContents;
}

write-host
LoadSharePointPowerShellEnvironment

# Enumerate the web app along with the site collections within it, and send a request to each one of them
Get-SPWebApplication | ForEach-Object {
	write-host "Warming Up" $_.Url;
	$html = get-webpage -url $_.Url -cred $cred;
}

