ModuleInitializer Proof of concept

Step 1) Build the Injector solution
Step 2) Build the Coen.Utilities solution
Step 3) Run the TestApplication in the Coen.Utilities solution
		Note the FileNotFoundException stating that the SharpZipLib could not be found
Step 4) Run the Injector.exe on Coen.Utilities using the following parameters:
		Injector.exe ModuleInitializer Initialize path/to/Coen.Utilities.dll
Step 5) Run the Testapplication in the Coen.Utilities again (don't rebuild, it will overwrite the Coen.Utilities)
		Note the tracelog stating the moduleInitializer was loaded and the zip file was created.