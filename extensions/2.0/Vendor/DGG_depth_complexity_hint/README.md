# DGG_depth_complexity_hint

...


## Contributors

- ...


## Status

DGG vendor extension, currently supported in ...


## Dependencies

Written against the glTF 2.0 spec.


## Motivation

Rendering transparent objects in real-time rendering engines is a non-trivial task.
Sorting drawable objects back-to-front during rendering is a common method ("painter's algorithm") to achieve correct blending of transparent and opaque objects in the scene.
However, this technique does not work in all cases. If different objects intersect, results are potentially incorrect.
Also, incorrect results might occurr if a single object has multiple layers of transparent surfaces, leading to visual self-intersections - or, as one could call it, if such object has in itself a high *depth complexity*.

There are various solution to this problem, all of which are non-trivial to implement and increasing the rendering workload.
A simple solution is to sort all triangles front-to-back before rendering each transparent object, taking into account the current camera perspective during each frame.
This works especially for small scenes and moderate triangle count, but might result in bad rendering performance for other cases.
Techniques such as [depth peeling](https://developer.download.nvidia.com/assets/gamedev/docs/OrderIndependentTransparency.pdf) are providing a potentially more scalable solution by rendering multiple passes that combine different layers of transparency within the entire scene.
Still, depth any renderer implementing peeling / Order-Independent Transparency (OIT) must know in advance about the maximum number of transparent layers (the maximum depth complexity) of the current view, which is generally not trivial to efficiently compute on-the-fly and therefore usually estimated up-front with a fixed value for the number of passes.

The motivation behind this extension is to improve both of the mentioned aspects, when rendering scenes that potentially have transparent parts:

1. Ensure best possible runtime performance and
2. reduce runtime complexity.


The following images show a visual illustration of the issue and its solution (data courtesy of adidas AG):

*Rendered example (BablyonJS) with default rendering:*
![example without OIT](no-depth-pre-pass.png "Rendered example (BablyonJS) with default rendering.")

*Rendered example (BablyonJS) with OIT enabled, after interpreting DGG_depth_complexity_hint:*
![example with OIT](depth-pre-pass.png "Rendered example (BablyonJS) with OIT enabled, after interpreting DGG_depth_complexity_hint.")


## Overview

This extensions provides, for each mesh in the glTF scene, an optional hint on its depth complexity.
This allows a real-time renderer
* to quickly understand if a scene has transparent objects that need advanced rendering techniques to be correctly displayed, as well as
* to faster and more precisely estimate, or compute, the maximum number of transparent layers occurring in a certain view.

Strictly speaking, this extension is not necessary - a renderer could use advanced techniques such as OIT with a fixed, high number of passes to solve most typical real-world cases.
However, this also means the renderer would potentially do some extra work, using an unnecessarily complex shader, for scenes that don't have intersecting transparent objects, or only ones that can be rendered with simple object-level sorting.

By providing the renderer with a hint on the depth complexity of each `mesh` in the glTF scene, this extension facilitates the correct implementation of OIT for scenes that need it, with minimal overhead, and allows renderers to avoid such special rendering techniques for scenes that don't need them.

...


### Depth Complexity Hint
If this extension is present in a gltF `scene`, `DGG_depth_complexity_hint` must be present for at least
* each `mesh` that is used with at least one transparent `material`,
* including untextured transparent materials as well as those with (semi-)transparent RGBA textures.

For each mesh where `DGG_depth_complexity_hint` is present, `depth_complexity` specifies the maximum number of layers that can be observed when rendering the object from any possible angle.

If `DGG_depth_complexity_hint` is present as an extension, but not used on any mesh in the scene, this tells the renderer that there are no meshes with transparent materials (and hence specific rendering techniques related to transparency are not needed).


### Animations
For animated meshes that use skinning or morph targets, `depth_complexity` describes the maximum depth complexity (i.e., the "worst case") that may occur at any point during the animation.


### Handling of Opaque Meshes
Writing applications are free to provide `DGG_depth_complexity_hint` also for opaque meshes in the scene, but are not obliged to do so.
This allows writing applications to maintain high performance during the encoding process, limit the computation of depth complexity to the meshes which need it to obtain correct results during rendering.



## Acknowledgements
...


