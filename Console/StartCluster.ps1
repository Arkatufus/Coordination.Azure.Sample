$command = "$PSScriptRoot\TestNode\StartNode.ps1"

Write-Host "Installing and updating Petabridge.Cmd"
dotnet tool install -g pbm
dotnet tool update -g pbm

Write-Host "Launching first node in a new powershell window"
Invoke-Expression "cmd /c start powershell -Command $command -p 15225 -j 15225 -c 9110"

Write-Host "Launching second node in a new powershell window"
Invoke-Expression "cmd /c start powershell -Command $command -p 15226 -j 15225 -c 9111"

Write-Host "Launching third node in a new powershell window"
Invoke-Expression "cmd /c start powershell -Command $command -p 15227 -j 15225 -c 9112"

Write-Host "Waiting a bit for second node to start..."
Start-Sleep -Seconds 10
Write-Host "Launching Petabridge.Cmd in a new powershell window"
Invoke-Expression "cmd /c start powershell -Command pbm localhost:9111"

Write-Host ''
Write-Host 'To forcefully kill the second node, type in the Petabridge.Cmd shell:'
Write-Host 'inject-failure kill-process'
Write-Host ''
Write-Host 'To simulate a dead network connection between first and second node, type in the Petabridge.Cmd shell:'
Write-Host 'inject-failure throttle -t akka.trttl.gremlin.tcp://ClusterSystem@localhost:15227 -r 0'