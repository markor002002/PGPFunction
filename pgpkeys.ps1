#
#Install Modules
#Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope LocalMachine
#Install-Module -Name PSPGP
#Import-Module PSPGP

#az login
#az account set --subscription "My Demos" 
#create subfolder key

#name of keyvault
$keyvault = 'kvfunction'
#name of azure function
$functioname = 'pgp12345'

$keyname = Read-Host -Prompt 'Input your project name'
$passcode = Read-Host -Prompt 'Enter your passcode'

Write-Host "You key name: '$keyname' and passphrase: '$passcode'"


New-PGPKey -FilePathPublic $PSScriptRoot\Keys\PGP-$keyname-public.asc -FilePathPrivate $PSScriptRoot\Keys\PGP-$keyname-private.asc -UserName $env:username 
#-Password $passcode

#$publicPGPKey = ConvertTo-Base64 -Path $PSScriptRoot\Keys\PGP-$keyname-public.asc
#$privatePGPKey = ConvertTo-Base64 -Path $PSScriptRoot\Keys\PGP-$keyname-private.asc


$file = "$PSScriptRoot\Keys\PGP-$keyname-public.asc"
$bytes = Get-Content $file -Encoding Byte
$publicPGPKey = [Convert]::ToBase64String($bytes)

$file = "$PSScriptRoot\Keys\PGP-$keyname-private.asc"
$bytes = Get-Content $file -Encoding Byte
$privatePGPKey = [Convert]::ToBase64String($bytes)

az keyvault secret set --name "PGP-$keyname-passcode"  --vault-name "$keyvault" --value "$passcode"
az keyvault secret set --name "PGP-$keyname-public"  --vault-name "$keyvault" --value $publicPGPKey --encoding base64
az keyvault secret set --name "PGP-$keyname-private"  --vault-name "$keyvault" --value $privatePGPKey --encoding base64

$MyFunctionApp = 'pgp12345'
$MyFunctionAppRG ='pgp12345_group'

az functionapp config appsettings set --name $MyFunctionApp --resource-group $MyFunctionAppRG --settings `""PGP-$keyname-passcode=@Microsoft.KeyVault(VaultName=$keyvault;SecretName=PGP-$keyname-passcode)"`"
az functionapp config appsettings set --name $MyFunctionApp --resource-group $MyFunctionAppRG --settings `""PGP-$keyname-public=@Microsoft.KeyVault(VaultName=$keyvault;SecretName=PGP-$keyname-public)"`"
az functionapp config appsettings set --name $MyFunctionApp --resource-group $MyFunctionAppRG --settings `""PGP-$keyname-private=@Microsoft.KeyVault(VaultName=$keyvault;SecretName=PGP-$keyname-private)"`"



