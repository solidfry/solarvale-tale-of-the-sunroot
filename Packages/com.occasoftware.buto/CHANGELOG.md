# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

[Changelog also available online](https://www.occasoftware.com/changelogs/buto)

## [7.11.3] - 2024-05-27

### Fixed

- Fixed issue with value remapping

## [7.11.2] - 2024-02-12

### Fixed

- Fixed issue when using Light area size property in builds (it is Editor Only!)

## [7.11.1] - 2024-02-09

### Fixed

- Fixed spot light issue.
- Fixed editor gizmos for spot and area lights.

## [7.11.0] - 2024-02-08

### Added

- Added support for area lights.

## [7.10.0] - 2024-01-09

### Added

- Added component for main light color override.

## [7.9.2] - 2023-12-06

### Fixed

- Improved protections for div by zero and NaN cases in Additional Light calculataions.

## [7.9.1] - 2023-11-27

### Fixed

- Expanded some guards against div by zero, which may help to prevent NaN scenarios on client devices.

## [7.9.0] - 2023-11-20

### Added

- Added a new property that makes it easy to control god rays, `Density in Light`.

### Fixed

- Buto continues rendering when Density is set to 0. This makes it easier to smoothly reduce fog density to 0.

## [7.8.0] - 2023-11-07

### Added

- Added a new property that makes it easy to control god rays, `Density in Shadow`.

## [7.7.3] - 2023-11-07

### Added

- Added a #define in Buto.hlsl to clarify API version #: `#define BUTO_API_VERSION_2`. Current version number is 2.

## [7.7.2] - 2023-11-03

### Fixed

- Removed the pragma that disabled a pow(f,e) warning. The restore pragma was causing build errors on PS5.

## [7.7.1] - 2023-10-12

### Fixed

- Fixed issue with RenderPipeline tag resulting in broken volume textures in 2022.3.x+ (something after .0f1).

## [7.7.0] - 2023-10-09

### Added

- Added an editor option for the light falloff over distance toggle and setting.

## [7.6.0] - 2023-10-06

### Added

- Added an experimental light falloff over distance option
- Added an experimental light falloff by range option

### Changed

- Updated icons for components.
- Improved Volume Override UI.

## [7.5.0] - 2023-10-06

### Changed

- Changed the default options for each quality setting to be more consistent with their names.
- Changed the default quality option from Low to High. Presumably you all want fog to look quite nice unless you decide otherwise?

## [7.4.0] - 2023-10-05

### Added

- Added an experimental light falloff mode for spot lights.

## [7.3.1] - 2023-10-05

### Fixed

- Fixed a missing pragma: `#pragma multi_compile _ _LIGHT_COOKIES`

## [7.3.0] - 2023-10-03

### Added

- Added a hidden experimental sampling option for high-quality local sampling with large view distances.

## [7.2.4] - 2023-10-02

### Changed

- Cleaned up logic around refreshing history textures.

## [7.2.3] - 2023-09-29

### Fixed

- Fixed various issues related to multiple cameras

## [7.2.2] - 2023-09-27

### Fixed

- Fixed backward compatibility with Cozy Integration

## [7.2.1] - 2023-09-19

### Fixed

- Buto will no longer throw errors when Light Cookies are disabled

## [7.2.0] - 2023-08-29

### Added

- Added individual sliders for Lighting + Media Temporal Integration. To disable TAA and jitter, set to 1. To disable TAA but keep jitter, set to 0.
- Added wind zone integration. To use it, add a Buto Wind Zone component to a Wind Zone object in your scene. Buto will inherit the wind direction and main wind speed.

### Changed

- Changed the wind offset to a static property on the CPU.

### Removed

- Removed the Temporal Integration toggle.
- Removed the slider that affected both Lighting + Media Temporal Integration rates at the same time.
- Removed the de-banding noise from the isolated blit pass.

## [7.1.0] - 2023-08-28

### Added

- Added a de-banding noise to the isolated blit pass.

## [7.0.2] - 2023-08-10

### Changed

- Cleaned up and simplified start menu.

## [7.0.1] - 2023-07-24

### Added

- Added tooltips for all editor options

### Changed

- Optimized Renderer Feature performance (0.1ms to 0.05ms).
- Renderer Feature no longer causes GC Allocs
- Renamed Number of Forward Cells to Sample Count
- Removed some conditional logic in the volume editor

## [7.0.0] - 2023-07-18

This version is compatible with Unity 2022.3.0f1

### Fixed

- Fixed a bug causing a black screen on Mac (Metal). (Compute textures must be declared as float4, not half4, even when using ARGBHalf RenderTextureFormat).

### Changed

- Buto now renders fog with more accuracy and less noise
- Sample Count is now the Grid Size Z property
- You can now control the fog render resolution

### Added

- Full support for transparent materials

### Removed

- Removed Distant Fog
- Removed Depth Softening Distance
- Removed Depth Interaction Mode
- Removed Ray Length Mode
- Removed Horizon Shadowing
- Removed Shadow Intensity

## [6.1.5] - 2023-07-06

This version is compatible with Unity 2022.3.0f1

### Changed

- Rearranged project folder hierarchy
- Cleaned up Add Component Menu.
- Replaced all half precision usages with float precisions.
- Improved ambient light sampling.

## [6.1.4] - 2023-06-27

This version is compatible with Unity 2022.3.0f1

### Fixed

- Buto Transparent Fog sampler now correctly handles situations where the _ButoTexture is uninitialized.

## [6.1.3] - 2023-06-15

This version is compatible with Unity 2022.3.0f1

### Fixed

- Fixed a rendering issue for Additional Lights on Mac OS
- Various bugfixes

## [6.1.2] - 2023-06-08

This version is compatible with Unity 2022.3.0f1

### Fixed

- Fixed a compilation issue for non-WebGL Platforms :)

