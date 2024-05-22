# EXT_primitive_voxels

<p align="center">
  <img src="figures/voxel_cube.png">
</p>

## Contributors
- Daniel Krupka, Cesium
- Ian Lilley, Cesium
- Sean Lilley, Cesium
- Janine Liu, Cesium
- Jeshurun Hembd, Cesium

## Status
Draft

## Dependencies
Written against the glTF 2.0 specification.

## Overview

This extension allows mesh primitives to represent volumetric (voxel) data via custom attributes. Primitives that use this extension must set their `mode` to the constant `0x80000000` (`2147483648`), which is used to indicate voxels.

Typically, glTF mesh primitives use the `POSITION` attribute to store positional mesh data. However, `POSITION` is neither required nor used by this extension. Instead, `EXT_primitive_voxels` describe the positions and geometry of voxels using its own properties, and the attributes refer to what kind of data they represent. Primitives with this extension can still be affected by node transforms to position, orient, and scale the voxel grid as needed.

```
"primitives": [
  {
    "attributes": {
      "_TEMPERATURE": 0
    },
    "mode": 2147483648,
    "extensions": {
      "EXT_primitive_voxels": {
        // geometric properties here
      }
    }
  }
]
```

Although voxels are commonly associated with cubic geometry on a box-based grid, this extension allows voxels to be based on other types of grid geometry, including cylinders and ellipsoids, visualized below.

|Box|Cylinder|Ellipsoid|
| ------------- | ------------- | ------------- |
|![Box Voxel Grid](figures/box.png)|![Cylindrical Voxel Grid](figures/cylinder.png)|![Ellipsoid Voxel Grid](figures/sphere.png)|

Each of these grids is defined by the respectively named properties within the extension:
- [`box`](#box-grid)
- [`cylinder`](#cylindrical-grid)
- [`ellipsoid`](#ellipsoidal-grid)

Only **one** of these may be defined at a time. Within this geometry, voxels exist inside a bounding volume that conforms to the geometry of the grid, as described by the relevant property.

The `dimensions` property refers to the number of subdivisions _within_ this bounding volume. Each value of `dimensions` must be a positive integer.

The relationship between `dimensions` and the other aforementioned properties is explained in detail below.

### Box Grid

A **box** grid is a Cartesian grid defined by `x`, `y`, and `z` axes with equally-sized boxes. A voxel primitive that is based on a box grid may define the `box` property like so:

```
"EXT_primitive_voxels": {
  "dimensions": [8, 8, 8],
  "box": {
    "min": [0.25, 0.5, 0.5],
    "max": [0.375, 0.625, 0.625]
  }
}
```

The `min` and `max` properties refer to the minimum and maximum positions that form the corners of the grid. These positions are given in the primitive's local space. The `dimensions` correspond to the subdivisions of the box along the `x`, `y`, and `z` axes respectively.

![Uniform box grid](figures/uniform-box.png)
<p align="center"><i>A box grid spanning from -1 to 1 in all three axes, subdivided into two cells along each axis. The origin is in the center of the box.</i></p>

These properties may be used to define the scale of the box grid, independent of the `scale` applied by a node. Additionally, the `dimensions` may be non-uniform.

![Non-uniform box grid](figures/non-uniform-box.png)
<p align="center"><i>A box grid that is non-uniformly scaled due to its min and max properties. It is also non-uniformly subdivided.</i></p>

### Cylindrical Grid

A **cylinder** grid is subdivided along the radius, height, and angle ranges of the cylinder, visualized below.

![Cylinder subdivisions](figures/cylinder-subdivisions.png)

The cylinder is aligned with the `y`-axis in the primitive's local space. As such, the `height` range of the cylinder is specified in values along that local `y`-axis. Subdivisions along the `radius` are concentric, centered around the `y`-axis and extending outwards. The `angle` is defined in `pi`

A voxel primitive that is based on a cylinder grid may define the `cylinder` property like so:

```
"EXT_primitive_voxels": {
  "dimensions": [4, 4, 4],
  "cylinder": {
    "radius": [0.5, 1.25],
    "height": [-1.0, 1.0],
    "angle": [-1.57079632679, 1.57079632679] // [-pi/2, pi/2]
  }
}
```

For instance, between `(0, -1, -pi)` and `(1, 1, pi)`

| Axis | Coordinate | Positive Direction |
| ---- | ---------- | ------------------ |
| 0	| radius | From center (increasing radius) |
| 1 |	height | From bottom to top (increasing height) |
| 2	| angle	| From -pi to pi clockwise (see figure below) |

[TODO](image)

### Ellipsoidal Grid

For **ellipsoid** geometry, the `bounds` refer to the section of the ellipsoid that the voxel data occupies. `(-pi, -pi/2, 0)` and `(pi, pi/2, 1)`

 Elements are laid out in memory on a first-axis-contiguous basis. For instance, with box-shaped voxels, the `x` data is contiguous (up to stride).


contains minimum and maximum values along the axes and/or parameters that control the grid's shape. These ranges are interpreted relative to the geometry of the grid, as indicated by the primitive's `mode`.

### Padding

```
 "padding": {
    "before": [1, 1, 1],
    "after": [1, 1, 1]
  }
```

The `padding` property specifies how many rows of attribute data in each dimension come from neighboring grids. This is useful in situations where the primitive represents a single tile in a larger grid, and data from neighboring tiles is needed for non-local effects e.g. trilinear interpolation, blurring, antialiasing. `padding.before` and `padding.after` specify the number of rows before and after the grid in each dimension, e.g. a `padding.before` of 1 and a `padding.after` of 2 in the `y` dimension mean that each series of values in a given `y`-slice is preceded by one value and followed by two.

The padding data must be supplied with the rest of the voxel data - this means if `dimensions` is `[d1, d2, d3]`, `padding.before` is `[b1, b2, b3]`, and `padding.after` is `[a1, a2, a3]`, the attribute must supply `(d1 + a1 + b1)*(d2 + a2 + b2)*(d3 + a3 + b3)` elements.

### Full Example

```
{
  "meshes": [
    {
      "primitives": [
        {
          "attributes": {
            "_TEMPERATURE": 0
          },
          "mode": 2147483648,
          "extensions": {
            "EXT_primitive_voxels": {
              "dimensions": [8, 8, 8],
              "bounds": {
                "min": [0.25, 0.5, 0.5],
                "max": [0.375, 0.625, 0.625]
              },
              "padding": {
                "before": [1, 1, 1],
                "after": [1, 1, 1]
              }
            }
          }
        }
      ]
    }
  ]
}
```
### Metadata

This extension may be paired with the `EXT_structural_metadata` extension.

{
  "extensions": {
    "EXT_structural_metadata": {
    
    }
  },
  "meshes": [
    {
      "primitives": [
        {
          "attributes": {
            "_TEMPERATURE": 0
          },
          "mode": 2147483648,
          "extensions": {
            "EXT_primitive_voxels": {
              "dimensions": [8, 8, 8],
              "bounds": {
                "min": [0.25, 0.5, 0.5],
                "max": [0.375, 0.625, 0.625]
              },
              "padding": {
                "before": [1, 1, 1],
                "after": [1, 1, 1]
              }
            }
          }
        }
      ]
    }
  ]
}

## Optional vs. Required
This extension is required, meaning it should be placed in both the `extensionsUsed` list and `extensionsRequired` list.
