#ifndef NOISE_OS_INCLUDED
#define NOISE_OS_INCLUDED

float map(float low1, float high1, float low2, float high2, float val)
{
	return low2 + (val - low1) * (high2 - low2) / (high1 - low1);
}


float3 modulo(float3 divident, float3 divisor)
{
	float3 positiveDivident = divident % divisor + divisor;
	return positiveDivident % divisor;
}

float easeIn(float t)
{
	return t * t;
}

float easeOut(float t)
{
	return 1.0 - easeIn(1.0 - t);
}

float easeInOut(float t)
{
	float a = easeIn(t);
	float b = easeOut(t);
	return lerp(a, b, t);
}

float rand3dTo1d(float3 vec, float3 dotDir = float3(12.9898, 78.233, 37.719))
{
    float random = dot(sin(vec), dotDir);
    random = frac(sin(random) * 143758.5453);
    return random;
}

float3 rand3dTo3d(float3 vec, float3 seed = 4605)
{
    return float3
    (
        rand3dTo1d(vec + seed),
        rand3dTo1d(vec + seed, float3(39.346, 11.135, 83.155)),
        rand3dTo1d(vec + seed, float3(73.156, 52.235, 9.151))
    );
}

float GradientNoise3dTiling(float3 vec, float3 period, float seed = 4605)
{
	float3 f = frac(vec);
	float3 t = float3(easeInOut(f.x), easeInOut(f.y), easeInOut(f.z));
	
	float3 cellsMinimum = floor(vec);
	float3 cellsMaximum = ceil(vec);
	cellsMinimum = modulo(cellsMinimum, period);
	cellsMaximum = modulo(cellsMaximum, period);
	
	float cellNoiseZ[2];
	for (int z = 0; z <= 1; z++)
	{
		float cellNoiseY[2];
		for (int y = 0; y <= 1; y++)
		{
			float cellNoiseX[2];
			for (int x = 0; x <= 1; x++)
			{
				float3 cell = floor(vec) + float3(x, y, z);
				float3 tiledCell = modulo(cell, period);
				float3 dir = rand3dTo3d(tiledCell, seed) * 2 - 1;
				float3 comparator = f - float3(x, y, z);
				cellNoiseX[x] = dot(dir, comparator);
			}
			cellNoiseY[y] = lerp(cellNoiseX[0], cellNoiseX[1], t.x);
		}
		cellNoiseZ[z] = lerp(cellNoiseY[0], cellNoiseY[1], t.y);
	}
	float noise = lerp(cellNoiseZ[0], cellNoiseZ[1], t.z);
	return noise;
}


float3 WorleyNoise3dTiling(float3 vec, float3 period, float seed = 4605)
{
	float3 d[3][3][3];
	float3 c[3][3][3];
	float3 baseCell = floor(vec);
	float minDistToCell = 10;
	
	float3 dirToClosestCell;
	float3 closestCell;
	for (int x1 = 0; x1 <= 2; x1++)
	{
		for (int y1 = 0; y1 <= 2; y1++)
		{
			for (int z1 = 0; z1 <= 2; z1++)
			{
				float3 coord = float3(x1 - 1, y1 - 1, z1 - 1);
				float3 cell = baseCell + coord;
				float3 tiledCell = modulo(cell, period);
				c[x1][y1][z1] = cell;
				float3 cellPosition = cell + rand3dTo3d(tiledCell, seed);
				float3 dirToCell = cellPosition - vec;
				d[x1][y1][z1] = dirToCell;
				
				float distToCell = length(dirToCell);
				if (distToCell < minDistToCell)
				{
					minDistToCell = distToCell;
					closestCell = cell;
					dirToClosestCell = dirToCell;
				}
			}
		}
	}
	
	float minEdgeDistance = 10;
	for (int x2 = 0; x2 <= 2; x2++)
	{
		for (int y2 = 0; y2 <= 2; y2++)
		{
			for (int z2 = 0; z2 <= 2; z2++)
			{
				float3 cell = c[x2][y2][z2];
				float3 dirToCell = d[x2][y2][z2];
				float3 diffToClosestCell = abs(closestCell - cell);
				
				bool isClosestCell = diffToClosestCell.x + diffToClosestCell.y + diffToClosestCell.z < 0.1;
				if (!isClosestCell)
				{
					float3 dirToCenter = (dirToClosestCell + dirToCell) * 0.5;
					float3 cellDiff = normalize(dirToCell - dirToClosestCell);
					float edgeDistance = dot(dirToCenter, cellDiff);
					minEdgeDistance = min(minEdgeDistance, edgeDistance);

				}
			}
		}

	}
	
	float random = rand3dTo1d(closestCell);
	return float3(minDistToCell, random, minEdgeDistance);
}
float SampleLayeredGradientNoise3dTiling(float3 vec, int octaves, float lacunarity, float gain, float3 period, float seed = 4605)
{
	float noise = 0;
	float frequency = 1;
	float amplitude = 1;
	
	for (int i = 1; i <= octaves; i++)
	{
		noise += GradientNoise3dTiling(vec * frequency + i * 0.12703, period * frequency, seed) * amplitude;
		frequency *= lacunarity;
		amplitude *= gain;
	}
	return noise;
}