## [6.1.1] - 2023-06-08

This version is compatible with Unity 2022.3.0f1

### Changed

- Various performance improvements, no visual changes

### Fixed

- Fixed a compilation issue for WebGL

## [6.1.0] - 2023-06-05

This version is compatible with Unity 2022.3.0f1

### Added

- Added a Start Menu when you first launch Buto. The Start Menu provides links to helpful resources and indicates where you can go to get help.

## [6.0.0] - 2023-06-05

This version is compatible with Unity 2022.3.0f1

### Changed

- Migrated to the new RTHandle and Blitter APIs

## [5.0.0] - 2023-06-05

This version is compatible with Unity 2021.3.0f1

### Changed

- Migrated to UPM format
- Improved performance with custom shadow sampling

### Added

- Added more quality setting options to provide better guidance on recommended settings

## [4.3.0] - 2023-05-23

### Added

- Added an option to prefer nearby samples. This improves the quality of nearby fog at the cost of more distant fog. This is the new Ray Length Mode option. The default setting is "Constant", but "Prefer Nearby" can give better results in some cases.
- Added an option to set the start distance for Distant Fog.

### Changed

- Buto now upsamples the buto render before temporal anti-aliasing.

## [4.2.0] - 2023-05-11

### Added

- Added option to control Distant Fog Base Height
- Added option to control Distant Fog Attenuation Size

### Changed

- Changed various tooltips for brevity
- OccaSoftware.Buto renamed to OccaSoftware.Buto.Runtime
- OccaSoftware.Buto.Demo now has an associated asmdef.
- Updated basic demo scene to showcase color overrides and directional lighting.
- The Profiles folder has been moved from AssetResources/~ to DemoResources/~ to propertly indicate that it is not required for the asset to function.
- Buto Fog Density mask context item moved from GameObject/Effects panel to GameObject/Rendering panel.
- Grouped all Buto-related scripts to the Buto/~ folder in the Add Component menu.

### Fixed

- Fixed a source of 40B GC Alloc per-frame.

### Removed

- Buto Fog Volume context menu item has been removed, as it presents a potentially confusing choice between Buto, Global, Box, and Sphere volumes where no choice needs to be made. Buto can simply be added to any type of volume using the Add Override menu.
- Prefab items have been removed, as they present a potentially confusing alternative to the context menu approach for adding Buto Lights and Fog Density Masks. If you get a prefab missing warning, simply unpack the prefab from source and create a new base prefab and variants.

## [4.1.0] - 2023-05-09

### Added

