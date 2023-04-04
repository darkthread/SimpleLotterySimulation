#Remove-Item ..\Report1 -Confirm:$false -Recurse
#Remove-Item ..\Reports\n1.jtl -Confirm:$false
(1,2,3,4,5,6,7,8,9,10,15,20,25,30,40,60,80,100) | ForEach-Object {
	& jmeter "-Jthreads=$_" -JticketsLocation=C:\LoadTest\Data\Tickets\ -n -t Simulation-LoadTest.jmx -l "..\Reports\n$_.jtl" -e -o "..\Reports\Summary$_"
}