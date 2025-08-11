# ================================
# Script to stop microservices
# Structure: ./scr/...
# ================================

Get-Process dotnet -ErrorAction SilentlyContinue | ForEach-Object { $_.Kill() }
Write-Host "`nProcesses is stopped" -ForegroundColor Green
