.\gacutil -i Barista.Core.dll
.\gacutil -i Barista.SharePoint.dll
.\gacutil -i Barista.SharePoint.Core.dll
.\RemoveBaristaTypes.ps1

Write-Host "Removing objects directly"
stsadm -o deleteconfigurationobject -id E6FF6D3D-647D-4D97-AE39-173CA64F6281
stsadm -o deleteconfigurationobject -id CACA603B-1AE3-4DE0-9A96-837359939439
stsadm -o deleteconfigurationobject -id F00C88C9-CFFF-46B3-92FF-BECFF78DDA12
stsadm -o deleteconfigurationobject -id 80244CA3-8273-4323-8B55-398FCA0EEBCF

Write-Host "Exporting persisted objects..."
.\exportPersistedObjects.ps1
.\gacutil -u Barista.SharePoint.Core
.\gacutil -u Barista.SharePoint
.\gacutil -u Barista.Core