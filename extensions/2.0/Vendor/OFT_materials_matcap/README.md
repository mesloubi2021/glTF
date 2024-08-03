# OFT_materials_matcap

## Contributors

* Wenjie Liu, OppenFuture Technologies, liuwenjie@oppentech.com

## Status

Draft

## Dependencies

Written against the glTF 2.0 spec.

## Overview

This extension defines an matcap shading model for use in glTF 2.0 materials, is defined by a MatCap texture, which encodes the material color and shading. Matcap material does not respond to lights since the matcap image file encodes baked lighting.It will cast a shadow onto an object that receives shadows, but it will not self-shadow or receive shadows.

## Extending Materials

The common matcap material is defined by adding the OFT_materials_matcap extension to any glTF material.When present, the extension indicates that a material should be matcap material and use available baseColor values(factor or map) and normal texture while ignoring all properties of the default PBR model related to lighting or color. Alpha coverage and doubleSided still apply to matcap materials.

```json
{
    "materials": [
        {
            "name": "MyMatcapMaterial",
            "pbrMetallicRoughness": {
                "baseColorFactor": [ 0.5, 0.8, 0.0, 1.0 ],
                "baseColorTexture": {
                    "index": 1,
                    "texCoord": 1
                },
            },
            "normalTexture": {
                "scale": 2,
                "index": 3,
                "texCoord": 1
            },
            "extensions": {
                "KHR_materials_matcap": {
                    "matcapTexture": {
                        "index": 2
                    }
                }
            }
        }
    ]
}
```

## Properties

Only one property is contributed by the `OFT_materials_matcap` extension:
|                                  | Type                                                                            | Description                            | Required             |
|----------------------------------|---------------------------------------------------------------------------------|----------------------------------------|----------------------|
|**matcapTexture**                 | [`textureInfo`](/specification/2.0/README.md#reference-textureInfo)             | The matcap texture.                    | Yes                  |

> Matcap texture contains RGB components encoded with the sRGB transfer function.

> For the calculation of uv, please refer to the following implementation：
```glsl
    vec3 viewDir = normalize( vViewPosition );
    vec3 x = normalize( vec3( viewDir.z, 0.0, - viewDir.x ) );
    vec3 y = cross( viewDir, x );
    // 0.495 to remove artifacts caused by undersized matcap disks
    vec2 uv = vec2( dot( x, normal ), dot( y, normal ) ) * 0.495 + 0.5;
```
> The matcap color is then calculated by uv, and scaled the diffuse color by matcap color:
```glsl
    vec4 matcapColor = texture2D( matcap, uv );
    matcapColor = sRGBToLinear( matcapColor );
    diffuseColor.rgb *= matcapColor.rgb;
```



