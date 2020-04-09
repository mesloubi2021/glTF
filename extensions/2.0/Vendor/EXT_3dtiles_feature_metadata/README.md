# EXT\_3dtiles\_feature\_metadata

## Contributors

* Sean Lilley, Cesium
* Samuel Vargas, Cesium
* Sam Suhag, Cesium
* Patrick Cozzi, Cesium

## Status

Draft

## Dependencies

Written against the glTF 2.0 spec. Depends on [`EXT_mesh_gpu_instancing`](https://github.com/KhronosGroup/glTF/pull/1691) for instanced features.

## Overview

In Geographic Information Systems (GIS) a feature is an entity that has both geometry and application-specific properties. This extension adds a mechanism for identifying features within the glTF's vertex and texture data, as well as a database for storing per-feature properties.

A feature may be a building in a city, a pipe in a BIM/CAD model, a point in a point cloud, and an instanced tree in a forest. A glTF may contain any number of features and any number of per-feature properties.

<p align="center">
<img src="./figures/feature-table-buildings.png" >
</p>

Geometry from different features may be batched into as few glTF primitives as possible to enable efficient rendering and interaction. Feature IDs, stored per-vertex or per-texel, enable individual features to be identified and updated at runtime, e.g., show/hide, highlight color, etc. Feature IDs may also be used to query properties from a feature table for styling and application-specific use cases such as populating a UI or issuing a REST API request. Some example feature table properties are building heights, geographic coordinates, and database primary keys.

A glTF primitive may contain one or more feature layers where each feature layer points to a feature table. Multiple feature layers enable different levels of feature granularity within a single dataset. For example, a point cloud may contain groups of points that represent different architectural components of a building while retaining per-point properties like intensity - in the first layer each group is considered a feature; in the second layer each point is considered a feature. Multiple feature layers also enable heterogenous datasets to be combined into a single glTF payload.

<table style="table-layout: fixed">
  <tr>
    <td style="width: 50%;"><img src="./figures/point-cloud-layers.png" ></td>
    <td><img src="./figures/composite-example.png" ></td>
  </tr>
  <tr>
    <td>Left: A point cloud with two feature tables, one storing per-group properties and the other storing per-point properties.</td>
    <td>Right: A glTF containing a 3D mesh (house), a point cloud (tree), and instanced models (fencing) with three feature tables.</td>
  </tr>
</table>

A feature table may be extended to include feature classes and hierarchies. See the `EXT_3dtiles_feature_hierarchy` extension.

## Example

The following example contains two features and a feature table with a mix of JSON and binary properties.

<p>
<img src="./figures/basic-example.png">
</p>


```json
{
  "meshes": [
    {
      "primitives": [
        {
          "attributes": {
            "POSITION": 0,
            "_FEATURE_ID_0": 1
          },
          "indices": 2,
          "extensions": {
            "EXT_3dtiles_feature_metadata": {
              "featureLayers": [
                {
                  "featureTable": 0,
                  "featureIds": {
                    "attribute": "_FEATURE_ID_0"
                  }
                }
              ]
            }
          }
        }
      ]
    }
  ],
  "extensions": {
    "EXT_3dtiles_feature_metadata": {
      "featureTables": [
        {
          "featureCount": 2,
          "featureProperties": {
            "Name": {
              "array": {
                "type": "string",
                "values": ["Building A", "Building B"]
              }
            },
            "Year": {
              "array": {
                "type": "number",
                "values": [1999, 2015]
              }
            },
            "Coordinates": {
              "accessor": 3
            }
          }
        }
      ]
    }
  },
  "accessors": [
    {
      "name": "Positions",
      "bufferView": 0,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 8,
      "type": "VEC3"
    },
    {
      "name": "Feature IDs",
      "bufferView": 1,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 8,
      "type": "SCALAR"
    },
    {
      "name": "Indices",
      "bufferView": 2,
      "byteOffset": 0,
      "componentType": 5123,
      "count": 12,
      "type": "SCALAR"
    },
    {
      "name": "Coordinates (longitude, latitude)",
      "bufferView": 3,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 2,
      "type": "VEC2"
    }
  ]
}
```

## Concepts

### Feature layers

A feature layer defines the mapping between geometry and features. There are two types of feature layers - ones that contain feature IDs, used to query properties from a feature table, and ones that contain feature properties directly.

A feature layer object contains the following properties:

* `featureTable`: the index of the feature table used by this layer
* `featureIds`: an object describing how to access feature IDs
* `featureProperties`: an object describing how to access feature properties

`featureIds` and `featureProperties` are mutually exclusive and have different characteristics when interfacing with a feature table.

Multiple feature layers within a single primitive cannot use the same feature table.

#### Feature IDs

Feature IDs are stored in vertex attributes or textures and are used to identify the features in a primitive. Feature IDs, whether sourced from a vertex attribute or texture, are integral values in the range `[0, featureCount - 1]`, where `featureCount` is the number of features in the layer's feature table. Feature properties are stored in the feature table, where feature ID is an index into each property array.

The `featureIds` object has the following properties:

* `attribute`: the name of a vertex attribute containing feature IDs
* `textureAccessor`: a view into a texture containing feature IDs
* `instanceStride`: an optional property that specifies the per-instance stride to apply to feature IDs when the mesh is instanced by the `EXT_mesh_gpu_instancing` extension. Set `instanceStride` to 1 and initialize `attribute`'s accessor with zeros (e.g. by not defining `bufferView`) to treat each instance as a separate feature. Explicit feature ids and custom strides may provide a lower level of feature granularity if desired.

`attribute` and `textureAccessor` are mutually exclusive. Per-vertex and per-texel feature IDs cannot be used in the same feature layer.

##### Per-vertex feature IDs

A feature is often represented geometrically as a group of vertices, like the vertices of a building in the [example](#example) above. Each vertex has a feature ID attribute indicating the feature to which it belongs.

For example, vertices for a primitive with three features may look like this:

```
feature id:  [0,   0,   0,   ..., 1,   1,   1,   ..., 2,   2,   2,   ...]
position:    [xyz, xyz, xyz, ..., xyz, xyz, xyz, ..., xyz, xyz, xyz, ...]
normal:      [xyz, xyz, xyz, ..., xyz, xyz, xyz, ..., xyz, xyz, xyz, ...]
```

Vertices do not need to be ordered by feature ID so the following is also OK:

```
feature id:  [0,   1,   2,   ..., 2,   1,   0,   ..., 1,   2,   0,   ...]
position:    [xyz, xyz, xyz, ..., xyz, xyz, xyz, ..., xyz, xyz, xyz, ...]
normal:      [xyz, xyz, xyz, ..., xyz, xyz, xyz, ..., xyz, xyz, xyz, ...]
```

Note that a vertex can't belong to more than one feature; in that case, the vertex needs to be duplicated so the feature IDs can be assigned.

The feature ID attribute is specified in a glTF mesh primitive by providing the `_FEATURE_ID_0` indexed attribute semantic.

###### Feature ID semantic

This extension adds a new indexed attribute semantic `_FEATURE_ID_0`. All indices must start with 0 and be continuous positive integers: `_FEATURE_ID_0`, `_FEATURE_ID_1`, `_FEATURE_ID_2`, etc.

The attribute's accessor `type` must be `"SCALAR"` and `normalized` must be `false`. There is no restriction on `componentType`. Accessor values must be integral values in the range `[0, featureCount - 1]`.

###### Example

An example of two feature layers with per-vertex feature IDs:

```json
{
  "primitives": [
    {
      "attributes": {
        "POSITION": 0,
        "_FEATURE_ID_0": 1,
        "_FEATURE_ID_1": 2
      },
      "indices": 3,
      "extensions": {
        "EXT_3dtiles_feature_metadata": {
          "featureLayers": [
            {
              "featureTable": 0,
              "featureIds": {
                "attribute": "_FEATURE_ID_0"
              }
            },
            {
              "featureTable": 1,
              "featureIds": {
                "attribute": "_FEATURE_ID_1"
              }
            }
          ]
        }
      }
    }
  ]
}
```

##### Per-texel feature IDs

Alternatively feature IDs can be stored in a texture and accessed with a [`textureAccessor`](#texture-accessor).

For a given point on the mesh's surface, the feature ID is sampled from the feature ID texture using the interpolated texture coordinates of the given TEXCOORD set, just like in traditional texture mapping.

The `textureAccessor`'s `channels` string must be of length one and `normalized` must be false (default). The texture's sampler must use nearest neighbor filtering - i.e. `minFilter` and `magFilter` must be `9728` (`NEAREST`).

###### Example

```json
{
  "primitives": [
    {
      "attributes": {
        "POSITION": 0,
        "TEXCOORD_0": 1,
      },
      "indices": 2,
      "extensions": {
        "EXT_3dtiles_feature_metadata": {
          "featureLayers": [
            {
              "featureTable": 0,
              "featureIds": {
                "textureAccessor": {
                  "texture": {
                    "texCoord": 0,
                    "index": 0
                  },
                  "channels": "r"
                }
              }
            }
          ]
        }
      }
    }
  ]
}
```

#### Feature properties

In some cases it is wasteful to use feature IDs, like when properties vary vertex-by-vertex or texel-by-texel. Instead properties may be stored directly in the primitive rather than in the feature table.

`featureProperties` is an object describing how to access properties stored directly in the primitive. `featureProperties` must contain an entry for each property in the layer's feature table.

Each entry is an object containing either an `attribute`, the name of a vertex attribute containing property values, or a `textureAccessor`, a view into a texture containing property values. `featureProperties` cannot have a mix of per-vertex and per-texel properties. When `featureProperties` contains per-texel properties, all `texCoord` values must be the same.

##### Examples

Per-vertex properties:

```json
{
  "primitives": [
    {
      "attributes": {
        "POSITION": 0,
        "_TEMPERATURE": 1,
        "_INTENSITY:": 2
      },
      "indices": 3,
      "extensions": {
        "EXT_3dtiles_feature_metadata": {
          "featureLayers": [
            {
              "featureTable": 0,
              "featureProperties": {
                "Temperature": {
                  "attribute": "_TEMPERATURE"
                },
                "Intensity": {
                  "attribute": "_INTENSITY"
                }
              }
            }
          ]
        }
      }
    }
  ]
}
```

Per-texel properties:

```json
{
  "primitives": [
    {
      "attributes": {
        "POSITION": 0,
        "TEXCOORD_0": 1,
      },
      "indices": 2,
      "extensions": {
        "EXT_3dtiles_feature_metadata": {
          "featureLayers": [
            {
              "featureTable": 0,
              "featureProperties": {
                "Temperature": {
                  "textureAccessor": {
                    "texture": {
                      "texCoord": 0,
                      "index": 0
                    },
                    "channels": "r",
                    "normalized": false
                  }
                },
                "Intensity": {
                  "textureAccessor": {
                    "texture": {
                      "texCoord": 0,
                      "index": 1
                    },
                    "channels": "r",
                    "normalized": true
                  }
                }
              }
            }
          ]
        }
      }
    }
  ]
}
```

### Texture Accessor

A texture accessor contains a reference to a glTF texture and information for accessing its values:

* `texture` - a `textureInfo` object
* `channels` - a string that implicitly defines both the type (`"SCALAR"`, `"VEC2"`, `"VEC3"`, `"VEC4"`) and the color channels to read from. Example: `"rgb"` signifies that this property's `type` is `"VEC3"` its values are obtained from the red, green, and blue channels. This is useful when packing properties into the same texture: one texture accessor might be `"r"`, another might be `"gb"` and a last might be `"a"`. The string must match the pattern `/^[rgba]{1,4}$/`. If the texture does not contain a particular color channel then the maximum value must be returned (e.g. 255 for 8-bit color depth).
* `normalized` - whether the texture data should be normalized to the `[0.0, 1.0]` range or not. This property overrides the normalization specified in the native image format (e.g. if `normalized` is true and the KTX2 image format is `VK_FORMAT_R8_UNORM` the values will still be normalized). Defaults to `false`.

Example: read data from a grayscale texture and normalize to the `[0.0, 1.0]` range.

```json
{
  "texture": {
    "index": 0
  },
  "channels": "r",
  "normalized": true
}
```


### Feature Table

A feature table stores information about per-feature application-specific properties. There are two types of feature tables - ones containing property arrays that can be indexed by a feature ID, and ones that simply store metadata about properties that are stored externally in primitives.

A feature layer with `featureIds` can only use a feature table of the first type. In this case `featureCount` will be defined, a value indicating the number of features in the feature table, and equivalently, the range of valid feature IDs that can index into the feature table. Property values may be stored in JSON arrays or glTF accessors.

A feature layer with `featureProperties` can only use a feature table of the second type. `featureCount` will not be defined and the feature table will just store metadata rather than actual property values.

Properties are stored by name in the `featureProperties` object. A feature table may contain any number of properties, or no properties at all. Property names must be unique across all feature tables.

As stated above, feature table properties can be represented in three different ways:

1. `array`: an array of values
    * Array elements can be any valid JSON data type, including objects and arrays.  Elements may be `null`.
    * The length of each array must be equal to `featureCount`
    * Elements in the array must conform to the `type` property. Valid `type` values are `"string"`, `"number"`, `"boolean"`, and `"any"` (default), where `"any"` must be used if the array contains arrays, objects, `null`, or mixed types.
2. `accessor`: a reference to binary data stored in a glTF accessor
    * The accessor's `count` must be equal to `featureCount`
3. `descriptor`: describes the format of external properties
    * Properties stored in vertex attribute accessors must have the same `type`, `componentType`, and `normalized` values. `normalized` is false by default.
    * Properties stored in textures must have the same `type` (implicitly defined by `channels`), `componentType` (implicitly defined by the texture's bit depth and signedness), and `normalized`

`array`, `accessor`, and `descriptor` are mutually exclusive.

Properties may also optionally define a `semantic`, an enumerated value describing how the property is to be interpreted. Application-specific semantics must start with an underscore, e.g., `_CLASSIFICATION`. Semantics must be unique within an individual feature table.

List of built-in semantics:

Semantic | Type | Description
--|--|--
`NAME`|`string`| The name of the feature. Names do not have to be unique.
`ID`|`number` or `string`|A unique identifier for this feature.

Example feature table with built-in and application-specific semantics:

```json
{
  "featureCount": 4,
  "featureProperties": {
    "bldg_name": {
      "semantic": "NAME",
      "array": {
        "type": "string",
        "values": ["House", "Store", "Bank", "House"]
      }
    },
    "bldg_uuid": {
      "semantic": "ID",
      "array": {
        "type": "string",
        "values": [
          "752ac29d-c262-473b-bd85-aff3c471f3c8",
          "c3006d52-099a-4c05-95a1-a91a1be71f0e",
          "9e011aa9-0ed9-497c-923f-0bed8c286134",
          "cd876cc5-f2d3-400b-ad64-f5dee35f0520"
        ]
      }
    },
    "bldg_classif": {
      "semantic": "_CLASSIFICATION",
      "array": {
        "type": "number",
        "values": [0, 1, 2, 0]
      }
    }
  }
}
```

Additional application-specific metadata about a property may be stored in the property's `extras` object:

```json
{
  "featureCount": 10,
  "featureProperties": {
    "ClassificationIds": {
      "accessor": 0,
      "extras": {
        "version": "1.4"
      }
    }
  }
}
```

```json
{
  "featureProperties": {
    "Accuracy": {
      "descriptor": {
        "type": "SCALAR",
        "componentType": 5121,
        "normalized": false
      },
      "extras": {
        "Offset": 0.1,
        "Gain": 20.0
      }
    }
  }
}
```

## Optional vs. Required

This extension is optional, meaning it should be placed in the glTF root's `extensionsUsed` list, but not in the `extensionsRequired` list.

## See Also

TODO

## glTF Schema Updates

TODO

## Known Implementations

TODO

## Additional Examples

### Two building features with JSON properties

```json
{
  "accessors": [
    {
      "name": "Positions",
      "bufferView": 0,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 8,
      "type": "VEC3"
    },
    {
      "name": "Feature IDs",
      "bufferView": 1,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 8,
      "type": "SCALAR"
    },
    {
      "name": "Indices",
      "bufferView": 2,
      "byteOffset": 0,
      "componentType": 5123,
      "count": 12,
      "type": "SCALAR"
    }
  ],
  "meshes": [
    {
      "primitives": [
        {
          "attributes": {
            "POSITION": 0,
            "_FEATURE_ID_0": 1
          },
          "indices": 2,
          "material": 0,
          "mode": 4,
          "extensions": {
            "EXT_3dtiles_feature_metadata": {
              "featureLayers": [
                {
                  "featureTable": 0,
                  "featureIds": {
                    "attribute": "_FEATURE_ID_0"
                  }
                }
              ]
            }
          }
        }
      ]
    }
  ],
  "extensionsUsed": [
    "EXT_3dtiles_feature_metadata"
  ],
  "extensions": {
    "EXT_3dtiles_feature_metadata": {
      "featureTables": [
        {
          "featureCount": 2,
          "featureProperties": {
            "Name": {
              "array": {
                "type": "string",
                "values": ["Building A", "Building B"]
              }
            },
            "Year": {
              "array": {
                "type": "number",
                "values": [1999, 2015]
              }
            },
            "Address": {
              "array": {
                "type": "any",
                "values": [
                  {
                    "Street": "Main Street",
                    "HouseNumber": "1"
                  },
                  {
                    "Street": "Main Street",
                    "HouseNumber": "2"
                  }
                ]
              }
            }
          }
        }
      ]
    }
  }
}
```

### Two building features with binary properties

```json
{
  "accessors": [
    {
      "name": "Positions",
      "bufferView": 0,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 8,
      "type": "VEC3"
    },
    {
      "name": "Feature Ids",
      "bufferView": 1,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 8,
      "type": "SCALAR"
    },
    {
      "name": "Indices",
      "bufferView": 2,
      "byteOffset": 0,
      "componentType": 5123,
      "count": 12,
      "type": "SCALAR"
    },
    {
      "name": "Ids",
      "bufferView": 3,
      "byteOffset": 0,
      "componentType": 5125,
      "count": 2,
      "type": "SCALAR"
    },
    {
      "name": "Coordinates",
      "bufferView": 4,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 2,
      "type": "VEC2"
    }
  ],
  "meshes": [
    {
      "primitives": [
        {
          "attributes": {
            "POSITION": 0,
            "_FEATURE_ID_0": 1
          },
          "indices": 2,
          "material": 0,
          "mode": 4,
          "extensions": {
            "EXT_3dtiles_feature_metadata": {
              "featureLayers": [
                {
                  "featureTable": 0,
                  "featureIds": {
                    "attribute": "_FEATURE_ID_0"
                  }
                }
              ]
            }
          }
        }
      ]
    }
  ],
  "extensionsUsed": [
    "EXT_3dtiles_feature_metadata"
  ],
  "extensions": {
    "EXT_3dtiles_feature_metadata": {
      "featureTables": [
        {
          "featureCount": 2,
          "featureProperties": {
            "Id": {
              "accessor": 3
            },
            "Coordinates": {
              "accessor": 4
            }
          }
        }
      ]
    }
  }
}
```

### Point cloud with per-point properties and per-group properties

```json
{
  "accessors": [
    {
      "name": "Positions",
      "bufferView": 0,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 8,
      "type": "VEC3"
    },
    {
      "name": "Colors",
      "bufferView": 1,
      "byteOffset": 0,
      "componentType": 5121,
      "count": 8,
      "stride": 4,
      "type": "VEC3"
    },
    {
      "name": "Feature IDs",
      "bufferView": 2,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 8,
      "type": "SCALAR"
    },
    {
      "name": "Intensity",
      "bufferView": 3,
      "byteOffset": 0,
      "componentType": 5123,
      "count": 8,
      "type": "SCALAR",
      "normalized": true
    },
    {
      "name": "Classification",
      "bufferView": 4,
      "byteOffset": 0,
      "componentType": 5121,
      "count": 8,
      "type": "SCALAR"
    },
    {
      "name": "Id",
      "bufferView": 5,
      "byteOffset": 0,
      "componentType": 5125,
      "count": 2,
      "type": "SCALAR"
    },
    {
      "name": "Coordinates",
      "bufferView": 6,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 2,
      "type": "VEC2"
    }
  ],
  "meshes": [
    {
      "primitives": [
        {
          "attributes": {
            "POSITION": 0,
            "COLOR_0": 1,
            "_FEATURE_ID_0": 2,
            "_INTENSITY": 3,
            "_CLASSIFICATION": 4
          },
          "mode": 0,
          "extensions": {
            "EXT_3dtiles_feature_metadata": {
              "featureLayers": [
                {
                  "featureTable": 0,
                  "featureProperties": {
                    "Intensity": {
                      "attribute": "_INTENSITY"
                    },
                    "Classification": {
                      "attribute": "_CLASSIFICATION"
                    }
                  }
                },
                {
                  "featureTable": 1,
                  "featureIds": {
                    "attribute": "_FEATURE_ID_0"
                  }
                }
              ]
            }
          }
        }
      ]
    }
  ],
  "extensionsUsed": [
    "EXT_3dtiles_feature_metadata"
  ],
  "extensions": {
    "EXT_3dtiles_feature_metadata": {
      "featureTables": [
        {
          "featureProperties": {
            "Intensity": {
              "descriptor": {
                "componentType": 5123,
                "type": "SCALAR",
                "normalized": true
              }
            },
            "Classification": {
              "descriptor": {
                "componentType": 5121,
                "type": "SCALAR"
              }
            }
          }
        },
        {
          "featureCount": 2,
          "featureProperties": {
            "Id": {
              "accessor": 5
            },
            "Coordinates": {
              "accessor": 6
            }
          }
        }
      ]
    }
  }
}
```

### Single feature example

```json
{
  "accessors": [
    {
      "name": "Positions",
      "bufferView": 0,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 4,
      "type": "VEC3"
    },
    {
      "name": "Feature Ids",
      "componentType": 5126,
      "count": 4,
      "type": "SCALAR"
    },
    {
      "name": "Indices",
      "bufferView": 1,
      "byteOffset": 0,
      "componentType": 5123,
      "count": 6,
      "type": "SCALAR"
    },
    
  ],
  "meshes": [
    {
      "primitives": [
        {
          "attributes": {
            "POSITION": 0,
            "_FEATURE_ID_0": 1
          },
          "indices": 2,
          "material": 0,
          "mode": 4,
          "extensions": {
            "EXT_3dtiles_feature_metadata": {
              "featureLayers": [
                {
                  "featureTable": 0,
                  "featureIds": {
                    "attribute": "_FEATURE_ID_0"
                  }
                }
              ]
            }
          }
        }
      ]
    }
  ],
  "extensionsUsed": [
    "EXT_3dtiles_feature_metadata"
  ],
  "extensions": {
    "EXT_3dtiles_feature_metadata": {
      "featureTables": [
        {
          "featureCount": 1,
          "featureProperties": {
            "Name": {
              "array": {
                "type": "string",
                "values": ["Building name"]
              }
            }
          }
        }
      ]
    }
  }
}
```

### Composite example

```json
{
  "accessors": [
    {
      "name": "Point positions",
      "bufferView": 0,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 8,
      "type": "VEC3"
    },
    {
      "name": "Point colors",
      "bufferView": 1,
      "byteOffset": 0,
      "componentType": 5121,
      "count": 8,
      "stride": 4,
      "type": "VEC3"
    },
    {
      "name": "Point feature Ids",
      "bufferView": 2,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 8,
      "type": "SCALAR"
    },
    {
      "name": "Intensity",
      "bufferView": 3,
      "byteOffset": 0,
      "componentType": 5123,
      "count": 8,
      "type": "SCALAR",
      "normalized": true
    },
    {
      "name": "Classification",
      "bufferView": 4,
      "byteOffset": 0,
      "componentType": 5121,
      "count": 8,
      "type": "SCALAR"
    },
    {
      "name": "Id",
      "bufferView": 5,
      "byteOffset": 0,
      "componentType": 5125,
      "count": 2,
      "type": "SCALAR"
    },
    {
      "name": "Coordinates",
      "bufferView": 6,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 2,
      "type": "VEC2"
    },
    {
      "name": "Positions",
      "bufferView": 7,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 8,
      "type": "VEC3"
    },
    {
      "name": "Feature IDs",
      "bufferView": 8,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 8,
      "type": "SCALAR"
    },
    {
      "name": "Indices",
      "bufferView": 9,
      "byteOffset": 0,
      "componentType": 5123,
      "count": 12,
      "type": "SCALAR"
    }
  ],
  "meshes": [
    {
      "primitives": [
        {
          "name": "Point cloud",
          "attributes": {
            "POSITION": 0,
            "COLOR_0": 1,
            "_FEATURE_ID_0": 2,
            "_INTENSITY": 3,
            "_CLASSIFICATION": 4
          },
          "mode": 0,
          "extensions": {
            "EXT_3dtiles_feature_metadata": {
              "featureLayers": [
                {
                  "featureTable": 0,
                  "featureProperties": {
                    "Intensity": {
                      "attribute": "_INTENSITY"
                    },
                    "Classification": {
                      "attribute": "_CLASSIFICATION"
                    }
                  }
                },
                {
                  "featureTable": 1,
                  "featureIds": {
                    "attribute": "_FEATURE_ID_0"
                  }
                }
              ]
            }
          }
        },
        {
          "name": "Buildings",
          "attributes": {
            "POSITION": 7,
            "_FEATURE_ID_0": 8
          },
          "indices": 9,
          "material": 0,
          "mode": 4,
          "extensions": {
            "EXT_3dtiles_feature_metadata": {
              "featureLayers": [
                {
                  "featureTable": 2,
                  "featureIds": {
                    "attribute": "_FEATURE_ID_0"
                  }
                }
              ]
            }
          }
        }
      ]
    }
  ],
  "extensionsUsed": [
    "EXT_3dtiles_feature_metadata"
  ],
  "extensions": {
    "EXT_3dtiles_feature_metadata": {
      "featureTables": [
        {
          "featureProperties": {
            "Intensity": {
              "descriptor": {
                "componentType": 5123,
                "type": "SCALAR",
                "normalized": true
              }
            },
            "Classification": {
              "descriptor": {
                "componentType": 5121,
                "type": "SCALAR"
              }
            }
          }
        },
        {
          "featureCount": 2,
          "featureProperties": {
            "Id": {
              "accessor": 5
            },
            "Coordinates": {
              "accessor": 6
            }
          }
        },
        {
          "featureCount": 2,
          "featureProperties": {
            "Name": {
              "array": {
                "type": "string",
                "values": ["Building A", "Building B"]
              }
            }
          }
        }
      ]
    }
  }
}
```

### Instancing example

```json
{
  "nodes": [
    {
      "mesh": 0,
      "extensions": {
        "EXT_mesh_gpu_instancing": {
          "attributes": {
            "TRANSLATION": 4
          }
        }
      }
    }
  ],
  "accessors": [
    {
      "name": "Positions",
      "bufferView": 0,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 12,
      "type": "VEC3"
    },
    {
      "name": "Implicit Feature IDs",
      "componentType": 5126,
      "count": 12,
      "type": "SCALAR"
    },
    {
      "name": "Explicit Feature IDs",
      "bufferView": 1,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 12,
      "type": "SCALAR"
    },
    {
      "name": "Indices",
      "bufferView": 2,
      "byteOffset": 0,
      "componentType": 5123,
      "count": 18,
      "type": "SCALAR"
    },
    {
      "name": "Per-Instance TRANSLATION",
      "bufferView": 3,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 2,
      "type": "VEC3"
    }
  ],
  "meshes": [
    {
      "primitives": [
        {
          "attributes": {
            "POSITION": 0,
            "_FEATURE_ID_0": 1,
            "_FEATURE_ID_1": 2
          },
          "indices": 3,
          "extensions": {
            "EXT_3dtiles_feature_metadata": {
              "featureLayers": [
                {
                  "featureTable": 0,
                  "featureIds": {
                    "instanceStride": 1,
                    "attribute": "_FEATURE_ID_0"
                  }
                },
                {
                  "featureTable": 1,
                  "featureIds": {
                    "instanceStride": 6,
                    "attribute": "_FEATURE_ID_1"
                  }
                }
              ]
            }
          }
        }
      ]
    }
  ],
  "extensions": {
    "EXT_3dtiles_feature_metadata": {
      "featureTables": [
        {
          "featureCount": 2,
          "featureProperties": {
            "Name": {
              "array": {
                "type": "string",
                "values": ["tree1", "tree2"]
              }
            }
          }
        },
        {
          "featureCount": 6,
          "featureProperties": {
            "State": {
              "array": {
                "type": "string",
                "values": ["normal", "normal", "broken", "nest", "normal", "normal"]
              }
            }
          }
        }
      ]
    }
  }
}
```

### Multiple texture layers with per-texel metadata

```json
{
  "accessors": [
    {
      "name": "Positions",
      "bufferView": 0,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 8,
      "type": "VEC3"
    },
    {
      "name": "Texcoords for color / material_id / accuracy",
      "bufferView": 1,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 8,
      "type": "VEC2"
    },
    {
      "name": "Texcoords for ortho classification and vegetation",
      "bufferView": 2,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 8,
      "type": "VEC2"
    },
    {
      "name": "Indices",
      "bufferView": 3,
      "byteOffset": 0,
      "componentType": 5123,
      "count": 24,
      "type": "SCALAR"
    }
  ],
  "images": [
    {
      "uri": "color.jpg"
    },
    {
      "uri": "material_id.png"
    },
    {
      "uri": "classification_ortho.png"
    },
    {
      "uri": "accuracy.png"
    },
    {
      "uri": "vegetation_ortho.png"
    }
  ],
  "textures": [
    {
      "source": 0
    },
    {
      "source": 1
    },
    {
      "source": 2
    },
    {
      "source": 3
    },
    {
      "source": 4
    },
  ],
  "materials": [
    {
      "pbrMetallicRoughness": {
        "baseColorTexture": {
          "index": 0,
          "texCoord": 0
        }
      }
    }
  ],
  "meshes": [
    {
      "primitives": [
        {
          "attributes": {
            "POSITION": 0,
            "TEXCOORD_0": 1,
            "TEXCOORD_1": 2
          },
          "indices": 3,
          "material": 0,
          "mode": 4,
          "extensions": {
            "EXT_3dtiles_feature_metadata": {
              "featureLayers": [
                {
                  "featureTable": 0,
                  "featureIds": {
                    "textureAccessor": {
                      "texture": {
                        "texCoord": 0,
                        "index": 1
                      },
                      "channels": "r"
                    }
                  }
                },
                {
                  "featureTable": 1,
                  "featureIds": {
                    "textureAccessor": {
                      "texture": {
                        "texCoord": 1,
                        "index": 2
                      },
                      "channels": "r"
                    }
                  }
                },
                {
                  "featureTable": 2,
                  "featureProperties": {
                    "Accuracy": {
                      "textureAccessor": {
                        "texture": {
                          "texCoord": 0,
                          "index": 3
                        },
                        "channels": "r",
                        "normalized": false
                      }
                    }
                  }
                },
                {
                  "featureTable": 3,
                  "featureProperties": {
                    "VegetationDensity": {
                      "textureAccessor": {
                        "texture": {
                          "texCoord": 1,
                          "index": 4
                        },
                        "channels": "r",
                        "normalized": false
                      }
                    },
                    "VegetationHealth": {
                      "textureAccessor": {
                        "texture": {
                          "texCoord": 1,
                          "index": 4
                        },
                        "channels": "g",
                        "normalized": false
                      }
                    }
                  }
                }
              ]
            }
          }
        }
      ]
    }
  ],
  "extensionsUsed": [
    "EXT_3dtiles_feature_metadata"
  ],
  "extensions": {
    "EXT_3dtiles_feature_metadata": {
      "featureTables": [
        {
          "featureCount": 5,
          "featureProperties": {
            "MaterialId": {
              "array": {
                "type": "string",
                "values": ["dirt", "grass", "wood", "brick", "glass"]
              }
            }
          }
        },
        {
          "featureCount": 7,
          "featureProperties": {
            "ClassificationId": {
              "array": {
                "type": "number",
                "values": [0, 1, 9, 10, 11, 12, 24]
              }
            }
          }
        },
        {
          "featureProperties": {
            "Accuracy": {
              "descriptor": {
                "type": "SCALAR",
                "componentType": 5121,
                "normalized": false
              },
              "extras": {
                "transform": [-128.0, 0.001]
              }
            }
          }
        },
        {
          "featureProperties": {
            "VegetationDensity": {
              "descriptor": {
                "type": "SCALAR",
                "componentType": 5121,
                "normalized": false
              }
            },
            "VegetationHealth": {
              "descriptor": {
                "type": "SCALAR",
                "componentType": 5121,
                "normalized": false
              }
            }
          }
        }
      ]
    }
  }
}
```