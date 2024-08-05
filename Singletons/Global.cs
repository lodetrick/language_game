using Godot;
using Godot.Collections;

public partial class Global : Node
{
    public static Global Instance { get; private set; }

	public static Dictionary<Vector2I, Chunk> chunkStorage;
	public static readonly System.Collections.Generic.Dictionary<string, MaterialState> materialStates;
	public static Array<Chunk> chunkPool;

	static Global()
	{
		chunkStorage = new Godot.Collections.Dictionary<Vector2I, Chunk>();
		chunkPool = new Array<Chunk>();

		materialStates = new System.Collections.Generic.Dictionary<string, MaterialState>
        {
            { "Water", new WaterMat() },
            { "Fire", new FireMat() },
            { "Earth", new EarthMat() },
            { "Air", new AirMat() },
            { "Glass", new GlassMat() },
            { "Oil", new OilMat() },
            { "Wood", new WoodMat() },
            { "Smoke", new SmokeMat() },
            { "Steam", new SteamMat() },
            { "Iron", new IronMat() },
            { "Dust", new DustMat() },
            { "Stone", new StoneMat() },
            { "Gold", new GoldMat() },
            { "Flesh", new FleshMat() },
            { "Ice", new IceMat() },
            { "Slag", new SlagMat() },
            { "Plant", new PlantMat() }
        };
	}

    public override void _Ready()
    {
        Instance = this;
	}

	public static Vector2I ToChunkPosition(Vector2I pos)
	{
		return new Vector2I(Mathf.FloorToInt(pos.X / (float)Chunk.size), Mathf.FloorToInt(pos.Y / (float)Chunk.size));
	}

	public static bool ContainsSolid(Rect2I rect)
	{
		// Find the chunks that intersect the rect: Convert top left and bottom right to chunk coords, everything in that rect
		Vector2I chunkTopLeft = ToChunkPosition(rect.Position);
		Vector2I chunkBottomRight = ToChunkPosition(rect.Position + rect.Size);

		for (int i = chunkTopLeft.X; i < chunkBottomRight.X; i++) {
			for (int j = chunkTopLeft.Y; j < chunkBottomRight.Y; j++) {
				if (chunkStorage[new Vector2I(i,j)].ContainsSolid(rect.Intersection(new Rect2I(i * Chunk.size, j * Chunk.size, Chunk.size, Chunk.size)))) {
					return true;
				}
			}
		}
		return false;
	}

	public static Pixel GetPixel(Vector2I pos)
	{
		Vector2I chunkPos = ToChunkPosition(pos);

		Chunk chunk;
		if (chunkStorage.TryGetValue(chunkPos, out chunk)) {
			return chunk.GetPixel(pos);
		}
		GD.PrintErr("Pixel at [", pos, "] not in memory, returning null");
		return null;
	}

	// Global Coords
	public static void SetPixel(Vector2I pos, Pixel pixel)
	{
		Vector2I chunkPos = ToChunkPosition(pos);

		Chunk chunk;
		if (chunkStorage.TryGetValue(chunkPos, out chunk)) {
			chunk.SetPixel(pos, pixel);
		}
	}

	// Local Coords
	public static void SetPixel(Vector2I chunkPos, Vector2I pos, Pixel pixel)
	{
		Chunk chunk;
		if (chunkStorage.TryGetValue(chunkPos, out chunk)) {
			chunk.SetPixelLocal(pos, pixel);
		}
	}
}