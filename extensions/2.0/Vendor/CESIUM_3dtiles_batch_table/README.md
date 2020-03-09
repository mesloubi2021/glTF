# CESIUM\_3dtiles\_batch\_table

## Contributors

TODO

## Status

TODO

* Change name?
  * CESIUM_3dtiles_batched_features
  * CESIUM_3dtiles_batched_models
* Rename batch to feature everywhere?
* Note the batch table hierarchy extension
* Update batch points diagram
* `values` and `accessor` are mutually exclusive
* Fill in other TODO's

## Dependencies

Written against the glTF 2.0 spec.

## Overview

TODO

## Summary

This extension allows offline batching of heterogeneous 3D models, such as different buildings in a city, for efficient streaming to a client for rendering and interaction. Efficiency comes from transferring multiple models in a single request and rendering them in the least number of draw calls necessary. Using the core 3D Tiles spec language, each model is a feature.

Per-feature IDs enable individual features to be identified and updated at runtime, e.g., show/hide, highlight color, etc. IDs may be used to query application-specific properties for styling and any application-specific use cases such as populating a UI or issuing a REST API request.

A _Batch Table_ contains per-feature application-specific properties. Some example batch table properties are building heights, geographic coordinates, and database primary keys. A _batch id_ vertex attribute is used to identify the vertices belonging to a feature, and that feature's properties may be retrieved from the batch table.

![Batch Table Diagram](./figures/batch-table-buildings.png)

Multiple batch ids and batch tables are allowed to support feature layers. For example, in point cloud models it may be useful to store both per-point properties and per-group properties - in the first layer each point is considered a feature; in the second layer each group of points is considered a feature.

![Batched Points](./figures/batched-points.png)

### Batch Id

This extensions adds a new indexed attribute semantic `_BATCHID_0`. All indices must start with 0 and be continuous positive integers: `_BATCHID_0`, `_BATCHID_1`, `_BATCHID_2`, etc. Each attribute represents a different feature layer and its corresponding batch table.

The mapping between batch id attributes and their batch tables is defined in the primitive's `CESIUM_3dtiles_batch_table` extension object where the key is the attribute name and the value is the index into the `batchTables` array in the top-level `CESIUM_3dtiles_batch_table` extension. In the example below `_BATCHID_0` corresponds to the first batch table and `_BATCHID_1` corresponds the third batch table.

```json
"primitives": [
  {
    "attributes": {
      "POSITION": 0,
      "_BATCHID_0": 1,
      "_BATCHID_1": 2
    },
    "indices": 3,
    "material": 0,
    "mode": 4,
    "extensions": {
      "CESIUM_3dtiles_batch_table": {
        "attributes": {
          "_BATCHID_0": 0,
          "_BATCHID_1": 2
        }
      }
    }
  }
]
```

`_BATCHID_0` is a required attribute for all primitives in the glTF; additional batch id attributes are optional. Clients are required to support at least `_BATCHID_0`.

Valid accessor type and component type for the `_BATCHID_0` attribute semantic property are defined below.

|Name|Accessor Type(s)|Component Type(s)|Description|
|----|----------------|-----------------|-----------|
|`_BATCHID_0`|`"SCALAR"`|`5121`&nbsp;(UNSIGNED_BYTE)<br>`5123`&nbsp;(UNSIGNED_SHORT)<br>`5125`&nbsp;(UNSIGNED_INT)|Feature id within a feature layer |

Note that to comply with alignment rules for accessors, accessors need to be aligned to 4-byte boundaries; for example, an `UNSIGNED_BYTE` batch id is expected to have a stride of 4, not 1.

#### Batch Id Accessor Requirements

Each component in the batch id accessor must be in the range `[0, batchLength - 1]` inclusive, where `batchLength` is the number of features in the feature layer.

In certain cases, batch id components can be implicity defined. When `bufferView` is not defined:

