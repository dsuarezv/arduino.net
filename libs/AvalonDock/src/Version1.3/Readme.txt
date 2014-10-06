AvalonDock 1.3 - Release Notes

AvalonDock.sln is the main solution of AvalonDock: it's a VS2010 solution and targets .NET 4.0. AvalonDock35.sln 
is a convenient solution used only fo recompiling AvalonDock for .NET 3.5. When switching betwen them much probably
a rebuild all for the solution is required. To completely remove any possible mismatch of referenced framework 
assemblies remove the obj directories of each project (just execute ClearObjs.bat). 

AvalonDock project is the main class library project. It contains all the sources of AvaloDock, plus other resources
like default styles and images.

AvalonDock.DemoApp is an application used primarly for test, it is not exposed into the distribuited setup.

AvalonDock.Themes contains two compiled themes (dev2010.xaml and ExpressionDark.xaml). It will contain more themes
in the future.

Samples directory contains four sample projects that use AvalonDock. These samples show some features of AvalonDock
and are included into the setup. To compile properly samples project be sure to compile a release version of 
AvalonDock project.