float SampleLayeredWorleyNoise3dTiling(float3 vec, int octaves, float lacunarity, float gain, float3 period, float seed = 4605)
{
	float noise = 0;
	float frequency = 1;
	float amplitude = 1;
	
	for (int i = 1; i <= octaves; i++)
	{
		noise += (WorleyNoise3dTiling(vec * frequency + i * 0.12703, period * frequency, seed).x - 0.5) * amplitude;
		frequency *= lacunarity;
		amplitude *= gain;
	}
	
	
	return noise;
}


float3 GradientNoise3dCurlTiling(float3 vec, float3 period, int seed = 4605)
{
	float epsilon = 0.0001;
	float3 curl;
	
	float n1, n2, a, b;
	
	// Find curl.x
	n1 = GradientNoise3dTiling(float3(vec.x, vec.y + epsilon, vec.z), period, seed);
	n2 = GradientNoise3dTiling(float3(vec.x, vec.y - epsilon, vec.z), period, seed);
	
	a = (n1 - n2) / (2.0 * epsilon);
	
	n1 = GradientNoise3dTiling(float3(vec.x, vec.y, vec.z + epsilon), period, seed);
	n2 = GradientNoise3dTiling(float3(vec.x, vec.y, vec.z - epsilon), period, seed);
	
	b = (n1 - n2) / (2.0 * epsilon);
	curl.x = a - b;
	
	// Find curl.y
	n1 = GradientNoise3dTiling(float3(vec.x, vec.y, vec.z + epsilon), period, seed);
	n2 = GradientNoise3dTiling(float3(vec.x, vec.y, vec.z - epsilon), period, seed);
	a = (n1 - n2) / (2.0 * epsilon);
	
	n1 = GradientNoise3dTiling(float3(vec.x + epsilon, vec.y, vec.z), period, seed);
	n2 = GradientNoise3dTiling(float3(vec.x - epsilon, vec.y, vec.z), period, seed);
	b = (n1 - n2) / (2.0 * epsilon);
	curl.y = a - b;
	
	// Find curl.z
	n1 = GradientNoise3dTiling(float3(vec.x + epsilon, vec.y, vec.z), period, seed);
	n2 = GradientNoise3dTiling(float3(vec.x - epsilon, vec.y, vec.z), period, seed);
	a = (n1 - n2) / (2.0 * epsilon);
	
	
	n1 = GradientNoise3dTiling(float3(vec.x, vec.y + epsilon, vec.z), period, seed);
	n2 = GradientNoise3dTiling(float3(vec.x, vec.y - epsilon, vec.z), period, seed);
	b = (n1 - n2) / (2.0 * epsilon);
	curl.z = a - b;
	
	return curl;
}


float3 SampleLayered3dCurlNoiseTiling(float3 vec, float octaves, float lacunarity, float gain, float3 period, int seed)
{
	float3 noise = float3(0, 0, 0);
	float frequency = 1.0;
	float amplitude = 1.0;
	
	for (int i = 1; i <= octaves; i++)
	{
		noise += GradientNoise3dCurlTiling(vec * frequency + i * 0.12703, period * frequency, seed + i) * amplitude;
		frequency *= lacunarity;
		amplitude *= gain;
	}
	 
	return noise;
}
#endif