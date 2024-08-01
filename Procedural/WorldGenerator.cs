using System;
using Godot;
using Godot.Collections;

public partial class WorldGenerator
{
	private static FastNoiseLite _noise, _dispNoise;
	private static RandomNumberGenerator _random, _randomX;

	private static readonly MaterialState[] materialStates;
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

		materialStates = new MaterialState[17];
		materialStates[0] = new WaterMat();
		materialStates[1] = new FireMat();
		materialStates[2] = new EarthMat();
		materialStates[3] = new AirMat();
		materialStates[4] = new GlassMat();
		materialStates[5] = new OilMat();
		materialStates[6] = new WoodMat();
		materialStates[7] = new SmokeMat();
		materialStates[8] = new SteamMat();
		materialStates[9] = new IronMat();
		materialStates[10] = new DustMat();
		materialStates[11] = new StoneMat();
		materialStates[12] = new GoldMat();
		materialStates[13] = new FleshMat();
		materialStates[14] = new IceMat();
		materialStates[15] = new SlagMat();
		materialStates[16] = new PlantMat();
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

		float length, totalLength = 0, currIter = 0;
		for (int i = 1; i < directions.Count; i++) {
			totalLength += (directions[i] - directions[i - 1]).Length();
		}

		Vector2I trunkDir;
		Vector2 pos = directions[0];

		for (int i = 1; i < directions.Count; i++) {
			Vector2 direction = directions[i] - directions[i - 1];

			if (Mathf.Abs(direction.X) > Mathf.Abs(direction.Y)) {
				length = Mathf.Abs(direction.X);
				trunkDir = Vector2I.Down;
			}
			else {
				length = Mathf.Abs(direction.Y);
				trunkDir = Vector2I.Left;
			}

			for (int j = 0; j < length; j++) {
				// width formula: width * (1 - x / length)
				int currentWidth = Mathf.CeilToInt(width * (1 - currIter / totalLength));

				Vector2I dir = (Vector2I)pos - Mathf.CeilToInt(currentWidth / 2) * trunkDir;
				
				for (int w = 0; w < currentWidth; w++) {
					chunk.SetPixel(dir + w * trunkDir, new Pixel(Colors.Burlywood, materialStates[6]));
				}

				// After, increment the position
				pos += direction / length;
				currIter++;
			}
		}
	}

	public static bool GenerateChunk(ref Chunk chunk, Vector2I position)
	{
		// First, find out what the ground level is (in terms of chunks), this should be pregenerated within the save file
		// Then, determine how many chunks beneath that this is. Find out the level of detail and what biome this is
		// If it's not one of the top chunks, then make it loose dirt or stone
		// If it is one of the top chunks, then make it grass with some pebbles, maybe trees?
		// If it's above one of the top chunks, then just make it a bunch of air.

		// If there were no changes made to the chunk, I don't want to store it
		if (position.Y > 2) {
			return false;
		}

		Array<Vector2> array = [new Vector2(0,0), new Vector2(3, -30), new Vector2(-3,-60), new Vector2(-10, -80)];

		GenerateBranch(chunk, array, 5);

		float[] surfaceLevel = GetSurfaceLevels(position.X, granularity);
		_random.Seed = (ulong)(seed + position.X * 223 + position.Y);
		_randomX.Seed = (ulong)(seed + position.X);
		
		for (int i = 0; i < Chunk.size; i++) {
			float weight = (float)granularity * i / Chunk.size;
			int subdivision = Mathf.FloorToInt(weight);
			float surface = 500 * Mathf.Lerp(surfaceLevel[subdivision], surfaceLevel[subdivision + 1], weight - subdivision);

			float randomNoiseX = (1 + _dispNoise.GetNoise1D(chunk.Position.X + i)) / 2;

			float randomX = _randomX.RandfRange(0,1);

			float grassHeight = 10 * (randomNoiseX - 0.4f) + randomX * 2; // 3 -> 4
			int grassPosition = _randomX.RandiRange(-2,0);

			for (int j = 0; j < Chunk.size; j++) {
				float height = j + Chunk.size * position.Y;
				float distance = height - surface;
				float randomValue = _random.RandfRange(0,1);
				float randomNoise = (1 + _dispNoise.GetNoise2D(chunk.Position.X + i, chunk.Position.Y + j)) / 2;

				if (distance > 0 && randomNoise > rockFrequency.SampleBaked(distance / 60)) {
					Pixel p = new Pixel(Colors.SlateGray.Darkened(randomValue / 10), materialStates[11]);
					chunk.SetPixelLocal(new Vector2I(i, j), p);
				}
				// TODO: Fix Grass generating under dirt (This succeeds but above that it doesn't) Probably fixed?
				else if (grassPosition > distance && distance > grassPosition - grassHeight) {
					Pixel p = new Pixel(Colors.ForestGreen.Lightened(randomX / 5), materialStates[16]);
					chunk.SetPixelLocal(new Vector2I(i, j), p);
				}
				else if (distance > grassPosition) {
					Pixel p = new Pixel(Colors.SaddleBrown.Lightened(randomValue / 10), materialStates[2]);
					chunk.SetPixelLocal(new Vector2I(i, j), p);
				}
				// if (Mathf.Abs(distance) < 0.5f) {
				// 	chunk.SetPixelLocal(new Vector2I(i, j), new Pixel(Colors.OrangeRed));
				// }
			}
		}

		if (!chunk._imageUpdated) {
			// Chunk is only air, don't want to add it
			return false;
		}

		chunk.Position = Chunk.size * position;
		
		return true;
	}

	public static void GenerateColumn(World world, int pos) {
		// Find the surface levels
		float[] surfaceLevel = new float[Chunk.size];

		for (int i = 0; i < Chunk.size; i++) {
			surfaceLevel[i] = GetSurfaceHeight(pos + i / (float)Chunk.size);
		}

		// Determine what wide structures are trying to spawn here
		
		// Village
	}

	public static void GenerateWorld(World world, Rect2I chunkBounds)
	{
		// First Pass: Generate general shape of land
		for (int i = 0; i < chunkBounds.Size.X; i++) {
			for (int j = 0; j < chunkBounds.Size.Y; j++) {
				Vector2I chunkPos = chunkBounds.Position + new Vector2I(i,j);

				Chunk chunk = new Chunk();
				if (GenerateChunk(ref chunk, chunkPos)) {
					world.chunkStorage[chunkPos] = chunk;
					world.AddChild(chunk);
				}
				else {
					world.chunkPool.Add(chunk);
				}
			}
		}

		// Second Pass: Caves?

		// Third Pass: Generate everything (grass, trees, rocks, etc) on the surface
	}
}