- Added Settings for the renderer feature. You can now configure the target RenderPassEvent directly from the Pipeline Asset inspector.
- Added a Depth Interaction Mode option. Early Exit mode clips rays early when there is nearby geometry. Maximize Samples mode shortens each step so that every ray takes the same number of samples.
- Added a Depth Softening Distance option. This option enables you to soften the fog when it is close to scene geometry, which gives a more natural and realistic look.
- Added more demo scenes.

### Changed

- Changed the array declarations in the renderer feature so that they are initialized only once and the same arrays are populated each frame.
- The Fog Density Mask component now conditionally compiles editor-related code using the #if UNITY_EDITOR directive.
- The Buto Light component now conditionally compiles editor-related code using the #if UNITY_EDITOR directive.

## [4.0.0] - 2023-05-05

### Changed

- Changed all Shader Graphs to Custom Shaders.
- Changed Distant Fog to start immediately from the Ray Origin. This gives more visually coherent results.
- Changed various functions to improve performance.

### Added

- Added a Distant Fog Density property.
- Added support for Light Cookies for Directional Light.
- Added more demo scenes.

### Fixed

- Fixed shadows when in Orthographic projection.
- Fixed an issue causing Distant Fog to throw an error.

## [3.6.7] - May 5, 2023

- Fixed an issue causing the shader to fail when Distant Fog is enabled.

## [3.6.6] - May 3, 2023

- Added support for orthographic projection.

## [‍3.6.5] - 2023-05-03

- Removed dependency on *"Packages/com.unity.render-pipelines.universal/Shaders/Utils/Fullscreen.hlsl"*, which resolves an error in Unity 2022+.
- Added a step to disable the SSAO Keyword during the Buto render pass, which resolves an issue causing screen artifacts in Unity 2022+.

## [3.6.4] - 2023-05-01

- Changed downsample and upsample systems to improve rendering quality
- Changed downsample and upsample systems to use custom shaders rather than shader graphs.

## [3.6.3] - 2023-03-22

- Fixed an issue causing Buto to fail to render in Unity 2022.2.11.

## [3.6.2] - 2023-02-21

- We've fixed an issue that caused incorrect frame data when using Temporal Anti-Aliasing. To make sure the fix works properly, make sure to disable the Native RenderPass option in your project. Don't worry, we've got you covered and our support team is here to help if you have any questions.
- We've also improved the quality of our fog to make it even more realistic and immersive. You'll notice the difference right away!
- Performance has also been enhanced, so you can use our volumetric fog asset without worrying about any slowdowns or issues.
- And finally, we've added a handy feature that automatically enables Animated Sample Positions when using TAA. If you haven't overridden Animated Sample Positions, Buto will take care of it for you, so you can focus on creating amazing content.

## [3.6.1] - 2023-02-10

- Fixed an issue where enabling Screen Space Shadows would result in inaccurate shadowing.

## [3.6.0] - 2023-02-06

- You can now add box density masks.
- Density masks now appear as toggleable gizmos in the scene view, even when not selected.
- The density mask blend falloff is now a density mask blend distance.
- The density mask blend distance now represents the distance (in units) over which the mask effect will fade out.

## [3.5.0] - January 18, 2023

- You can now disable Buto for specific cameras in the scene. This can be useful for Overlay, UI, or Effects-related cameras. To disable Buto for a specific camera, simply add the [.c]DisableButoRendering[.c] component to that camera.
- Fixed an issue with the new [.c]_ButoTexture[.c] that caused the Buto render pass to have incorrect alpha values.

## [3.4.1] - 2023-01-17

- Buto now always clears its temporary render targets before rendering.

## [3.4.0] - 2023-01-11

- Buto now writes to a fullscreen texture before writing to the screen. This enables custom texture sampling from the Buto Fog texture in custom shaders.
- You can sample the Buto texture using the new Buto.hlsl include file, which has both Shadergraph-ready and Custom Shader-ready methods.
- Two new Shaders (Transparent-ReceiveFullFog-xxx.Shadergraph) demonstrate usages in Shadergraph. Objects using the materials are present in the full scene.
- Buto now automatically sets two scripting defines during the first compilation after the asset is imported to the project: [.c]OCCASOFTWARE[.c] and [.c]BUTO[.c].

