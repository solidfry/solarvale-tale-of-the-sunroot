URP Setup:

1.Import the URP assets:
Open the “URP assets.unitypackage” file located
in the main folder.

2.Configure Renderer:
Open the inspector of your Universal Renderer Data file and click “Add Renderer
Feature”. Select “Decal” and add the component.
Then under the component settings, set “Technique” to “Screen Space”.

3.Hide Stamp Icons:
Disable the built in decal projector's stamp gizmo icon.

4.Add a Decal Spline to your scene:
Right click the hierarchy window and under “Uhm…Uhm..Games” select “Decal Spline”.
A Decal spline object will be added to the scene.

5. (Optional)By default decal splines draw on every object, to exclude certain
objects from receiving decals , a “Rendering layer” can be used. For this
feature enable ”Use Rendering Layers” A custom “Rendering layer” can be
added by opening the “Project Settings…” under the edit menu. Find “URP
Global Settings”. Add a new rendering layer and name it “decal splines”. This
layer can now be used in the Decal Spline’s layer mask.




HDRP Setup:

1. Import the HDRP assets:
Open the “HDRP assets.unitypackage” file
located in the main folder.

2. Configure Renderer:
Open the inspector of your HD Render Pipeline Asset file. Here make sure Decals are Enabled.

3.Hide Stamp Icons:
Disable the built in decal projector's stamp gizmo icon.

4.Add a Decal Spline to your scene:
Right click the hierarchy window and under “Uhm…Uhm..Games” select “Decal Spline”.
A Decal spline object will be added to the scene.

5. (Optional)By default decal splines draw on every object, to exclude certain
objects from receiving decals , a “Decal layer” can be used. For this feature to
function, enable Layers.




Controls:

Escape - exit edit mode.
Left shift - flip auto snap while holding.
Left control - Remove anchor point / unlock curve handle.
Space - Hide anchor points.



More details can be read in the included user manual.