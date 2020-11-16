# MAXAR\_state

## Status

Draft

## Dependencies

Written against the glTF 2.0 spec.

## Overview

This extension adds linkage between multiple representations of a glTF node.  Typical uses include relating healthy, damaged and destroyed versions of a building within a larger tile.  The intent is to prefer using conventionalized state names which reference explicit node names, rather than relying on consumers to transform a node name and seek out possible alternatives.

```json
"nodes": [
    {
        "extensions": {
            "MAXAR_state": {
                "damaged": "9911ec75-9b2c-4efd-8d3b-d7dcd5f33948",
                "destroyed": "24c3cfe7-6247-4032-b297-200b577db43e"
            }
        },
        "name": "b1a1282d-14f3-4136-82fe-eebfe73978e9",
        // etc...
    }
]
```

## Open Issues

The alternate nodes could reference orphans in the same scene, or the alternate nodes could be placed in an otherwise unused scene within the model.  Whether orphans or secondary scene attachment is preferred is not yet known.  The `MSFT_lod` extension uses orphan nodes for lower levels of detail.  The rationale for attaching the nodes to an otherwise unused scene is to avoid validation issue reports, but that is only a minor issue.

Also, the use of node names or numeric identifiers is not finalized.  Names are stable, while numeric identifiers are at risk to be invalidated if modified by a tool without an understanding of this extension.  The downside of names is that they can be ambiguous (I do not believe the glTF spec requires unique names.)

Finally, it is possible these relationships could be encoded using the Feature Metadata extension instead.  This means models with states could be completely optimized and not kept as separate nodes in the hierarchy merely for the purpose of identification and switching.

## Optional vs. Required

Generally, this extension is considered optional, meaning it should be placed in the glTF root's `extensionsUsed` list, but not in the `extensionsRequired` list.  It is expected that models will still be valid when only considering the default representation.

## See Also

- The [AGI_articulations](https://github.com/KhronosGroup/glTF/master/extensions/2.0/Vendor/AGI_articulations) extension
- The [MSFT_lod](https://github.com/KhronosGroup/glTF/tree/master/extensions/2.0/Vendor/MSFT_lod) extension

## glTF Schema Updates

- **glTF node extension JSON schema**: [glTF.MAXAR_state.schema.json](schema/glTF.MAXAR_state.schema.json)

## Known Implementations
