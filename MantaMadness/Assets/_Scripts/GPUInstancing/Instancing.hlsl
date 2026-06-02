#ifndef GRASS_INSTANCED_INCLUDED
#define GRASS_INSTANCED_INCLUDED

struct InstanceData
{
    float4x4 m;
};

StructuredBuffer<InstanceData> _PerInstanceData;

void InstancingBillboard_float(
    float3 Position,
    float InstanceID,
    float3 CameraPositionObject,
    out float3 OutPosition,
    out float3 OutNormal
)
{
    InstanceData data = _PerInstanceData[InstanceID];

    // Position de l'instance
    float3 instancePos = mul(data.m, float4(0, 0, 0, 1)).xyz;

    // Scale de l'instance
    float scaleX = length(mul(data.m, float4(1, 0, 0, 0)).xyz);
    float scaleY = length(mul(data.m, float4(0, 1, 0, 0)).xyz);
    float scaleZ = length(mul(data.m, float4(0, 0, 1, 0)).xyz);

    // Direction vers caméra, en Object Space
    float3 forward = CameraPositionObject - instancePos;
    forward.y = 0;
    forward = normalize(forward);

    float3 up = float3(0, 1, 0);
    float3 right = normalize(cross(up, forward));

    float3 localPos =
        right * Position.x * scaleX +
        up * Position.y * scaleY +
        forward * Position.z * scaleZ;

    OutPosition = instancePos + localPos;
    OutNormal = forward;
}

#endif