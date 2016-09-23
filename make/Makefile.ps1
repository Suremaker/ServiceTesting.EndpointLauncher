Define-Step -Name "Update Assembly Info" -Target "DEV,BUILD" -Body {
. (require 'psmake.mod.update-version-info')

	Update-VersionInAssemblyInfo $VERSION
}

Define-Step -Name "Build solution" -Target "DEV,BUILD" -Body {

	call $Context.NuGetExe restore Wonga.ServiceTesting.EndpointLauncher.sln
	call "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" Wonga.ServiceTesting.EndpointLauncher.sln /t:"Clean,Build" /p:Configuration=Release /m /verbosity:m /nologo /p:TreatWarningsAsErrors=true /tv:14.0
}

Define-Step -Name "Test" -Target "DEV,BUILD" -Body {
	. (require 'psmake.mod.testing')

	Define-NUnit3Tests -GroupName 'Tests' -ReportName 'test-results' -TestAssembly '*\bin\Release\*.Tests.dll' `
		| Run-Tests -EraseReportDirectory -ReportDirectory "reports"
}

Define-Step -Name "Upload test results" -Target "BUILD" -Body {
	$wc = New-Object 'System.Net.WebClient'
	$wc.UploadFile("https://ci.appveyor.com/api/testresults/nunit3/$($env:APPVEYOR_JOB_ID)", (Resolve-Path .\reports\test-results.xml))
}

Define-Step -Name "Package" -Target "DEV,BUILD" -Body {
	. (require 'psmake.mod.packaging')

	Find-VSProjectsForPackaging | Package-VSProject
}