* If `accessor.count` equals `batchLength`, the accessor must be initialized with consecutive integers starting at 0 and ending at `batchLength - 1`, e.g. `[0, 1, 2, 3, ..., batchLength - 1]`. This can provide memory savings when each point in a point cloud is its own feature and batch ids do not need to be explicity defined.
* Otherwise if `accessor.count` does not equal `batchLength` the accessor must be initialized with zeros, in accordance with the glTF accessor schema. This can provide memory savings if the model consists of single feature.

### Batch Table

A batch table contains per-feature application-specific properties. A batch table may contain any number of properties, or no properties at all. The number of features is specified in the `batchLength` property. `batchLength` must be greater than or equal to 1.

Batch Table properties can be represented in two different ways:

1. An array of values
    * Array elements can be any valid JSON data type, including objects and arrays.  Elements may be `null`.
    * The length of each array must be equal to `batchLength`.
2. A reference to binary data via an accessor.
    * The accessor's count must be equal to `batchLength`.
    
It is more efficient to store long numeric arrays in the binary body.

#### Example

Batch table containing a mix of JSON and binary properties for two buildings (features)

```json
{
  "batchLength": 2,
  "properties": {
    "name": {
      "values": [
        "Building name",
        "Another building name"
      ]
    },
    "yearBuilt": {
      "values": [
        1999,
        2015
      ]
    },
    "address": {
      "values": [
        {
          "street": "Main Street",
          "houseNumber": "1"
        },
        {
          "street": "Main Street",
          "houseNumber": "2"
        }
      ]
    },
    "geographicCoordinates": {
      "accessor": 0
    },
    "zone": {
      "accessor": 1
    }
  }
}
```

#### Batch Table Accessors Requirements

For each batch table property's accessor, `accessor.count` must equal `batchLength`.

## Optional vs. Required

This extension is optional, meaning it should be placed in the glTF root's `extensionsUsed` list, but not in the `extensionsRequired` list.

## See Also

TODO

## glTF Schema Updates

TODO

## Known Implementations

TODO

## Examples

### Batched models with JSON properties

```json
{
  "accessors": [
    {
      "name": "positions (float)",
      "bufferView": 0,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 8,
      "type": "VEC3"
    },
    {
      "name": "batch ids (unsigned byte)",
      "bufferView": 1,
      "byteOffset": 0,
      "componentType": 5121,
      "count": 8,
      "type": "SCALAR"
    },
    {
      "name": "indices (unsigned short)",
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
          "name": "two buildings composed of two triangles each",
          "attributes": {
            "POSITION": 0,
            "_BATCHID_0": 1
          },
          "indices": 2,
          "material": 0,
          "mode": 4,
          "extensions": {
            "CESIUM_3dtiles_batch_table": {
              "attributes": {
                "_BATCHID_0": 0
              }
            }
          }
        }
      ]
    }
  ],
  "extensionsUsed": [
    "CESIUM_3dtiles_batch_table"
  ],
  "extensions": {
    "CESIUM_3dtiles_batch_table": {
      "batchTables": [
        {
          "batchLength": 2,
          "properties": {
            "name": {
              "values": [
                "Building name",
                "Another building name"
              ]
            },
            "yearBuilt": {
              "values": [
                1999,
                2015
              ]
            },
            "address": {
              "values": [
                {
                  "street": "Main Street",
                  "houseNumber": "1"
                },
                {
                  "street": "Main Street",
                  "houseNumber": "2"
                }
              ]
            }
          }
        }
      ]
    }
  }
}
```

### Batched models with binary properties

