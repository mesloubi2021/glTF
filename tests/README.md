# Animation Controller

## Overview

This gltf extra's spec tries to define Unity's animator in gltf format to have a more ordered way of using animations.

```javascript
{
    "animationControllers": [{
        "name": "controller",
        "nodes": [1],
        "timeScale": 1,
        "parameters": [{
                "triggerParam": {
                    "value": false
                }
            },
            { "boolParam": false },
            { "intParam": 0 },
            { "floatParam": 0 }
        ],
        "layers": [{
                "name": "base layer",
                "blendMode": "override",
                "weight": 1,
                "initialState": 1,
                "states": [{
                        "name": "animation state 1",
                        "speed": 1,
                        "clip": 0,
                        "offset": 0
                    },
                    {
                        "name": "animation state 2",
                        "speed": 1,
                        "multParam": 2,
                        "motionParam": 3,
                        "offsetParam": 3
                    }
                ],
                "transitions": [{
                        "from": 0,
                        "to": 1,
                        "conditions": [{
                            "param": 3,
                            "cond": "greater",
                            "value": 0
                        }],
                        "params": {
                            "duration": 0.25,
                            "fixedDuration": true,
                            "exitTime": 0.25,
                            "offset": 0
                        }
                    },
                    {
                        "from": -1,
                        "to": 1
                    },
                    {
                        "from": -2,
                        "to": 1,
                        "conditions": [{
                            "param": 0,
                            "cond": "if",
                            "value": true
                        }]
                    }
                ]
            },
            {
                "name": "baseLayerSync",
                "blendMode": "additive",
                "weight": 1,
                "sync": 0,
                "timing": true,
                "syncedClipStates": [-1, -1]
            }
        ]
    }]
}
```