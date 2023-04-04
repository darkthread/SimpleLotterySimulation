'Threads,AvgRespTime,Throughput,ErrCount' | Out-File Summary.csv
Get-ChildItem .\Summary*\statistics.json | ForEach-Object {
	$jo = Get-Content -Raw $_.FullName | ConvertFrom-Json
	$threads = [regex]::Match($_.FullName, 'Summary(?<n>\d+)').Groups['n'].Value
	$meanResTime = $jo.'HTTP Request'.meanResTime
	$throughput = $jo.'HTTP Request'.throughput
	$errCount = $jo.'HTTP Request'.errorCount
	Write-Output "$threads,$meanResTime,$throughput,$errCount"
} | Out-File Summary.csv -Append