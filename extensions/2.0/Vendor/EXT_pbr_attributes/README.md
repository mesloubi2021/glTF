# EXT_pbr_attributes

## Contributors and Acknowledgements

* Florian Hoenig, Unbound Technologies, [@rianflo](http://twitter.com/rianflo)
* Michael Saenger, Unbound Technologies, [@abductee_org](http://twitter.com/abductee_org)
* Alex Evans, Mediamolecule, [@mmalex](https://twitter.com/mmalex)
* Gary Hsu, Microsoft, [@bghgary](https://twitter.com/bghgary)
* Warren Moore, metalbyexample.com, [@warrenm](https://twitter.com/warrenm)
* David Farrell, Oculus, [@Nosferalatu](https://twitter.com/Nosferalatu)
* Don McCurdy, Google, [@donrmccurdy](https://twitter.com/donrmccurdy)
* Ocean Quigley, Facebook, [@oceanquigley](https://twitter.com/oceanquigley)
* Scott Nagy, Microsoft, [@visageofscott](https://twitter.com/visageofscott)
* Milan Bulat, Foundry, [@speedy_milan](https://twitter.com/speedy_milan)

## Status

Draft

## Dependencies

Written against the glTF 2.0 spec.

## Overview

This extension defines the metalness-roughness material model from Physically-Based Rendering (PBR) adding additional material parameters, which reference per vertex attributes. These additional material parameters, complete a full PBR workflow representation per vertex.

This extension allows meshes to have attributes of any name, and allows materials to select those attributes together with a swizzle (ie. rgba, r, a), so that attributes can be packed into a compact form (for example: METALLIC_ROUGHNESS_AMBIENTOCCLUSION as a single 3-component vector attribute).

This material <-> attribute indirection allows DCC applications to save multiple materials for a single mesh into glTF, and allows real-time engines to switch / blend between them at run-time. It also allows DCC applications to use glTF as a storage format, used for saving and loading artist defined shading configurations, minimizing data loss and/or shading configuration change / conversion over the save and load cycle(s).

### Example
![\[Comparison\]](Figures/vertex_metal_rough_comparison.png)

Left: per-vertex albedo only. Right: per-vertex albedo attributes extended with metalness-roughness

[painted_sphere.glb](examples/painted_sphere.glb)

## glTF Schema Updates

```C
{
    "asset": {
        "generator": "Unbound/PlayEngine",
        "version": "2.0"
    },
    "scene": 0,
    "extensionsUsed": [
        "EXT_pbr_attributes"
    ],
    "materials": [
        {
            "name": "vertexPBRMat",
            "pbrMetallicRoughness": {
                "baseColorFactor": [1, 1, 1, 1],
                "metallicFactor" : 1,
                "roughnessFactor": 1
            },
            "extensions" : {
                "EXT_pbr_attributes" : {
                    "baseColorAttribSpace" : "sRGB",
					"baseColorAttrib" : "COLOR_0",
					"baseColorSwizzle" : "rgb",
					"roughnessAttribSpace" : "linear",
					"roughnessAttrib" : "ROUGHNESS_METALLIC",
					"roughnessSwizzle" : "r",
					"metallicAttribSpace" : "linear",
					"metallicAttrib" : "ROUGHNESS_METALLIC",
					"metallicSwizzle" : "b",
                }
            }
        }
    ],
    "meshes": [
        {
            "name": "myMesh_metallic_roughness",
            "primitives": [
                {
                    "material": 0,
                    "mode": 4,
                    "attributes": {
                        "ROUGHNESS_METALLIC": 4,
                        "COLOR_0": 3,
                        "NORMAL": 2,
                        "POSITION": 1
                    },
                    "indices": 0
                }
            ]
        }
    ]
}
```

### JSON Schema


This extension adds on additional `enum` to the `materials` section. 

[Schema for color space selection](Schema/glTF.EXT_pbr_attributes.schema.json)

This enum provides information about the color space interpretation of the  vertex attributes used. If not present, `"linear"` is assumed. If color space is sRGB, the implementation is required to convert the color to linear space in order to correctly interpolate via the fixed function pipeline.

If one or more of the attributes are referenced by the material, they must be multiplied with the respective factor, for example, "roughnessAttrib" float value has to be multiplied with "roughnessFactor" of the pbrMetallicRoughness material.

If a relevant texture (for example, metallicRoughnessTexture) is defined in the material, an implementation must multiply the texture values with both attribute value and constant factor.

Swizzle is defined as one or more of letters in the standard 4 component vector "rgba". To simplify the implementations, each letter can appear only once in the swizzle string, so for example, "rrgg" swizzle is not allowed.

### Attributes

This extension allows any name to be used as a valid attribute semantic property name.
This extension also allows any accessor type and component type for each attribute. Implementations are encouraged to support at least FLOAT, UNSIGNED_BYTE and UNSIGNED_SHORT component types, in 1 to 4 component vector formats.

## Best Practices

The primary motivation of this extension is to allow PBR materials to be represented primarily by vertex attributes.
For this use case, it is recommended to set both "metallicFactor" and "roughnessFactor" to 1.0. Both factors can be then interpreted and used as a global "knob" for artistic control.
Loaders should pay attention to floating point precision such that 1.0 is exactly represented.

If an exporter implementation chooses to add a metallicRoughnessTexture, the texture values take semantic precedence with regards to being linear shading parameters and the attribute values are interpreted as a factor.
Such configurations are defined for consistency with base color, but are not recommended.

### Caveats with regards to Core
`EXT_pbr_attributes` is generally best placed in `"extensionsUsed"`.
In case of an implementation not supporting this extension, the resulting fallback will be a vertex-colored result. If material roughness and metalness factors were set to 1.0 as recommended, the fallback will result in a fully rough and fully metallic surface.
If sRGB was used for COLOR_0, the resulting color space and interpolation will be off.

Implementations concerned with these potentially undesirable results, maybe choose to add the extension to `"extensionsRequired"`.

When COLOR_0 attribute exists in the mesh, and baseColorAttrib is defined, baseColorAttrib has the priority, and COLOR_0 attribute (as per Core specification) is ignored. 

## Known Implementations

* Unbound from 0.2.6 onwards supports glTF 2.0 export with optional EXT_pbr_attributes 
* [TODO: add list]

## Resources

Example File: [painted_sphere.glb](examples/painted_sphere.glb)