```json
{
  "accessors": [
    {
      "name": "positions (float)",
      "bufferView": 0,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 8,
      "type": "VEC3"
    },
    {
      "name": "batch ids (unsigned byte)",
      "bufferView": 1,
      "byteOffset": 0,
      "componentType": 5121,
      "count": 8,
      "type": "SCALAR"
    },
    {
      "name": "indices (unsigned short)",
      "bufferView": 2,
      "byteOffset": 0,
      "componentType": 5123,
      "count": 12,
      "type": "SCALAR"
    },
    {
      "name": "id property (unsigned int)",
      "bufferView": 3,
      "byteOffset": 0,
      "componentType": 5125,
      "count": 2,
      "type": "SCALAR"
    },
    {
      "name": "cartographic property (vec3 float)",
      "bufferView": 4,
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
          "name": "two buildings composed of two triangles each",
          "attributes": {
            "POSITION": 0,
            "_BATCHID_0": 1
          },
          "indices": 2,
          "material": 0,
          "mode": 4,
          "extensions": {
            "CESIUM_3dtiles_batch_table": {
              "attributes": {
                "_BATCHID_0": 0
              }
            }
          }
        }
      ]
    }
  ],
  "extensionsUsed": [
    "CESIUM_3dtiles_batch_table"
  ],
  "extensions": {
    "CESIUM_3dtiles_batch_table": {
      "batchTables": [
        {
          "batchLength": 2,
          "properties": {
            "id": {
              "accessor": 3
            },
            "cartographic": {
              "accessor": 4
            }
          }
        }
      ]
    }
  }
}
```

### Point cloud with per-point properties and per-group properties;

