using Godot;
using Godot.Collections;

public partial class World : Node2D
{
	public Dictionary<Vector2I,Chunk> chunkStorage;
	private Vector2 cameraSize;
	private Camera2D cam;

	private Vector2I currChunk;
	private Rect2I loadedChunks;

    public override void _Ready()
    {
		chunkStorage = new Dictionary<Vector2I, Chunk>();
		cameraSize = GetCanvasTransform().AffineInverse().BasisXform(GetViewportRect().Size) * 0.5f;
		cam = GetParent().GetNode<Camera2D>("Camera2D");
		Vector2 cameraTopLeft = cam.GetScreenCenterPosition() - cameraSize / 2;
		Vector2I chunkTopLeft = ToChunkPosition((Vector2I)cameraTopLeft);
		loadedChunks = new Rect2I(0,0,0,0); // Empty Rectangle (none are loaded)
		currChunk = ToChunkPosition((Vector2I)cam.GetScreenCenterPosition());
		
		Rect2I chunksToLoad = new Rect2I(chunkTopLeft, ToChunkPosition((Vector2I)(cameraSize * 1.3f)));
		LoadChunks(chunksToLoad);
    }

    private Vector2I ToChunkPosition(Vector2I pos)
	{
		return pos / Chunk.size;
	}

	public void SetPixel(Vector2I pos, Pixel pixel)
	{
		Vector2I chunkPos = ToChunkPosition(pos);

		if (chunkStorage.ContainsKey(chunkPos)) {
			Chunk chunk = chunkStorage[chunkPos];
			chunk.SetPixel(pos, pixel);
		}
	}

	private void LoadChunks(Rect2I newChunks)
	{
		Rect2I intersection = newChunks.Intersection(loadedChunks);

		// loop through next chunks to add
		for (int i = 0; i < newChunks.Size.X; i++) {
			for (int j = 0; j < newChunks.Size.Y; j++) {
				Vector2I position = new Vector2I(i, j)  + newChunks.Position;
				if (!intersection.HasPoint(position)) {
					// Create new Chunk
					chunkStorage[position] = new Chunk
					{
						Position = Chunk.size * position
					};
					AddChild(chunkStorage[position]);
				}
			}
		}

		// loop through old chunks to remove
		for (int i = 0; i < loadedChunks.Size.X; i++) {
			for (int j = 0; j < loadedChunks.Size.Y; j++) {
				Vector2I position = new Vector2I(i, j) + loadedChunks.Position;
				if (!intersection.HasPoint(position)) {
					// Remove Old Chunk
					chunkStorage[position].QueueFree();
					chunkStorage.Remove(position);
				}
			}
		}

		// Update Variables
		loadedChunks = newChunks;
	}

    public override void _Process(double delta)
    {
		Vector2I nextChunk = ToChunkPosition((Vector2I)cam.GetScreenCenterPosition());
		
		if (nextChunk != currChunk) {
			Vector2I diff = nextChunk - currChunk;

			Rect2I newChunks = new Rect2I(loadedChunks.Position + diff, loadedChunks.Size);
			LoadChunks(newChunks);

			currChunk = nextChunk;
		}
    }
}
