# OMI_texture_materialx

## Contributors

* Ernest Lee (iFire), V-Sekai, [@fire](https://github.com/fire)

## Status

In development.

## Dependencies

Written against the glTF 2.0 spec.

## Overview

This extension allows glTF models to use MaterialX as a valid image format. A client that does not implement this extension can ignore the provided MaterialX image and continue to rely on the PNG and JPG textures available in the base specification. Defining a fallback texture is optional. The [best practices](#best-practices) section describes the intended use case of this extension and the expected behavior when using it without a fallback texture.

MaterialX is an open standard for transfer of rich material and look-development content between applications and renderers. - [source](https://github.com/materialx/MaterialX)

## glTF Schema Updates

The `OMI_texture_materialx` extension is added to the `materials` node.

<!-- 
The `OMI_texture_materialx` extension is added to the `textures` node and specifies a `source` property that points to the index of the `images` node which in turn points to the MaterialX image.

The following glTF will load `image.mtlx` in clients that support this extension, and otherwise fall back to `image.png`.

```
"textures": [
    {
        "source": 0,
        "extensions": {
            "OMI_texture_materialx": {
                "source": 1
            }
        }
    }
],
"images": [
    {
        "uri": "image.png"
    },
    {
        "uri": "image.mtlx"
    }
]
```

When used in the glTF Binary (.glb) format the `images` node that points to the MaterialX image uses the `mimeType` value of `image/materialx`. 

```
"textures": [
    {
        "source": 0,
        "extensions": {
            "OMI_texture_materialx": {
                "source": 1
            }
        }
    }
],
"images": [
    {
        "mimeType": "image/png",
        "bufferView": 1
    },
    {
        "mimeType": "image/materialx",
        "bufferView": 2
    }
]
``` -->

### JSON Schema

<!-- [glTF.OMI_texture_materialx.schema.json](schema/glTF.OMI_texture_materialx.schema.json) -->

## Best Practices

Since MaterialX is not as widely supported as JPEG and PNG, it is recommended to use this extension only when the features exposed by MaterialX are required. For example, in commerce applications the variations of materials can be requested at minimal cost, Material textures can be used without a fallback to improve storage and transmission costs.

When a fallback image is defined, this extension should not be present in `extensionsRequired`. This will allow all clients to render the glTF correctly, and those that support this extension can request the optimized MaterialX version of the textures.

### Using Without a Fallback

To use MaterialX images without a fallback, define `OMI_texture_materialx` in both `extensionsUsed` and `extensionsRequired`. The `texture` node will then have only an `extensions` property as shown below.

```
"textures": [
    {
        "extensions": {
            "OMI_texture_materialx": {
                "source": 1
            }
        }
    }
]
```

If a glTF contains a MaterialX with no fallback texture and the browser does not support MaterialX, the client should either display an error or decode the MaterialX image at runtime.

## Known Implementations

V-Sekai uses it for prototyping character atlases.

## Resources

Lucasfilm's [MaterialX page](https://www.materialx.org/) is a good resource about the format.

For browsers that do not natively support MaterialX, there is pending work to implement a wasm based materialx module.
