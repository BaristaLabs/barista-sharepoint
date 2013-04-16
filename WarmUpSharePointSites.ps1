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
    $pageContents = $wc.DownloadString($url);
    $wc.Dispose();
    return $pageContents;
}

write-host 
LoadSharePointPowerShellEnvironment

# Enumerate the web app along with the site collections within it, and send a request to each one of them
foreach ($site in Get-SPSite)
{
	write-host $site.Url;
	$html = get-webpage -url $site.Url -cred $cred;
}