## [3.3.2] - 2022-12-07

- Included missing demo scripts

## [3.3.1] - 2022-12-06

- Various performance optimizations with up to ~25% faster rendering.
- The editor now offers quality presets.

## [3.3.0] - 2022-11-29

- Buto now supports spotlights.
- We've introduced a new directional lighting setting. With directional lighting, you can now tint the fog based on the position of your main directional light in the scene.
- We've improved the editor GUI. The connection between the Color Influence and the Individual and Ramp color overrides is now more obvious.

## [3.2.2] - 2022-09-28

### Bug Fixes

- Spherical Harmonics now correctly uses the ambient light color of the scene when setting the fog world color.
- Setting your directional light intensity to 0 will no longer cause the scene to turn black.
- Buto will now use the same number of steps for every ray, regardless of the depth to opaque geometry. As a result, nearby opaque objects will now have higher quality results. The performance impact will also be more consistent frame-to-frame.

### Updated Demo Scene

- The demo scene now has both a large and small environment to test. These environments should help you to understand better how the fog settings impact your scene.

## [3.2.1] - 2022-09-11

### Bug Fixes

- Changed the Scene Depth UV mode from Default to Raw. This fixes a sampling issue that causes aliasing when using an incorrectly upscaled version of the low-resolution depth texture in the final merge pass.

## [3.2.0] - 2022-08-29

### New Features

- You can now control Fog Density Mask blend modes for each Fog Density Mask component in the scene, giving you much greater control over how and where fog will render.
- Buto now includes a built-in Volumetric Noise generation system within the editor. This system is seamlessly integrated with Buto's inspector editor, so you can easily configure the look of your fog.
- You can now configure Lit, Shadowed, and Emission fog colors which are combined additively with the pre-existing Custom Color Ramp system. This gives you greater control and easier configuration for minor fog color changes. Quality of Life Changes
- The Volumetric Fog editor window has been simplified and grouped into more logical sections. When in Scene View, disabling the Fog Effects in the Scene View Options will also disable Buto.

## [3.1.2] - 2022-06-27

### Bug Fixes

- Decals were incorrectly rendering twice when the Decal Render Pass Technique was set to DBuffer. Decals will now render correctly for both DBuffer and Screen Space Decal Techniques.

## [3.1.1] - 2022-06-17

### Bug Fixes

- Incorrect UV coordinates were being passed to the Volumetric Fog shader, which resulted in incorrectly sampled fog. The correct UV coordinates will now be passed to avoid this issue.

## [3.1.0] - 2022-06-13

### Feature Update

- Volumetric Fog density will linearly go to 0 as a ray approaches the Max Volumetric Distance when Analytic Fog is disabled.Bug Fixes
- Fixed a visual bug occuring around object borders that resulted from jittering the depth UVs.
- Fixed a code error causing the Buto Lights to be ignored during the light integration step.Demo
- Changed the color of scene objects to red so that it is easier to see them among the fog, which is typically a mid-grey color.

## [3.0.0] - June 10, 2022

- Integrated with the Volume Component system
- Added support for Temporal Anti Aliasing and corresponding controls to the Volume Component.
- Added Fog Density Masks and corresponding controls to the Volume Component.
- You can add a Fog Density mask from the Add Game Object menu from Game Object -> Effects -> Buto Fog Density Mask
- Added Spherical Harmonics support
- Introduced additional controls for performance, such as Maximum Self-Shadowing Octaves
- Adjusted Analytic Fog equations
- Adjusted some editor minimum / maximums.

## [2.3.0]

### New Assets

- Added 24 more volumetric noise textures Improvements
- Added Lacunarity and Gain parameters. These parameters give you better control over the final look of the volumetric noise.
- Adjusted the fog sampling algorithm so that we sample through the fog volume more evenly, giving less noisy results for most use cases.Bug Fixes
- Fixed an issue causing some material parameters to be reset when exiting and re-launching Unity.
- Fixed an issue causing the fog to sample the Color Ramp map as if the fog were further away than it actually was. Color Ramp functionality will operate more consistently now.

## [2.2.0]

### New Assets

