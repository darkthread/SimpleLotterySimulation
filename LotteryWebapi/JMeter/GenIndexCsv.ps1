﻿(0..99) | % { $i = $_; (0..99) | % { Write-Output "$($i.ToString('0000'))\$($_.ToString('0000'))" } } | Out-File .\index.csv -Encoding utf8