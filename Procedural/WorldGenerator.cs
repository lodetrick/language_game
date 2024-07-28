using System;
using Godot;
using Godot.Collections;

public partial class WorldGenerator
{
	private static FastNoiseLite _noise, _dispNoise;
	private static RandomNumberGenerator _random, _randomX;
	private static Curve rockFrequency;
	public static int seed = 7842923;
	public static int spacing = 5; // Spacing between recorded values of the heightmap
	public static int granularity = 4;

	static WorldGenerator() {
		_noise = new FastNoiseLite() {
			Seed = seed,
			NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin,
		};

		_dispNoise = new FastNoiseLite() {
			Seed = seed / 2,
			Frequency = 0.08f,
		};
		_random = new RandomNumberGenerator();
		_randomX = new RandomNumberGenerator();
		rockFrequency = GD.Load<Curve>("res://Procedural/RockFrequency.tres");

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


	private static void GenerateTree(Chunk chunk, Vector2I pos)
	{
		// Generate the tree starting from the location
		// Have trunk and branches and leaves in the background
		// Have roots in the foreground

		// Maybe beforehand know how tall and wide it is, so can determine which chunks to simulate it for

		// Start with width calculated based on height
		// Travel upwards with a tilt a few to the side (-3 - 3 x) get smaller each time (by one)
		// Occasionally create branches of 1-2 width that branch off and get smaller faster
	}

	private static void GenerateBranch(Chunk chunk, Array<Vector2> directions, int width)
	{
		// First vec is the origin, rest are destinations
		// Maybe have the width render based on the direction (vec pointing up means horizontal line)
		// Calculate when to shrink the branch, based on needing to be at one at the very last destination
		
		// Vector2 totalDirection = directions[0] - directions[directions.Count - 1];
		// float totalLength = Mathf.Max(Mathf.Abs(totalDirection.X), Mathf.Abs(totalDirection.Y));

		float totalLength = 0;
		for (int i = 0; i < directions.Count - 1; i++) {
			totalLength += (directions[i + 1] - directions[i]).Length();
		}


		Vector2 pos = directions[0];

		// width formula: width * (1 - x / length)

		int totalIters = 0;
		for (int i = 1; i < directions.Count; i++) {
			Vector2 direction = directions[i] - directions[i - 1];

			float length;
			Vector2I trunkDir;
			if (Mathf.Abs(direction.X) > Mathf.Abs(direction.Y)) {
				length = Mathf.Abs(direction.X);
				trunkDir = Vector2I.Down;
			}
			else {
				length = Mathf.Abs(direction.Y);
				trunkDir = Vector2I.Left;
			}

			for (int j = 0; j < length; j++) {
				// place the pixels

				int currentWidth = Mathf.CeilToInt(width * (1 - totalIters / totalLength));

				chunk.SetPixel((Vector2I)pos, new Pixel(Colors.Azure));

				Vector2I dir = (Vector2I)pos - Mathf.CeilToInt(currentWidth / 2) * trunkDir;
				GD.Print(dir, trunkDir);
				
				for (int w = 0; w < currentWidth; w++) {
					chunk.SetPixel(dir + w * trunkDir, new Pixel(Colors.Burlywood));
				}

				// After, increment the position
				pos += direction / length;
				totalIters++;
			}
			// currentPosition = directions[i];
			chunk.SetPixel((Vector2I)directions[i], new Pixel(Colors.Red));
		}
	}

	public static Chunk GenerateChunk(Vector2I position)
	{
		// First, find out what the ground level is (in terms of chunks), this should be pregenerated within the save file
		// Then, determine how many chunks beneath that this is. Find out the level of detail and what biome this is
		// If it's not one of the top chunks, then make it loose dirt or stone
		// If it is one of the top chunks, then make it grass with some pebbles, maybe trees?
		// If it's above one of the top chunks, then just make it a bunch of air.

		// If there were no changes made to the chunk, I don't want to store it

		Chunk chunk = new Chunk(Chunk.size * position);

		float[] surfaceLevel = GetSurfaceLevels(position.X, granularity);
		_random.Seed = (ulong)(seed + position.X * 223 + position.Y);
		_randomX.Seed = (ulong)(seed + position.X);
		
		for (int i = 0; i < Chunk.size; i++) {
			float weight = (float)granularity * i / Chunk.size;
			int subdivision = Mathf.FloorToInt(weight);
			float surface = 1000 * Mathf.Lerp(surfaceLevel[subdivision], surfaceLevel[subdivision + 1], weight - subdivision);

			float randomNoiseX = (1 + _dispNoise.GetNoise1D(chunk.Position.X + i)) / 2;

			float randomX = _randomX.RandfRange(0,1);

			float grassHeight = 10 * (randomNoiseX - 0.4f) + randomX * 2; // 3 -> 4
			int grassPosition = _randomX.RandiRange(-2,0);

			for (int j = 0; j < Chunk.size; j++) {
				float distance = j + Chunk.size * position.Y - surface;
				float randomValue = _random.RandfRange(0,1);
				float randomNoise = (1 + _dispNoise.GetNoise2D(chunk.Position.X + i, chunk.Position.Y + j)) / 2;

				if (distance > 0 && randomNoise > rockFrequency.SampleBaked(distance / 60)) {
					chunk.SetPixelLocal(new Vector2I(i, j), new Pixel(Colors.SlateGray.Darkened(randomValue / 10)));
				}
				// TODO: Fix Grass generating under dirt (This succeeds but above that it doesn't)
				else if (grassPosition > distance && distance > grassPosition - grassHeight) {
					chunk.SetPixelLocal(new Vector2I(i, j), new Pixel(Colors.ForestGreen.Lightened(randomX / 5)));
				}
				else if (distance > grassPosition) {
					chunk.SetPixelLocal(new Vector2I(i, j), new Pixel(Colors.SaddleBrown.Lightened(randomValue / 10)));
				}
				if (Mathf.Abs(distance) < 0.5f) {
					// chunk.SetPixelLocal(new Vector2I(i, j), new Pixel(Colors.OrangeRed));
				}
			}
		}
		
		Array<Vector2> array = [new Vector2(0,0), new Vector2(3, -30), new Vector2(-3,-60), new Vector2(-30, -80)];

		GenerateBranch(chunk, array, 5);


		if (!chunk._imageUpdated) {
			// Chunk is only air, don't want to add it
			return null;
		}
		
		return chunk;
	}
}
