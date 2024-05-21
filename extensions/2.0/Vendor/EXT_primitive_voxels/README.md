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

This extension allows mesh primitives to represent volumetric (voxel) data via custom attributes and new primitive modes.

Voxels are often associated with cubic geometry on a box-based grid. However, with this extension, voxels may be based on other grid geometries. As such, the extension adds three new primitive modes:
- `0x80000000` (`2147483648`) - A box. The grid is based on equally-sized boxes.
- `0x80000001` (`2147483649`) - A cylinder. The grid is a stack of concentric rings, divided evenly around the circumference.
- `0x80000002` (`2147483650`) - An ellipsoid. The grid is a set of concentric ellipsoids, divided evenly in latitude and longitude.

The lowest byte is reserved for future voxel modes: `0x80000000`-`0x800000FF`.

Primitives with the `EXT_primitive_voxels` extension may still be affected by node transforms to position, orient, and scale the voxel grid as needed. The `POSITION` attribute is _not_ required or used by this extension. Positioning is derived through the node transforms in combination with the properties on the extension.

## Voxel Geometry

A voxel grid is defined in "unit" objects centered at the origin, contained in a bounding box between `(-1, -1, -1)` and `(1, 1, 1)`, as shown below.

|Box|Cylinder|Ellipsoid|
| ------------- | ------------- | ------------- |
|![Rectangular Voxel Grid](figures/box.png)|![Cylindrical Voxel Grid](figures/cylinder.png)|![Ellipsoid Voxel Grid](figures/sphere.png)|

This extension contains properties applicable to each grid shape that is used for the data. The geometry specified by the primitive's `mode` affects how the values are interpreted.

```
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
```

### Bounds

The `bounds` property describes the world-space bounds of the whole voxel primitive. The `bounds.min` and `bounds.max` apply to the voxel geometry in the following ways.

| Shape | Meaning |
| ----- | -------- |
| Box | [x, y, z] |
| Cylinder | [ radius, angle, height] |
| Ellipsoid | [longitude, latitude, height] |

#### Box

For box geometry, the `bounds` refer to the minimum and maximum corners of the voxel grid. These represent positions given in world space. The `bounds` provides a method of specifying the scale of the grid, without requiring a node `transform`.

[](TODO)

#### Cylinder

For cylinder geometry, the `bounds` refer to the slice of the cylinder that voxel data occupies. For instance, between `(0, -1, -pi)` and `(1, 1, pi)`
- Ellipsoids: a surface patch with height, between `(-pi, -pi/2, 0)` and `(pi, pi/2, 1)`

### Dimensions

The `dimensions` property refers to the number of subdivisions in the volume specified by `bounds`. Each value must be nonzero. Elements are laid out in memory on a first-axis-contiguous basis. For instance, with box-shaped voxels, the `x` data is contiguous (up to stride).

###

The `padding` property specifies how many rows of attribute data in each dimension come from neighboring grids. This is useful in situations where the primitive represents a single tile in a larger grid, and data from neighboring tiles is needed for non-local effects e.g. trilinear interpolation, blurring, antialiasing. `padding.before` and `padding.after` specify the number of rows before and after the grid in each dimension, e.g. a `padding.before` of 1 and a `padding.after` of 2 in the `y` dimension mean that each series of values in a given `y`-slice is preceded by one value and followed by two.

The padding data must be supplied with the rest of the voxel data - this means if `dimensions` is `[d1, d2, d3]`, `padding.before` is `[b1, b2, b3]`, and `padding.after` is `[a1, a2, a3]`, the attribute must supply `(d1 + a1 + b1)*(d2 + a2 + b2)*(d3 + a3 + b3)` elements.

### Example


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


## Optional vs. Required
This extension is required, meaning it should be placed in both the `extensionsUsed` list and `extensionsRequired` list.