- Added 5 more volumetric noise textures (2 higher resolution perlin noise textures, 3 worley noise textures.Improvements
- Added a Horizon Shadowing option, which realistically causes fog to become shadowed when the main light falls below the planet's horizon line. Increases performance cost.
- Removed the maximum limit on Volumetric Fog Distance.
- Added an option to completely disable Analytic Fog, renamed Non Volumetric Fog to Analytic Fog, and removed the maximum limit on Analytic Fog Distance.
- Added an option to enable Self-Shadowing Volumetrics. Self-Shadowing causes light to be attenuated as it moves through a volume of non-zero density, resulting in shadowing on the far side of the volume. Computationally expensive, especially when paired with Octaves.
- Added an option (Octaves) to increase the quality of the volumetric noise by resampling the noise texture over progressively smaller domains. Computationally expensive.
- Adjusted the Noise Intensity Min/Max slider to a Noise Intensity Mapping Slider. Previously, this slider basically only allowed you to increase the range of intensity values in the scene. Now, this slider allows you to clip values outside of the mapping to create more sharply defined noise.
- Adjusted the calculation method applied to fog density when supplied to the Analytic Fog. Analytic Fog should now be more coherent with the Volumetric Fog under a wider variety of circumstances.Bug Fixes
- Adjusted the calculation method used to to construct the final noise values out of the input noise textures. Final noise values will now correctly occupy a [0, 1] range, which improves quality of final rendering.
- Adjusted the Wind Velocity calculations so that it operates consistently regardless of the Noise Tiling Domain setting. In other words, changing the Noise Tiling Domain will no longer also indirectly affect the apparent wind velocity.

## [2.1.1]

### Bug Fixes

- Fixed an issue that could cause Buto to fail to re-create itself between scene loads.

## [2.1.0]

### Improvements

- Added tooltips to all editor controls and renamed some parameters for clarity.
- Changed Height Falloff parameter [0, 2] to Attenuation Boundary Size [0, inf). Height Falloff parameter previously represented an abstract value attenuating the fog density over height. Attenuation Boundary - Size now represents a concrete value at which the fog will have been attenuated to 36% of its original value.
- Created a separate assembly definition for the OccaSoftware.Buto.Demo namespace region. Reminder that you can safely remove the entire DemoResources folder once familiar with the asset.
- Updated Readme to reflect the GameObject/Volume/Buto Fog Volume method of creating fog volumes in scene.

### Bug Fixes

- Fixed a rendering issue that could occur when using a single shadow cascade.

## [2.0.1]

- Fixed an issue causing Reflection Probes to render black when the Buto Fog Volume was in scene.

## [2.0.0]

- Upgraded to Unity 2021 LTS. Future updates will target Unity 2021 LTS until the next LTS release. Unity 2020 LTS is locked to version 1.3.2.
- Improved Editors
- You can now create a Buto Light from the GameObject/Light context menu.
- You can now create a Buto Fog Volume from the GameObject/Volume context menu.

## [1.3.2]

- Fixed an issue causing Volumetric Point Light intensity to approach infinitely high values in some cases.

## [1.3.1]

- Fixed an issue that could occur on import due to Package Manager Dependencies

## [1.3.0]

- Added Volumetric Point Light support
- Any Game Object can be designated as a Volumetric Point Light by adding the Buto Light component
- The Buto Light component can inherit lighting parameters of a Light component on the same Game Object if one is present

## [1.2.2]

- Fixed an issue that could occur on some graphics platforms

## [1.2.1]

- Fixed an issue when transitioning to a new scene
- Added custom editor GUI

## [1.2.0]

- Introduced Sample Count slider.
- Introduced option to disable animation of raymarch offset over time
- Introduced public API, GetFogMaterial and SetFogMaterial
- Added a script demonstrating an example of using the SetFogMaterial method.
- Added two more default Fog Materials
- Removed all const constructors to ensure coherence with GLSL

## [1.1.0]

- Introduced Volumetric Noise Intensity sliders.
- Removed Volumetric Noise falloff over distance
- Fixed an issue that could occur when increasing the Base Height above 0.
- Corrected an issue with Fog Emission Color sampling.

## [1.0.1]

- Fixed an issue that could occur if the Fog Render Feature is added to a Renderer without a Buto Fog Volume in the scene.

## [1.0.0] - 2022-02-15

- First release
