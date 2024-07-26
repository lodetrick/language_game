using System;
using Godot;

public partial class WorldGenerator
{
	private static FastNoiseLite _noise;
	private static RandomNumberGenerator _random;
	public static int seed = 7842923;
	public static int spacing = 5; // Spacing between recorded values of the heightmap
	public static int granularity = 4;

	static WorldGenerator() {
		_noise = new FastNoiseLite() {
			Seed = seed,
			NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin,
		};
		_random = new RandomNumberGenerator();
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
		float[] surface = new float[granularity + 1];

		for (int i = 0; i < granularity; i++) {
			surface[i] = GetSurfaceHeight(chunkPos + i / (float)granularity);
		}

		surface[granularity] = GetSurfaceHeight(1 + chunkPos);

		return surface;
	}

	private static float RandomCentered(float range)
	{
		return _random.RandfRange(-range,range);
	}

	public static Chunk GenerateChunk(Vector2I position)
	{
		// First, find out what the ground level is (in terms of chunks), this should be pregenerated within the save file
		// Then, determine how many chunks beneath that this is. Find out the level of detail and what biome this is
		// If it's not one of the top chunks, then make it loose dirt or stone
		// If it is one of the top chunks, then make it grass with some pebbles, maybe trees?
		// If it's above one of the top chunks, then just make it a bunch of air.

		// If there were no changes made to the chunk, I don't want to store it

		Chunk chunk = new Chunk() {
			Position = Chunk.size * position
		};

		float[] surfaceLevel = GetSurfaceLevels(position.X, granularity);
		_random.Seed = (ulong)(seed + position.X * 223 + position.Y);
		
		for (int i = 0; i < Chunk.size; i++) {
			float weight = (float)granularity * i / Chunk.size;
			int subdivision = Mathf.FloorToInt(weight);
			float surface = 1000 * Mathf.Lerp(surfaceLevel[subdivision], surfaceLevel[subdivision + 1], weight - subdivision);

			int grassHeight = _random.RandiRange(3,4);
			int grassPosition = _random.RandiRange(0,1);
			Color grassColor = new Color(RandomCentered(0.08f),RandomCentered(0.08f),RandomCentered(0.08f));

			for (int j = 0; j < Chunk.size; j++) {
				float distance = j + Chunk.size * position.Y - surface;
				float randomValue = _random.RandfRange(0,1);

				if ((distance - 25) > 10 * randomValue) {
					chunk.SetPixelLocal(new Vector2I(i, j), new Pixel(Colors.SlateGray));
				}
				else if (distance > grassPosition + grassHeight) {
					chunk.SetPixelLocal(new Vector2I(i, j), new Pixel(Colors.SaddleBrown));
				}
				else if (distance > grassPosition) {
					chunk.SetPixelLocal(new Vector2I(i, j), new Pixel(Colors.ForestGreen + grassColor));
				}
			}
		}

		if (!chunk._imageUpdated) {
			// Chunk is only air, don't want to add it
			return null;
		}
		
		return chunk;
	}
}
