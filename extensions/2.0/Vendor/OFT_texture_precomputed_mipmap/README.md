# OFT_texture_precomputed_mipmap

## Contributors

* Wenjie Liu, OppenFuture Technologies, liuwenjie@oppentech.com

## Status

Draft

## Dependencies

Written against the glTF 2.0 spec.

## Overview

This extension adds the ability to specify textures using precomputed mipmap.

The extension is added to the `textures` node and specifies a `sources` list whose members point to the indices of the `images` nodes, corresponding to all levels of the mipmap.

The levels must start with `0` and end with the maximum level of the mipmap as specified in WebGL specification.

The source image of level `0` must be the same as the source image of the specified texture.

## Example:

```json
"textures": [
    {
        "source": 0,
        "extensions": {
            "OFT_texture_precomputed_mipmap": {
                "sources": [
                    {
                        "level": 0,
                        "source": 0
                    },
                    {
                        "level": 1,
                        "source": 1
                    },
                    {
                        "level": 2,
                        "source": 2
                    },
                    {
                        "level": 3,
                        "source": 3
                    },
                    {
                        "level": 4,
                        "source": 4
                    }
                ]
            }
        }
    }
]
```
