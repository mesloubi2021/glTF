# KHR\_materials\_thinfilm

## Khronos 3D Formats Working Group

* Norbert Nopper, UX3D [@UX3DGpuSoftware](https://twitter.com/UX3DGpuSoftware)
* Ben Houston, [Threekit](https://threekit.com)  [@BenHouston3D](https://twitter.com/BenHouston3D)

## Acknowledgments

* TODO

## Status

Experimental

## Dependencies

Written against the glTF 2.0 spec.

## Overview

TODO  

## Extending Materials

The thin film materials are defined by adding the `KHR_materials_thinfilm` extension to any glTF material. 

```json
{
    "materials": [
        {
            "extensions": {
                "KHR_materials_thinfilm": {
                    "thinfilmFactor": 1.0
                }
            }
        }
    ]
}
```

### Thinfilm

All implementations should use the same calculations for the BRDF inputs. Implementations of the BRDF itself can vary based on device performance and resource constraints. See [appendix](/specification/2.0/README.md#appendix-b-brdf-implementation) for more details on the BRDF calculations.

|                             | Type                                                                | Description                      | Required              |
|-----------------------------|---------------------------------------------------------------------|----------------------------------|-----------------------|
|**thinfilmIOR**           | `number`                                                            | The thin film index of refraction.         | No, default: `1.5`    |
|**thinfilmIORTexture**          | [`textureInfo`](/specification/2.0/README.md#reference-textureInfo) | The thin film index of refraction modulation texture. | No                    |
|**thinfilmThickness** | `number`                                                            | The thin film maximum thickness in nanometers. | No, default: `400.0`  |
|**thinfilmThicknessTexture** | [`textureInfo`](/specification/2.0/README.md#reference-textureInfo) | The thin film thickness modulation texture. | No                    |

```
// implementation specific min, max for the precalculated iridescence lookup table per 
// https://hal.archives-ouvertes.fr/hal-01518344/document
#define THINFILM_MIN_THICKNESS 400
#define THINFILM_MAX_THICKNESS 1200

float thinfilmThicknessNormalized = clamp( ( thinfilmTexture.g * thinfilmThickness ) - THINFILM_MIN_THICKNESS ) /
  ( THINFILM_MAX_THICKNESS - THINFILM_MIN_THICKNESS ), 0, 1.0 );
float thinfilmIntensity = thinfilmTexture.r * iorToF0( thinfilmIOR );
```

The thin film effect is overwriting the F (Surface Reflection Ratio) from the specular glossiness effect.
```
vec3 thinfilmSpecularReflection(vec3 F0, float VdotH, float thinfilmIntensity, float thinfilmThicknessNormalized )
{
    vec3 F = fresnelReflection(F0, VdotH);

    if (thinfilmIntensity == 0.0)
    {
        return F;
    }

    vec3 lutSample = texture(lutTexture, thinfilmThicknessNormalized, VdotH)).rgb - 0.5;

	vec3 intensity = thinfilmIntensity * 4.0 * F0 * (1.0 - F0);
	
	return clamp(lutSample * intensity + F, 0.0, 1.0);    
}
```


## Appendix

TODO

## Reference

### Theory, Documentation and Implementations

[A Practical Extension to Microfacet Theory for the Modeling of Varying Iridescence](https://belcour.github.io/blog/research/2017/05/01/brdf-thin-film.html)  
[Practical Multilayered Materials - Call of Duty - page 62](https://blog.selfshadow.com/publications/s2017-shading-course/drobot/s2017_pbs_multilayered.pdf)  