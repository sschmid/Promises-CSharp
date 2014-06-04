#!/bin/sh
xbuild Promises.sln /verbosity:minimal
mono Tests/Libraries/nspec/NSpecRunner.exe Tests/bin/Debug/Tests.dll
