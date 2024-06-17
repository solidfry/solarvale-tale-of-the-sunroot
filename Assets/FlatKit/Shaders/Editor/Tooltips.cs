using System.Collections.Generic;

namespace FlatKit.StylizedSurface {
public static class Tooltips {
    // @formatter:off
    public static readonly Dictionary<string, string> Map = new Dictionary<string, string> {
        { "Color", "Primary color of the material. This color is usually the most visible." }, {
            "Cel Shading Mode", "Lets you choose how the coloring/shading is applied.\n" +
                                "- 'None' uses only flat Color parameter above, no primary cel is added;\n" +
                                "- 'Single' adds a primary cel layer;\n" +
                                "- 'Steps' works with a special texture for adding as many cels as there are in the texture;\n" +
                                "- 'Curve' works with a special texture, but unlike 'Steps', it uses a smooth interpolated one."
        },
        { "Color Shaded", "Color of the cel layer. Usually it is the dark part of cel shaded objects." },
        { "Self Shading Size", "How much of the object is covered with the cel layer." }, {
            "Edge Size",
            "How smooth the transition between cel layers is. Values to the left mean 'sharper', values to the right are 'smoother'."
        }, {
            "Localized Shading",
            "Can also be called the \"flatness\" value. Defines how spread out the shading is across " +
            "the object. The value of 0 distributes the color transition gradually across the whole " +
            "object. The value of 1 makes shading fully flat."
        },
        { "Enable Extra Cel Layer", "If one cel is not enough, here's another one." }, {
            "Enable Specular",
            "Toggle specular highlight on/off. Specular is a highlight that appears on shiny surfaces, " +
            "like metal or glass. It is kind of a glare effect."
        },
        { "Specular Color", "Color of specular highlight. Usually it is white or very bright." }, {
            "Specular Size",
            "How big the specular highlight is. Values to the left mean 'smaller', values to the right are 'bigger'."
        }, {
            "Specular Edge Smoothness",
            "How smooth or sharp the specular highlight is. Values to the left mean 'sharper', values to the right are 'smoother'."
        }, {
            "Enable Rim",
            "Enables a rim highlight on the edges of the object. It can be used as a pseudo-outline layer or even as an additional color layer."
        },
        { "Rim Color", "Color of the rim highlight. Usually it is bright." }, {
            "Light Align",
            "Position of the rim highlight that depends on light direction. Use this to move the rim part on the mesh."
        },
        { "Rim Size", "How big the rim highlight is." }, {
            "Rim Edge Smoothness",
            "How smooth or sharp the rim part is. Values to the left mean 'sharper', values to the right are 'smoother'."
        },

        // Height gradient.
        {
            "Enable Height Gradient",
            "Height gradient enables a color overlay that gradually changes across the vertical axis."
        },
        { "Gradient Color", "Sets the color of the gradient overlay part." }, {
            "[DR_GRADIENT_ON]Space", "Whether the gradient should be in World or Local space.\n" +
                                     "- World space means that the `Center` parameter is relative to the world origin.\n" +
                                     "- Local space means that the `Center` parameter is relative to the object's pivot point."
        },
        { "Center X", "X coordinate of the middle gradient point." },
        { "Center Y", "Y coordinate of the middle gradient point." },
        { "Size", "How stretched the gradient is." },
        { "Gradient Angle", "Rotates the gradient on the mesh." },

        // Outline.
        { "Enable Outline", "Enables an outline layer on the material." },
        { "[DR_OUTLINE_ON]Width", "Thickness of the outline." },
        { "[DR_OUTLINE_ON]Color", "Color of the outline." }, {
            "[DR_OUTLINE_ON]Scale", "Controls a way of stretching the model uniformly. This is used rarely, " +
                                    "typically on symmetrical objects when the model can't have smooth normals."
        }, {
            "[DR_OUTLINE_ON]Smooth Normals", "Processes the mesh and creates a copy of it with smoothed normals " +
                                             "baked into a UV channel.\nPlease see our online documentation " +
                                             "(https://flatkit.dustyroom.com) for more details."
        }, {
            "[DR_OUTLINE_ON]Depth Offset", "Pushes the outline further from the camera. Use this if you are seeing " +
                                           "outlines intersecting the object."
        }, {
            "[DR_OUTLINE_ON]Space", "Whether the outline should be in Clip (aka Screen) or Local space. Different " +
                                    "objects require different space, so try both."
        }, {
            "Camera Distance Impact", "How much the distance between the object and the camera influences the " +
                                      "outline width. This is useful for making outlines thinner on distant objects."
        }, {
            "Enable Vertex Colors", "Uses mesh vertex color in object's appearance.\n" +
                                    "Vertex colors are colors that are stored in the mesh itself. They can be painted in 3D " +
                                    "modeling software like Blender or Maya. Vertex colors are usually used for additional " +
                                    "color layers."
        }, {
            "Light Color Contribution",
            "How much the color of real-time lights (directional, point or spot) influences the color of the object.\n" +
            "Has no effect when the lighting is baked as that color is already included in the lightmap and/or light probes."
        }, {
            "Point / Spot Light Edge",
            "Sharpness of the transition between lit and unlit parts of the surface when using point or spot lights."
        }, {
            "Override Light Direction",
            "Enables overriding of light direction. Use it when you want to re-align the shaded part for the current material only."
        },
        { "Pitch", "Moves the shaded part across world-space X coordinate" },
        { "Yaw", "Moves the shaded part across world-space Y coordinate" },

        // Shadows.
        {
            "Mode", "Use this menu to let the current material receive shadows.\n" +
                    "- 'Multiply' parameter multiplies black shadow over existing colors of shading;\n" +
                    "- 'Color' parameter applies freely colored shadow over existing colors of shading."
        },
        { "Power", "How opaque the received shadows are." },
        { "[_UNITYSHADOWMODE_COLOR]Color", "Color of the received shadows." }, {
            "Sharpness",
            "How smooth or sharp the received shadows are. Values to the left mean 'sharper', values to the right are 'smoother'."
        },
        {
            "Shadow Occlusion",
            "Mask received Unity shadows in areas where normals face away from the light. Useful to " +
            "remove shadows that 'go through' objects."
        },

        // Texture maps.
        { "Albedo", "Main texture of the material. It is also known as 'Diffuse'." }, 
        {
            "Mix Into Shading",
            "Uses the main texture when calculating lighting and shading colors. When disabled, the Environment Lighting of the scene has a greater impact on the material."
        },
        { "Texture Impact", "How opaque or transparent the texture is." }, 
        {
            "Blending Mode", "Select which blending mode to use for the texture:\n" +
                             "-'Add' adds the texture to the existing colors of shading;\n" +
                             "-'Multiply' multiplies the texture over the existing colors of shading;\n" +
                             "-'Interpolate' (only Detail Map) blends the texture with the existing colors of shading."
        },
        {
            "Detail Map",
            "A texture that is used to add small details to the surface. There is no principal difference " +
            "between this texture and the main one, except that it is usually smaller and has more details."
        },
        { "Detail Color", "The color to be applied to the detail map." },
        { "Detail Impact", "How visible the detail map is." },

        { "Normal Map", "Also known as 'Bump Map'. This texture contains normals applied to the surface." }, {
            "Emission Map", "A texture representing the parts of the object that emit light. It is usually used for " +
                            "glowing signs, screens, lights, etc."
        }, {
            "Emission Color",
            "The color of the emitted light. Note that this is an HDR value. When combined with Bloom " +
            "post-processing, it can be used to create a glowing effect."
        },
        { "Base Alpha Cutoff", "Allows controlling which pixels to render based on the texture alpha values." }, {
            "Surface Type",
            "Whether the object is opaque or transparent. This defines the render queue of the material."
        },
        { "Render Faces", "Whether to render the Front, Back or both faces of the mesh." },
        { "Alpha Clipping", "Allows controlling which pixels to render based on the texture alpha values." },
        { "Enable GPU Instancing", "GPU Instancing allows rendering many copies of the same mesh at once." },
    };
    // @formatter:on
}
}