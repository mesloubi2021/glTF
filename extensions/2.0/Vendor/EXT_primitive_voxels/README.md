# EXT_primitive_voxels

## Contributors
- Daniel Krupka, Cesium
- Ian Lilley, Cesium
- Sean Lilley, Cesium

## Status
Draft

## Dependencies
Written against the glTF 2.0 specification.

## Overview

This extension allows primitives to use their attribute data to represent volumetric (voxel) data.

```
{
  "meshes": [
    {
      "primitives": [
        {
          "attributes": {
            "POSITION": 0,
            "_TEMPERATURE": 1
          },
          "mode": 2147483648,
          "extensions": {
            "EXT_primitive_voxels": {
              "dimensions": [8, 8, 8],
              "neighboringEdgeCount" 1
            }
          }
        }
      ]
    }
  ]
}
```

The extension adds three new primitive modes, corresponding to voxel grid geometries:
- `0x80000000` (`2147483648`) - A Cartesian box. The grid is a Cartesian grid of equally-sized boxes.
- `0x80000001` (`2147483649`) - A cylinder. The grid is a stack of concentric rings, evenly divided around the circumference.
- `0x80000002` (`2147483650`) - An ellipsoid. The grid is a set of concentric ellipsoids, divided evenly in latitude and longitude.

The `dimensions` property of the extension specifies the voxel grid dimensions:
- x/y/z for boxes
- r/theta/z for cylinders
- lat/lon/height for ellipsoids

Dimensions must be nonzero.

The `neighborEdgeCount` property specifies how many rows of attribute data in each dimension come from
neighboring grids. This is useful in situations where the primitive represents a single tile in a larger grid, and
data from neighboring tiles is needed for non-local effects e.g. blurring, antialiasing.

The neighbor data must be supplied with the rest of the voxel data - this means if `d1`, `d2`, `d3` are the grid dimensions and `n` is `neighborEdgeCount`, the attribute must supply `(d1 + n)*(d2 + n)*(d3 + n)` elements.

## Optional vs. Required
This extension is required, meaning it should be placed in both the `extensionsUsed` list and `extensionsRequired` list.
