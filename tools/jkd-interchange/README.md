---
last_updated: 2026-09-09
reflects: Jet 4 database and CSV interchange utilities
---

# JKD interchange utility

Standalone Windows utility for moving birth inputs between Horoscope Explorer Pro `.JKD`
files and ikiastrro. It is intentionally outside the solution until a later product feature
integrates the workflow into the application.

Only these inputs are mapped: name, date/time of birth, city/country, latitude/longitude,
and GMT/UTC offset. Horoscope Explorer calculations and settings are not imported. Its sex
field is not imported because `tbl_BirthDetails` does not currently have an equivalent.

The legacy file uses Microsoft Jet and must be opened by a 32-bit process. The script relaunches
itself under 32-bit Windows PowerShell automatically.

```powershell
.\tools\jkd-interchange\jkd-interchange.ps1 inspect "D:\path\people.JKD"
.\tools\jkd-interchange\jkd-interchange.ps1 import "D:\path\people.JKD"
.\tools\jkd-interchange\jkd-interchange.ps1 export "D:\path\ikiastrro-export.JKD"
```

Use `-SqlServer` or `-Database` to target another SQL Server/database. Export refuses to
replace a file unless `-Force` is supplied. Import is transactional and does not generate
charts; chart generation remains an explicit ikiastrro operation.

## JKD and CSV

```powershell
.\tools\jkd-interchange\jkd-csv.ps1 export "D:\path\people.JKD" "D:\path\people.csv"
.\tools\jkd-interchange\jkd-csv.ps1 import "D:\path\people.csv" "D:\path\people.JKD"
```

CSV is UTF-8 with Excel-compatible headers:

```csv
Name,Sex,DateOfBirth,TimeOfBirth,City,Country,Latitude,Longitude,UtcOffset,IanaTimeZoneId
```

Dates use `yyyy-MM-dd`, times use `HH:mm:ss`, and offsets use `+HH:mm` or `-HH:mm`.
`Latitude`, `Longitude`, `UtcOffset`, and `IanaTimeZoneId` may be blank for future app input;
the app will resolve missing values before saving. CSV-to-JKD requires latitude, longitude,
and UTC offset because JKD needs them. IANA time-zone ID stays blank when exported from JKD.

The converter automatically relaunches under 32-bit Windows PowerShell for Jet 4 access.
It refuses to replace an output file unless `-Force` is supplied.
