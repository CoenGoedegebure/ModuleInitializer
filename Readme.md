## ModuleInitializer Proof of concept

This repository contains the source code supporting the article on https://www.coengoedegebure.com/module-initializers-in-dotnet

To get it to run, follow these steps:

* 1) Build the Injector solution.
* 2) Build the Coen.Utilities solution.
* 3) Run the TestApplication in the Coen.Utilities solution. Note the FileNotFoundException stating that the SharpZipLib could not be found.
* 4) Now run the Injector.exe on the Coen.Utilities assembly using the following parameters:
		`Injector.exe ModuleInitializer Initialize path/to/Coen.Utilities.dll`
* 5) Run the Testapplication in the Coen.Utilities again (don't rebuild, it will overwrite the Coen.Utilities). Note the tracelog stating the moduleInitializer was loaded and the zip file was created.