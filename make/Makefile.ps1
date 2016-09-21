Define-Step -Name "Update Assembly Info" -Target "DEV,BUILD" -Body {
. (require 'psmake.mod.update-version-info')

	Update-VersionInAssemblyInfo $VERSION
}

Define-Step -Name "Build solution" -Target "DEV,BUILD" -Body {

	call $Context.NuGetExe restore Wonga.ServiceTesting.EndpointLauncher.sln
	call "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" Wonga.ServiceTesting.EndpointLauncher.sln /t:"Clean,Build" /p:Configuration=Release /m /verbosity:m /nologo /p:TreatWarningsAsErrors=true /tv:14.0
}

Define-Step -Name "Tests" -Target "DEV,BUILD" -Body {
	. (require 'psmake.mod.testing')

	$NunitPackageVersion = '3.2.1'
	$path = Fetch-Package 'NUnit.ConsoleRunner' $NunitPackageVersion
	copy "appveyor_addins\*" "$path\tools" -force

	Define-NUnit3Tests -GroupName 'Tests' -ReportName 'test-results' -TestAssembly '*\bin\Release\*.Tests.dll' -NUnitVersion $NunitPackageVersion -ReportFormat 'AppVeyor' `
		| Run-Tests -EraseReportDirectory -ReportDirectory "reports"
}

Define-Step -Name "Package" -Target "DEV,BUILD" -Body {
	. (require 'psmake.mod.packaging')

	Find-VSProjectsForPackaging | Package-VSProject
}