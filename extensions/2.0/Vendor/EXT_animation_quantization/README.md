# EXT_animation_quantization

## Contributors

* Lukas Cone

## Status

Draft

## Dependencies

Written against the glTF 2.0 spec.

## Overview

In a current standard implementation, animation tracks must be stored as `FLOAT` component type with exception of rotation. Translation and scale must be stored as `VEC3` and rotation as `VEC4`.
Such implementation can be heavy on memory and transmition sizes.

This extension expands the set of allowed component types for animation sampler storage to provide a memory/precision tradeoff - depending on the application needs, 16-bit or 8-bit storage can be sufficient. It also adds ability to use sampler per axis for translations as well as uniform scale sampling.

In order to keep simplicity, this extension doesn't introduce new component types, but new schemas.

Translation tracks can use sampler per axis, this allows more freedom of reducing frames.

Scale tracks as well can use sampler per axis, but also a single `SCALAR` accesor can be used for uniform scaling.

Rotation tracks can be now processed in multiple ways. Quaternion doesn't need to store W component, such component can be recomputed from X, Y, Z values. If rotation is applied only for single axis, there is no need to store other axes, since they should be 0 values. In this case `SCALAR` or `VEC2` can be used istead of `VEC4`.

This extension also reintroduces schema from `KHR_texture_transform` to increase quantized precision.

Time values can now be quantized as well.

## glTF Schema Updates

```json
"animations": [
    {
        "channels": [
            {
                "sampler": 0,
                "target": {
                    "node": 1,
                    "path": "translation"
                },
                "extensions": {
                    "EXT_animation_quantization": {
                        "mainSamplerAxis": "X",
                        "offset": [-0.5, -0.5, -0.5],
                        "scale": [1, 1, 1],
                        "additionalSamplers": [
                            {
                                "id": 1,
                                "axis": "Y"
                            },
                        ]
                    }
                }
            },
        ],
        "extensions": {
            "EXT_animation_quantization": {
                "timeScale": 2.3333,
            }
        }
    }
]
```

### Properties

| |Type|Description|Required|
|-|-|-|-|
|**mainSamplerAxis**|`string`|Specifies what axis is animated from main sampler.|Depends|
|**offset**|`number[1-4]`|Applies offset to all sampled values.|No|
|**scale**|`number[1-4]`|Applies scale to values from samplers. Scale must be applied before offset!|No|
|**additionalSamplers**|`additional sampler [1-*]`|Additional samplers for other axes. It's forbidden to use this property for rotations!|No|

#### mainSamplerAxis

- Type: `string`
- Required only for:
  - Accessor type `"SCALAR"` and target.path `"translation"` and `"rotation"`.
  - Accessor type `"VEC2"` and target.path `"rotation"`.
- Allowed values:
  - `"X"`
  - `"Y"`
  - `"Z"`
  
  Only exception is for `"SCALAR"` accessor and target.path `"scale"`, where when set, it will scale around specified exis, if this property is not present, it scales object uniformly around all axes.

#### offset

Applies offset to all sampled values.

- Type: `number[1-4]`
- Required: No
- Default value: [0, 0, 0, 0]

#### scale

Applies scale to values from samplers. Scale must be applied before offset!

- Type: `number[1-4]`
- Required: No
- Default value: [1, 1, 1, 1]

#### additionalSamplers

Additional samplers for other axes. It's forbidden to use this property for rotations!

||Type|Description|Required|
|-|-|-|-|
|**id**|`integer`|Index of a sampler used to compute specified axis.|Yes|
|**axis**|`string`|Specifies what axis is animated.|Yes|

#### additionalSamplers.id

Index of a sampler used to compute specified axis

- Type: `integer`
- Required: Yes
- Minimum: `>=0`

#### additionalSamplers.axis

Specifies what axis is animated.

- Type: `string`
- Required: Yes
- Allowed values:
  - `"X"`
  - `"Y"`
  - `"Z"`

