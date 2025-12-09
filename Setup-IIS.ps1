#Requires -RunAsAdministrator
# DigitalPrize IIS Setup Script
# Run this script as Administrator

$ErrorActionPreference = "Stop"

$publishPath = "c:\Dev\WorldPlay\DigitalPrize\publish"
$iisPath = "c:\inetpub\wwwroot\DigitalPrize"
$siteName = "DigitalPrize"
$apiAppPoolName = "DigitalPrizeApiPool"
$frontendAppPoolName = "DigitalPrizePool"
$apiPort = 5000
$frontendPort = 8080

Write-Host "=== DigitalPrize IIS Setup ===" -ForegroundColor Cyan

# Check if IIS is installed
$iisFeature = Get-WindowsOptionalFeature -Online -FeatureName IIS-WebServer
if ($iisFeature.State -ne "Enabled") {
    Write-Host "IIS is not installed. Installing..." -ForegroundColor Yellow
    Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServer -All -NoRestart
    Enable-WindowsOptionalFeature -Online -FeatureName IIS-ASPNET45 -All -NoRestart
}

# Import IIS module
Import-Module WebAdministration

# Create directories
Write-Host "Creating directories..." -ForegroundColor Green
New-Item -ItemType Directory -Path "$iisPath" -Force | Out-Null
New-Item -ItemType Directory -Path "$iisPath\api" -Force | Out-Null

# Copy files
Write-Host "Copying API files..." -ForegroundColor Green
Copy-Item -Path "$publishPath\api\*" -Destination "$iisPath\api" -Recurse -Force

Write-Host "Copying Frontend files..." -ForegroundColor Green
Copy-Item -Path "$publishPath\wwwroot\*" -Destination "$iisPath" -Recurse -Force

# Create App Pool for API
Write-Host "Creating API Application Pool..." -ForegroundColor Green
if (Test-Path "IIS:\AppPools\$apiAppPoolName") {
    Remove-WebAppPool -Name $apiAppPoolName
}
$apiPool = New-WebAppPool -Name $apiAppPoolName
$apiPool.managedRuntimeVersion = ""
$apiPool.processModel.identityType = "ApplicationPoolIdentity"
$apiPool | Set-Item

# Create App Pool for Frontend
Write-Host "Creating Frontend Application Pool..." -ForegroundColor Green
if (Test-Path "IIS:\AppPools\$frontendAppPoolName") {
    Remove-WebAppPool -Name $frontendAppPoolName
}
$frontendPool = New-WebAppPool -Name $frontendAppPoolName
$frontendPool.managedRuntimeVersion = ""
$frontendPool.processModel.identityType = "ApplicationPoolIdentity"
$frontendPool | Set-Item

# Remove existing site if exists
if (Test-Path "IIS:\Sites\$siteName") {
    Write-Host "Removing existing site..." -ForegroundColor Yellow
    Remove-Website -Name $siteName
}

# Create website for frontend
Write-Host "Creating Frontend Website on port $frontendPort..." -ForegroundColor Green
New-Website -Name $siteName -PhysicalPath $iisPath -ApplicationPool $frontendAppPoolName -Port $frontendPort -Force

# Create API application under the site
Write-Host "Creating API Application..." -ForegroundColor Green
New-WebApplication -Name "api" -Site $siteName -PhysicalPath "$iisPath\api" -ApplicationPool $apiAppPoolName

# Set permissions
Write-Host "Setting permissions..." -ForegroundColor Green
$acl = Get-Acl $iisPath
$identity = "IIS AppPool\$apiAppPoolName"
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule($identity, "ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.AddAccessRule($rule)
Set-Acl $iisPath $acl

$identity2 = "IIS AppPool\$frontendAppPoolName"
$rule2 = New-Object System.Security.AccessControl.FileSystemAccessRule($identity2, "ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.AddAccessRule($rule2)
Set-Acl $iisPath $acl

# Start the site
Write-Host "Starting website..." -ForegroundColor Green
Start-Website -Name $siteName

Write-Host ""
Write-Host "=== Setup Complete! ===" -ForegroundColor Cyan
Write-Host "Frontend URL: http://localhost:$frontendPort" -ForegroundColor Green
Write-Host "API URL: http://localhost:$frontendPort/api" -ForegroundColor Green
Write-Host ""
Write-Host "Note: Make sure URL Rewrite module is installed for SPA routing." -ForegroundColor Yellow
Write-Host "Download from: https://www.iis.net/downloads/microsoft/url-rewrite" -ForegroundColor Yellow
