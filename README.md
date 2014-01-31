LIFX-Control
============

This is a test project to control LIFX V1 lightbulbs with a C# library.  

Not associated with LIFX, using packet information documented by https://github.com/magicmonkey/lifxjs 

(Huge thanks to MagicMonkey and the others for digging out the protocol, without them none of this would be possible.)

Currently there are three sample projects included - 
LifxController by codemonkey76 for command line control
LIFXControlTest - a WPF based test driver program for the LIFXControl object
LightSpeak - a test program that provides voice control for the bulbs using a simple grammar and the Microsoft.Speec v11 SDK

You will need the Microsoft Speech Platform runtime, version 11 to compile the LightSpeak sample which can be found at:
http://www.microsoft.com/en-us/download/details.aspx?id=27225

This library is a work in progress - intended features are:

Simple control of bulbs in groups or individually.
Management of exposed bulb properties, including groups and labels.
Scheduled and timed tasks and sequences.
Storage and management of sequences.
Integration with varied sensors.
Support for Win32 apps, WinRT (Windows 8.1 and Windows Phone 8), and .Net Micro Frameworks.
Example programs for each.

If you find issues, please enter them here.  Also, please include how many bulbs are active when you run into your issue.