### Extending rotation accessors

|Accessor type|Component Type(s)|
|-|-|
|`"SCALAR"`<br>`"VEC2"`<br>`"VEC3"`<br>`"VEC4"`|`5126`&nbsp;(FLOAT)<br>`5120`&nbsp;(BYTE)&nbsp;normalized<br>`5121`&nbsp;(UNSIGNED_BYTE)&nbsp;normalized<br>`5122`&nbsp;(SHORT)&nbsp;normalized<br>`5123`&nbsp;(UNSIGNED_SHORT)&nbsp;normalized

#### Rotation as scalar

Such value represents single X, Y or Z axis, this mode should be used for rotations around single axis, all other axes (except W) in quaternion must be 0.

W component should be calculated as `W = sqrt(1.0 - <component> * <component>)`

#### Rotation as 2 component vector

First component must represent X, Y or Z. Second component always represents W.

This mode is same as `rotation as scalar` but without need of recomputing W component.

#### Rotation as 3 component vector

All components represents X, Y and Z axes.

W component should be calculated as `W = sqrt(1 - X * X + Y * Y + Z * Z)`

TODO: data alignment?

### Extending scale accessors

|Accessor type|Component Type(s)|
|-|-|
|`"SCALAR"`<br>`"VEC3"`|`5126`&nbsp;(FLOAT)<br>`5121`&nbsp;(UNSIGNED_BYTE)<br>`5121`&nbsp;(UNSIGNED_BYTE)&nbsp;normalized<br>`5123`&nbsp;(UNSIGNED_SHORT)<br>`5123`&nbsp;(UNSIGNED_SHORT)&nbsp;normalized

#### Scale as scalar

This mode can represent uniform scale across all axes or scale along single axis.

This is dermined whenever `"axis"`/`"mainSamplerAxis"` property is present.

 "timeStart": 0,
 "timeScale": 2.3333,

### Time properties

| |Type|Description|Required|
|-|-|-|-|
|**timeScale**|`number`|Applies scale to time value.|No|

#### timeScale

Applies scale to time value.

- Type: `number`
- Required: No

### Extending time accessors

|Accessor type|Component Type(s)|
|-|-|
|`"SCALAR"`|`5126`&nbsp;(FLOAT)<br>`5121`&nbsp;(UNSIGNED_BYTE)<br>`5121`&nbsp;(UNSIGNED_BYTE)&nbsp;normalized<br>`5123`&nbsp;(UNSIGNED_SHORT)<br>`5123`&nbsp;(UNSIGNED_SHORT)&nbsp;normalized

Restiction for time values still apply, values must strictly increase.

### Decoding/encoding quantized values

Implementations should assume following equations are used to get corresponding floating-point value `f` from a normalized integer `c` and should use the specified equations to encode floating-point values to integers after range normalization:

|`accessor.componentType`|int-to-float|float-to-int|
|-----------------------------|--------|----------------|
| `5120`&nbsp;(BYTE)          |`f = max(c / 127.0, -1.0)`|`c = round(f * 127.0)`|
| `5121`&nbsp;(UNSIGNED_BYTE) |`f = c / 255.0`|`c = round(f * 255.0)`|
| `5122`&nbsp;(SHORT)         |`f = max(c / 32767.0, -1.0)`|`c = round(f * 32767.0)`|
| `5123`&nbsp;(UNSIGNED_SHORT)|`f = c / 65535.0`|`c = round(f * 65535.0)`|

> **Implementation Note:** Due to OpenGL ES 2.0 / WebGL 1.0 platform differences, some implementations may decode signed normalized integers to floating-point values differently. While the difference is unlikely to be significant for normal/tangent data, implementations may want to use unsigned normalized or signed non-normalized formats to avoid the discrepancy in position data.

> **Implementation Note:** Un-normalized values can increase performance. Quantization decode fraction can be stored inside `scale` property, hence removing need for normalization.

### JSON Schema

TODO: After or if draft is approved
