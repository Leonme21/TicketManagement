param (
    [string]$JavaPath = ""
)

$SonarUrl = "http://localhost:9000"
$SonarLogin = "admin"
$SonarPassword = "admin" # Default password
$ProjectKey = "TicketManagement"

# Setup Java Path if provided
if ($JavaPath -ne "") {
    Write-Host "Using provided Java path: $JavaPath" -ForegroundColor Cyan
    $env:JAVA_HOME = $JavaPath
    $env:Path = "$JavaPath\bin;" + $env:Path
}

# Check for Java (Required for SonarScanner)
$UserJavaPath = "$env:LOCALAPPDATA\Programs\Eclipse Adoptium\jdk-17.0.18.8-hotspot"

try {
    java --version | Out-Null
    Write-Host "Java detected in PATH." -ForegroundColor Green
}
catch {
    Write-Host "Java not found in PATH. Checking common locations..." -ForegroundColor Yellow
    
    if (Test-Path "$UserJavaPath\bin\java.exe") {
        Write-Host "Java found at: $UserJavaPath" -ForegroundColor Green
        $env:JAVA_HOME = $UserJavaPath
        $env:Path = "$UserJavaPath\bin;" + $env:Path
    }
    else {
        if ($JavaPath -eq "") {
            Write-Error "Java not found. Please install Java 17 or provide the path using -JavaPath."
            Write-Warning "Example: .\analyze.ps1 -JavaPath 'C:\Program Files\Eclipse Adoptium\jdk-17...'"
            exit 1
        }
    }
}

# Check/Install SonarScanner
$scannerInstalled = dotnet tool list -g | Select-String "dotnet-sonarscanner"
if (-not $scannerInstalled) {
    Write-Host "Installing SonarScanner for .NET..."
    dotnet tool install --global dotnet-sonarscanner
}
else {
    Write-Host "SonarScanner already installed." -ForegroundColor Green
}

# Wait for SonarQube to be ready
Write-Host "Checking if SonarQube is ready at $SonarUrl..."
$retries = 30
$sleeping = 5
$ready = $false

for ($i = 0; $i -lt $retries; $i++) {
    try {
        $response = Invoke-WebRequest -Uri "$SonarUrl/api/system/status" -UseBasicParsing -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            $content = $response.Content | ConvertFrom-Json
            if ($content.status -eq "UP") {
                $ready = $true
                break
            }
        }
    }
    catch {
        # Ignore connection errors while waiting
    }
    Write-Host "Waiting for SonarQube to start... ($($i+1)/$retries)"
    Start-Sleep -Seconds $sleeping
}

if (-not $ready) {
    Write-Error "SonarQube failed to start or is not reachable. Please check docker container status."
    exit 1
}

Write-Host "SonarQube is UP. Starting analysis..."

# Begin Analysis
dotnet sonarscanner begin /k:"$ProjectKey" /d:sonar.host.url="$SonarUrl" /d:sonar.login="$SonarLogin" /d:sonar.password="$SonarPassword" /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml"

# Build
dotnet build

# End Analysis
dotnet sonarscanner end /d:sonar.login="$SonarLogin" /d:sonar.password="$SonarPassword"
