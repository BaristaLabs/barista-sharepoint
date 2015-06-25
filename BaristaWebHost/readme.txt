To run this project, IISExpress must be forced to x64 bit mode.

You can configure Visual Studio 2012 to use IIS Express 64-bit by setting the following registry key:

reg add HKEY_CURRENT_USER\Software\Microsoft\VisualStudio\11.0\WebProjects /v Use64BitIISExpress /t REG_DWORD /d 1


Ensure that you stop any existing IISExpress instances before attempting to run the project again.


Also, if you've deployed SPBarista, ensure that you remove Barista.Core.dll from the GAC to enable debugging.