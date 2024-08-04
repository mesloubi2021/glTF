# Unity\_colliders

## Contributors

## Status

## Dependencies

Written against the glTF 2.0 spec.

## Overview

This extension allows glTF to support Unity colliders.

## Unity Colliders

Setup:
```json
"extensionsUsed": [
    "Unity_colliders"
],
"extensionsRequired": [
    "Unity_colliders"
]
```

Node Extension:
```json
"nodes": [
    {
        "mesh": 0,
        "extensions": {
            "Unity_colliders": {
                "collider": 0
            }
        }
    }
]
```

glTF Extension:
```json
"extensions": {
    "Unity_colliders": {
        "colliders": [
            {
                "boxCollider": {
                    "center": [
                        0.0,
                        0.0,
                        0.0
                    ],
                    "size": [
                        1.0,
                        1.0,
                        1.0
                    ]
                }
            },
            {
                "capsuleCollider": {
                    "center": [
                        0.0,
                        0.0,
                        0.0
                    ],
                    "radius": 0.5,
                    "height": 1.0,
                    "direction": "y"
                }
            },
            {
                "sphereCollider": {
                    "center": [
                        0.0,
                        0.0,
                        0.0
                    ],
                    "radius": 0.5
                }
            },
            {
                "meshCollider": {
                    "convex": false,
                    "mesh": 0
                }
            }
        ]
    }
}
```
