function global:Deploy-SPSolutions() {
  <#
  .Synopsis
    Deploys one or more Farm Solution Packages to the Farm.
  .Description
    Specify either a directory containing WSP files, a single WSP file, or an XML configuration file containing the WSP files to deploy.
    If using an XML configuration file, the format of the file must match the following:
      <Solutions>    
        <Solution Path="<full path and filename to WSP>" UpgradeExisting="false">
          <WebApplications>            
            <WebApplication>http://example.com/</WebApplication>        
          </WebApplications>    
        </Solution>
      </Solutions>
    Multiple <Solution> and <WebApplication> nodes can be added. The UpgradeExisting attribute is optional and should be specified if the WSP should be udpated and not retracted and redeployed.
  .Example
    PS C:\> . .\Deploy-SPSolutions.ps1
    PS C:\> Deploy-SPSolutions -Identity C:\WSPs -WebApplication http://demo
    
    This example loads the function into memory and then deploys all the WSP files in the specified directory to the http://demo Web Application (if applicable).
  .Example
    PS C:\> . .\Deploy-SPSolutions.ps1
    PS C:\> Deploy-SPSolutions -Identity C:\WSPs -WebApplication http://demo,http://mysites
    
    This example loads the function into memory and then deploys all the WSP files in the specified directory to the http://demo and http://mysites Web Applications (if applicable).
  .Example
    PS C:\> . .\Deploy-SPSolutions.ps1
    PS C:\> Deploy-SPSolutions -Identity C:\WSPs -AllWebApplications
    
    This example loads the function into memory and then deploys all the WSP files in the specified directory to all Web Applications (if applicable).
  .Example
    PS C:\> . .\Deploy-SPSolutions.ps1
    PS C:\> Deploy-SPSolutions -Identity C:\WSPs\MyCustomSolution.wsp -AllWebApplications
    
    This example loads the function into memory and then deploys the specified WSP to all Web Applications (if applicable).
  .Example
    PS C:\> . .\Deploy-SPSolutions.ps1
    PS C:\> Deploy-SPSolutions -Identity C:\WSPs\MyCustomSolution.wsp -AllWebApplications -UpgradeExisting
    
    This example loads the function into memory and then deploys the specified WSP to all Web Applications (if applicable); existing deployments will be upgraded and not retracted and redeployed.
  .Example
    PS C:\> . .\Deploy-SPSolutions.ps1
    PS C:\> Deploy-SPSolutions C:\Solutions.xml
    
    This example loads the function into memory and then deploys all the WSP files specified by the Solutions.xml configuration file.
  .Parameter Config
    The XML configuration file containing the WSP files to deploy.
  .Parameter Identity
    The directory, WSP file, or XML configuration file containing the WSP files to deploy.
  .Parameter UpgradeExisting
    If specified, the WSP file(s) will be updated and not retracted and redeployed (if the WSP does not exist in the Farm then this parameter has no effect).
  .Parameter AllWebApplications
    If specified, the WSP file(s) will be deployed to all Web Applications in the Farm (if applicable).
  .Parameter WebApplication
    Specifies the Web Application(s) to deploy the WSP file to.
  .Link
    Get-Content
    Get-SPSolution
    Add-SPSolution
    Install-SPSolution
    Update-SPSolution
    Uninstall-SPSolution
    Remove-SPSolution
  #>
  [CmdletBinding(DefaultParameterSetName="FileOrDirectory")]
  param (
    [Parameter(Mandatory=$true, Position=0, ParameterSetName="Xml")]
    [ValidateNotNullOrEmpty()]
    [xml]$Config,

    [Parameter(Mandatory=$true, Position=0, ParameterSetName="FileOrDirectory")]
    [ValidateNotNullOrEmpty()]
    [string]$Identity,
    
    [Parameter(Mandatory=$false, Position=1, ParameterSetName="FileOrDirectory")]
    [switch]$UpgradeExisting,
    
    [Parameter(Mandatory=$false, Position=2, ParameterSetName="FileOrDirectory")]
    [switch]$AllWebApplications,
    
    [Parameter(Mandatory=$false, Position=3, ParameterSetName="FileOrDirectory")]
    [Microsoft.SharePoint.PowerShell.SPWebApplicationPipeBind[]]$WebApplication
  )
  function Block-SPDeployment($solution, [bool]$deploying, [string]$status, [int]$percentComplete) {
    do { 
      Start-Sleep 2
      Write-Progress -Activity "Deploying solution $($solution.Name)" -Status $status -PercentComplete $percentComplete
      $solution = Get-SPSolution $solution
      if ($solution.LastOperationResult -like "*Failed*") { throw "An error occurred during the solution retraction, deployment, or update." }
      if (!$solution.JobExists -and (($deploying -and $solution.Deployed) -or (!$deploying -and !$solution.Deployed))) { break }
    } while ($true)
    sleep 5  
  }
  switch ($PsCmdlet.ParameterSetName) { 
    "Xml" { 
      # An XML document was provided so iterate through all the defined solutions and call the other parameter set version of the function
      $Config.Deployment.Solutions.Solution | ForEach-Object {
        if ($_ -ne $null) {
            [string]$path = $_.Path
            [bool]$upgrade = $false
            if (![string]::IsNullOrEmpty($_.UpgradeExisting)) {
              $upgrade = [bool]::Parse($_.UpgradeExisting)
            }
            $webApps = $_.WebApplications.WebApplication
            Deploy-SPSolutions -Identity $path -UpgradeExisting:$upgrade -WebApplication $webApps -AllWebApplications:$(($webApps -eq $null) -or ($webApps.Length -eq 0))
        }
      }
      # Iterate through all 'features' and either activate or deactivate based on identity
      $Config.Deployment.Features.Feature | ForEach-Object {
        if ($_ -ne $null) {
            $identity = $_.Identity
            $url = $_.Url
            [string]$action = $_.Action
        
            switch ($action) {
                "Enable" {
					if ($url -ne $null) {
						Write-Progress -Activity "Enabling Feature $identity on $url" -Status "Enabling $identity" -PercentComplete -1
						$args = @"
Add-PSSnapin "Microsoft.SharePoint.PowerShell" -ErrorAction SilentlyContinue
Enable-SPFeature –Identity $($identity) -Url $($url)
"@
						& powershell.exe -Version 2 $args
						Write-Progress -Activity "Enabling Farm Feature $identity" -Status "Enabled" -Completed
					}
					else {
						Write-Progress -Activity "Enabling Farm Feature $identity" -Status "Enabling $identity" -PercentComplete -1
						$args = @"
Add-PSSnapin "Microsoft.SharePoint.PowerShell" -ErrorAction SilentlyContinue
Enable-SPFeature –Identity $($identity)
"@
						& powershell.exe -Version 2 $args
						Write-Progress -Activity "Enabling Farm Feature $identity" -Status "Enabled" -Completed
					}
                }
                "Disable" {
					try {
					if ($url -ne $null) {
						Write-Progress -Activity "Disabling Feature $identity on $url" -Status "Disabling $identity" -PercentComplete -1
						$args = @"
Add-PSSnapin "Microsoft.SharePoint.PowerShell" -ErrorAction SilentlyContinue
Disable-SPFeature –Identity $($identity) -url $($url) -Confirm:0
"@
						& powershell.exe -Version 2 {$args}
						Write-Progress -Activity "Disabling Feature $identity on $url" -Status "Disabled" -Completed
					}
					else {
						Write-Progress -Activity "Disabling Farm Feature $identity" -Status "Disabling $identity" -PercentComplete -1
						$args = @"
Add-PSSnapin "Microsoft.SharePoint.PowerShell" -ErrorAction SilentlyContinue
Disable-SPFeature –Identity $($identity) -Confirm:0
"@
						& powershell.exe -Version 2 $args
						Write-Progress -Activity "Disabling Farm Feature $identity" -Status "Disabled" -Completed
					}
					}
					catch {
					}
                }
            }
        }
      }
      break
    }
    "FileOrDirectory" {
      $item = Get-Item (Resolve-Path $Identity)
      if ($item -is [System.IO.DirectoryInfo]) {
        # A directory was provided so iterate through all files in the directory and deploy if the file is a WSP (based on the extension)
        Get-ChildItem $item | ForEach-Object {
          if ($_.Name.ToLower().EndsWith(".wsp")) {
            Deploy-SPSolutions -Identity $_.FullName -UpgradeExisting:$UpgradeExisting -WebApplication $WebApplication
          }
        }
      } elseif ($item -is [System.IO.FileInfo]) {
        # A specific file was provided so assume that the file is a WSP if it does not have an XML extension.
        [string]$name = $item.Name
        
        if ($name.ToLower().EndsWith(".xml")) {
          Deploy-SPSolutions -Config ([xml](Get-Content $item.FullName))
          return
        }
        $solution = Get-SPSolution $name -ErrorAction SilentlyContinue
        
        if ($solution -ne $null -and $UpgradeExisting) {
          # Just update the solution, don't retract and redeploy.
          Write-Progress -Activity "Deploying solution $name" -Status "Updating $name" -PercentComplete -1
          $solution | Update-SPSolution -CASPolicies:$($solution.ContainsCasPolicy) `
            -GACDeployment:$($solution.ContainsGlobalAssembly) `
            -LiteralPath $item.FullName
        
          Block-SPDeployment $solution $true "Updating $name" -1
          Write-Progress -Activity "Deploying solution $name" -Status "Updated" -Completed
          
          return
        }

        if ($solution -ne $null) {
          #Retract the solution
          if ($solution.Deployed) {
            Write-Progress -Activity "Deploying solution $name" -Status "Retracting $name" -PercentComplete 0
            if ($solution.ContainsWebApplicationResource) {
              $solution | Uninstall-SPSolution -AllWebApplications -Confirm:$false
            } else {
              $solution | Uninstall-SPSolution -Confirm:$false
            }
            #Block until we're sure the solution is no longer deployed.
            Block-SPDeployment $solution $false "Retracting $name" 12
            Write-Progress -Activity "Deploying solution $name" -Status "Solution retracted" -PercentComplete 25
          }

          #Delete the solution
          Write-Progress -Activity "Deploying solution $name" -Status "Removing $name" -PercentComplete 30
          Get-SPSolution $name | Remove-SPSolution -Confirm:$false
          Write-Progress -Activity "Deploying solution $name" -Status "Solution removed" -PercentComplete 50
        }

        #Add the solution
        Write-Progress -Activity "Deploying solution $name" -Status "Adding $name" -PercentComplete 50
        $solution = Add-SPSolution $item.FullName
        Write-Progress -Activity "Deploying solution $name" -Status "Solution added" -PercentComplete 75

        #Deploy the solution
        
        if (!$solution.ContainsWebApplicationResource) {
          Write-Progress -Activity "Deploying solution $name" -Status "Installing $name" -PercentComplete 75
          $solution | Install-SPSolution -GACDeployment:$($solution.ContainsGlobalAssembly) -CASPolicies:$($solution.ContainsCasPolicy) -Confirm:$false -Force:$true
          Block-SPDeployment $solution $true "Installing $name" 85
        } else {
          if ($WebApplication -eq $null -or $WebApplication.Length -eq 0) {
            Write-Progress -Activity "Deploying solution $name" -Status "Installing $name to all Web Applications" -PercentComplete 75
            $solution | Install-SPSolution -GACDeployment:$($solution.ContainsGlobalAssembly) -CASPolicies:$($solution.ContainsCasPolicy) -AllWebApplications -Force:$true -Confirm:$false
            Block-SPDeployment $solution $true "Installing $name to all Web Applications" 85
          } else {
            $WebApplication | ForEach-Object {
              $webApp = $_.Read()
              Write-Progress -Activity "Deploying solution $name" -Status "Installing $name to $($webApp.Url)" -PercentComplete 75
              $solution | Install-SPSolution -GACDeployment:$gac -CASPolicies:$cas -WebApplication $webApp -Confirm:$false -Force:$true
              Block-SPDeployment $solution $true "Installing $name to $($webApp.Url)" 85
            }
          }
        }
        Write-Progress -Activity "Deploying solution $name" -Status "Deployed" -Completed
      }
      break 
    }
  } 
}
