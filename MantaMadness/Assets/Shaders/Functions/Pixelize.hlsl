void Pixelize_float(float2 UV, float2 blockCount, float2 blockSize, float2 halfBlockSize, out float2 outUV)
{
    float2 blockPos = floor(UV * blockCount);
    outUV = blockPos * blockSize + halfBlockSize;
}