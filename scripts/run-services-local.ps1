# ================================
# Script to run microservices
# Structure: ./scr/...
# ================================

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }

# this function check port 
function Test-PortOpen {
    param (
        [string]$HostValue,
        [int]$Port
    )
    try {
        $tcp = New-Object System.Net.Sockets.TcpClient
        $tcp.Connect($HostValue, $Port)
        $tcp.Close()
        return $true
    }
    catch {
        return $false
    }
}

# remove all dotnet process to be sure that there are no started processes
# can be removed and if there are some problems - run stop-service-local.bat
Get-Process dotnet -ErrorAction SilentlyContinue | ForEach-Object { $_.Kill() }

# clear log file
$logFile = "authserver_error.log"
if (Test-Path $logFile) {
    Remove-Item $logFile
}

Write-Host "`nStarting AuthServer with HTTPS profile..." -ForegroundColor Cyan
$authProcess = Start-Process "dotnet" -ArgumentList 'run --launch-profile "https" --project ../scr/AuthServer' -PassThru -RedirectStandardError $logFile -NoNewWindow

if ($authProcess.HasExited) {
    Write-Host "AuthServer failed to start" -ForegroundColor Red
    exit 1
}

Write-Host "`nAuthServer process started. Checking port 7118..." -ForegroundColor Yellow

$maxAttempts = 30
$attempt = 0
$ready = $false

Start-Sleep -Seconds 5

do {
    Write-Host "Attempt $attempt to connect..."
    Start-Sleep -Seconds 2
    $attempt++

    if ($authProcess.HasExited) {
        Write-Host "AuthServer stopped unexpectedly. Exit Code: $($authProcess.ExitCode)" -ForegroundColor Red
        exit 1
    }

    if (Test-PortOpen -HostValue "localhost" -Port 7118) {
        $ready = $true
        break
    }

} while (-not $ready -and $attempt -lt $maxAttempts)

if (-not $ready) {
    Write-Host "AuthServer did not open port 7118 after $maxAttempts attempts." -ForegroundColor Red
    exit 1
}

Write-Host "`n AuthServer is ready. Run other services..." -ForegroundColor Green
# Services list and their ports(real HTTPS ports)
$services = @(
    @{ Name = "ApiGateway";         Port = 7023 },
    @{ Name = "AccountService";     Port = 7127 },
    @{ Name = "TransactionService"; Port = 7212 },
    @{ Name = "HistoryService";     Port = 7213 }
)

foreach ($service in $services) {
    Write-Host "Starting $($service.Name)..."
    $proc = Start-Process "dotnet" -ArgumentList "run --launch-profile `"https`" --project ../scr/$($service.Name)" -PassThru -NoNewWindow

    # waiting port opening
    $maxAttempts = 20
    $attempt = 0
    $ready = $false
    Start-Sleep -Seconds 5

    do {
        Start-Sleep -Seconds 2
        $attempt++

        if ($proc.HasExited) {
            Write-Host "$($service.Name) failed to start. Exit Code: $($proc.ExitCode)" -ForegroundColor Red
            exit 1
        }

        if (Test-PortOpen -HostValue "localhost" -Port $service.Port) {
            $ready = $true
            break
        }

    } while (-not $ready -and $attempt -lt $maxAttempts)

    if ($ready) {
        Write-Host "$($service.Name) is ready on port $($service.Port)." -ForegroundColor Green
    }
    else {
        Write-Host "$($service.Name) did not open port $($service.Port) after $maxAttempts attempts." -ForegroundColor Red
    }
}

Write-Host "`n All services are running and ready!" -ForegroundColor Green