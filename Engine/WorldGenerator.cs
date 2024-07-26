using System;
using Godot;

public partial class WorldGenerator
{
	private static FastNoiseLite _noise;
	public static int seed = 7842923;
	public static int spacing = 5; // Spacing between recorded values of the heightmap

	static WorldGenerator() {
		_noise = new FastNoiseLite() {
			Seed = seed,
			NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin,
		};
	}

	// public static int[] GenerateHeightmap(int seed, int left, int right)
	// {
	// 	// Relatively Flat Heightmap
	// 	_noise.Seed = seed;

	// 	int distance = right - left;
	// 	int[] heightmap = new int[distance];

	// 	for (int i = 0; i < distance; i++) {
	// 		heightmap[i] = (int)(100 * _noise.GetNoise1D(i + left));
	// 		GD.Print(heightmap[i]);
	// 	}

	// 	return heightmap;
	// }

	// public static void GenerateGround(World world, int left, int[] heightmap, int seed)
	// {
	// 	// start with heightmap with variation
	// 	// Then fine-grained variation (1-2 pixels) for grass stalks
	// 	// Break it up with rocks (4x3 ish?)
		
	// 	// for each height
	// 	for (int i = 0; i < heightmap.Length - 1; i++) {
	// 		// fill in the spaces
	// 		for (int j = 0; j < spacing; j++) {
	// 			// TODO: Could use a better interpolation method, decide later
	// 			float height = Mathf.Lerp(heightmap[i], heightmap[i + 1], j / spacing);


	// 		}
	// 	}
	// }

	private static float GetSurfaceHeight(float pos) {
		return _noise.GetNoise1D(pos);
	}

	private static float[] GetSurfaceLevels(int chunkPos, int granularity)
	{
		// granularity: How many subdivisions per chunk
		float[] surface = new float[granularity + 4];

		surface[0] = GetSurfaceHeight(chunkPos - 1f / granularity);

		for (int i = 0; i < granularity; i++) {
			surface[i + 1] = GetSurfaceHeight(chunkPos + i / (float)granularity);
		}

		surface[granularity + 1] = GetSurfaceHeight(1 + chunkPos);
		surface[granularity + 2] = GetSurfaceHeight(1 + chunkPos + 1f / granularity);
		surface[granularity + 3] = GetSurfaceHeight(1 + chunkPos + 2f / granularity);

		return surface;
		// return [
		// 	(_noise.GetNoise1D(chunkPos - 1)),
		// 	(_noise.GetNoise1D(chunkPos    )),
		// 	(_noise.GetNoise1D(chunkPos + 1)),
		// 	(_noise.GetNoise1D(chunkPos + 2)),
		// ];
	}

	public static Chunk GenerateChunk(Vector2I position)
	{
		// First, find out what the ground level is (in terms of chunks), this should be pregenerated within the save file
		// Then, determine how many chunks beneath that this is. Find out the level of detail and what biome this is
		// If it's not one of the top chunks, then make it loose dirt or stone
		// If it is one of the top chunks, then make it grass with some pebbles, maybe trees?
		// If it's above one of the top chunks, then just make it a bunch of air.

		Chunk chunk = new Chunk() {
			Position = Chunk.size * position
		};

		int granularity = 4;
		float[] surfaceLevel = GetSurfaceLevels(position.X, granularity);
		
		for (int i = 0; i < Chunk.size; i++) {
			float weight = (float)granularity * i / Chunk.size;
			int subdivision = Mathf.FloorToInt(weight);
			// float surface = 1000 * Mathf.CubicInterpolate(surfaceLevel[subdivision + 1], surfaceLevel[subdivision + 2],
			// 											  surfaceLevel[subdivision    ], surfaceLevel[subdivision + 3],
			// 											  weight - subdivision);

			// Lerping is basically the exact same as the cubic
			float surface = 1000 * Mathf.Lerp(surfaceLevel[subdivision + 1], surfaceLevel[subdivision + 2], weight - subdivision);

			for (int j = 0; j < Chunk.size; j++) {
				if (j + Chunk.size * position.Y > surface) {
					chunk.SetPixelLocal(new Vector2I(i, j), new Pixel(Colors.LawnGreen));
				}
				else {
					chunk.SetPixelLocal(new Vector2I(i, j), new Pixel(Colors.SkyBlue));
				}
			}
		}
		
		return chunk;
	}
}
