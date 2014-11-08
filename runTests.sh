#!/bin/sh
xbuild Tests/Tests.sln /verbosity:minimal
if [ $? = 0 ]
then
	mono Tests/Libraries/nspec/NSpecRunner.exe Tests/bin/Debug/Tests.dll
else
	echo "ERROR: Could not compile!"
	exit 1
fi
