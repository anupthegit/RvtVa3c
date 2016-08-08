# Etude

A Revit custom exporter add-in generating JSON output for the Allegro.


## Setup, Compilation and Installation

Etude is a standard Revit add-in application.

It is installed in the standard manner, i.e., by copying two files to the standard Revit Add-Ins folder:

- The .NET assembly DLL Etude.dll
- The add-in manifest Etude.addin

In order to generate the DLL, you download and compile the Visual Studio solution:

- Download or clone the [Etude GitHub repository](https://github.com/anupthegit/Etude).
- Open the solution file Etude.sln in Visual Studio 2012 or later.
- Build the solution locally:
    - Add references to the Revit API assembly files RevitAPI.dll and RevitAPIUI.dll, located in your Revit installation directory, e.g.,

            C:\Program Files\Autodesk\Revit Architecture 2015

    - If you wish to debug, set up the path to the Revit executable in the Debug tab, Start External Program; change the path to your system installation, e.g.,

            C:\Program Files\Autodesk\Revit Architecture 2015\Revit.exe

    - Build the solution.

This will open the Revit installation you referred to, and install the plugin, which can then be launched from the Revit Add-Ins tab.

And wonderfully exports the your Revit model to a JSON file.

For more details, please refer to the [Revit API Getting Started](http://thebuildingcoder.typepad.com/blog/about-the-author.html#2) material, especially the DevTV and My First Revit Plugin tutorials.