```json
{
  "accessors": [
    {
      "name": "positions (float)",
      "bufferView": 0,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 8,
      "type": "VEC3"
    },
    {
      "name": "colors (unsigned byte)",
      "bufferView": 1,
      "byteOffset": 0,
      "componentType": 5121,
      "count": 8,
      "type": "VEC3"
    },
    {
      "name": "per-point batch ids (implicitly 0 to batchLength-1) (batch table 0) (unsigned byte)",
      "componentType": 5121,
      "count": 8,
      "type": "SCALAR"
    },
    {
      "name": "per-feature batch ids (batch table 1) (unsigned byte)",
      "bufferView": 2,
      "byteOffset": 0,
      "componentType": 5121,
      "count": 8,
      "type": "SCALAR"
    },
    {
      "name": "intensity property (unsigned short)",
      "bufferView": 3,
      "byteOffset": 0,
      "componentType": 5123,
      "count": 8,
      "type": "SCALAR"
    },
    {
      "name": "classification property (unsigned byte)",
      "bufferView": 4,
      "byteOffset": 0,
      "componentType": 5121,
      "count": 8,
      "type": "SCALAR"
    },
    {
      "name": "id property (unsigned int)",
      "bufferView": 5,
      "byteOffset": 0,
      "componentType": 5125,
      "count": 2,
      "type": "SCALAR"
    },
    {
      "name": "cartographic property (vec3 float)",
      "bufferView": 6,
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
          "name": "two buildings composed of four points each",
          "attributes": {
            "POSITION": 0,
            "COLOR_0": 1,
            "_BATCHID_0": 2,
            "_BATCHID_1": 3
          },
          "mode": 0,
          "extensions": {
            "CESIUM_3dtiles_batch_table": {
              "attributes": {
                "_BATCHID_0": 0,
                "_BATCHID_1": 1
              }
            }
          }
        }
      ]
    }
  ],
  "extensionsUsed": [
    "CESIUM_3dtiles_batch_table"
  ],
  "extensions": {
    "CESIUM_3dtiles_batch_table": {
      "batchTables": [
        {
          "batchLength": 8,
          "properties": {
            "intensity": {
              "accessor": 4
            },
            "classification": {
              "accessor": 5
            }
          }
        },
        {
          "batchLength": 2,
          "properties": {
            "id": {
              "accessor": 6
            },
            "cartographic": {
              "accessor": 7
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
      "name": "positions (float)",
      "bufferView": 0,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 4,
      "type": "VEC3"
    },
    {
      "name": "batch ids (implicitly zeros) (unsigned byte)",
      "componentType": 5121,
      "count": 4,
      "type": "SCALAR"
    },
    {
      "name": "indices (unsigned short)",
      "bufferView": 1,
      "byteOffset": 0,
      "componentType": 5123,
      "count": 6,
      "type": "SCALAR"
    }
  ],
  "meshes": [
    {
      "primitives": [
        {
          "name": "one building composed of two triangles",
          "attributes": {
            "POSITION": 0,
            "_BATCHID_0": 1
          },
          "indices": 2,
          "material": 0,
          "mode": 4,
          "extensions": {
            "CESIUM_3dtiles_batch_table": {
              "attributes": {
                "_BATCHID_0": 0
              }
            }
          }
        }
      ]
    }
  ],
  "extensionsUsed": [
    "CESIUM_3dtiles_batch_table"
  ],
  "extensions": {
    "CESIUM_3dtiles_batch_table": {
      "batchTables": [
        {
          "batchLength": 1,
          "properties": {
            "name": {
              "values": [
                "Building name"
              ]
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
      "name": "point positions (float)",
      "bufferView": 0,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 8,
      "type": "VEC3"
    },
    {
      "name": "point colors (unsigned byte)",
      "bufferView": 1,
      "byteOffset": 0,
      "componentType": 5121,
      "count": 8,
      "type": "VEC3"
    },
    {
      "name": "per-point batch ids (implicitly 0 to batchLength-1) (batch table 0) (unsigned byte)",
      "componentType": 5121,
      "count": 8,
      "type": "SCALAR"
    },
    {
      "name": "per-group batch ids (batch table 1) (unsigned byte)",
      "bufferView": 2,
      "byteOffset": 0,
      "componentType": 5121,
      "count": 8,
      "type": "SCALAR"
    },
    {
      "name": "intensity property (unsigned short)",
      "bufferView": 3,
      "byteOffset": 0,
      "componentType": 5123,
      "count": 8,
      "type": "SCALAR"
    },
    {
      "name": "classification property (unsigned byte)",
      "bufferView": 4,
      "byteOffset": 0,
      "componentType": 5121,
      "count": 8,
      "type": "SCALAR"
    },
    {
      "name": "id property (unsigned int)",
      "bufferView": 5,
      "byteOffset": 0,
      "componentType": 5125,
      "count": 2,
      "type": "SCALAR"
    },
    {
      "name": "cartographic property (vec3 float)",
      "bufferView": 6,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 2,
      "type": "VEC3"
    },
    {
      "name": "positions (float)",
      "bufferView": 7,
      "byteOffset": 0,
      "componentType": 5126,
      "count": 8,
      "type": "VEC3"
    },
    {
      "name": "batch ids (unsigned byte)",
      "bufferView": 8,
      "byteOffset": 0,
      "componentType": 5121,
      "count": 8,
      "type": "SCALAR"
    },
    {
      "name": "indices (unsigned short)",
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
          "name": "two buildings composed of four points each",
          "attributes": {
            "POSITION": 0,
            "COLOR_0": 1,
            "_BATCHID_0": 2,
            "_BATCHID_1": 3
          },
          "mode": 0,
          "extensions": {
            "CESIUM_3dtiles_batch_table": {
              "attributes": {
                "_BATCHID_0": 0,
                "_BATCHID_1": 1
              }
            }
          }
        },
        {
          "name": "two buildings composed of two triangles each",
          "attributes": {
            "POSITION": 8,
            "_BATCHID_0": 9
          },
          "indices": 10,
          "material": 0,
          "mode": 4,
          "extensions": {
            "CESIUM_3dtiles_batch_table": {
              "attributes": {
                "_BATCHID_0": 2
              }
            }
          }
        }
      ]
    }
  ],
  "extensionsUsed": [
    "CESIUM_3dtiles_batch_table"
  ],
  "extensions": {
    "CESIUM_3dtiles_batch_table": {
      "batchTables": [
        {
          "batchLength": 8,
          "properties": {
            "intensity": {
              "accessor": 4
            },
            "classification": {
              "accessor": 5
            }
          }
        },
        {
          "batchLength": 2,
          "properties": {
            "id": {
              "accessor": 6
            },
            "cartographic": {
              "accessor": 7
            }
          }
        },
        {
          "batchLength": 2,
          "properties": {
            "name": {
              "values": [
                "Building name",
                "Another building name"
              ]
            }
          }
        }
      ]
    }
  }
}
```