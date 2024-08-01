using Godot;
using Godot.Collections;

public partial class World : Node2D
{
	public Godot.Collections.Dictionary<Vector2I,Chunk> chunkStorage;
	public Array<Chunk> chunkPool;
	private Vector2 _cameraSize;
	private Camera2D _cam;
	private Vector2I _currChunk;
	private Rect2I _loadedChunks;

    public override void _Ready()
    {
		chunkStorage = new Godot.Collections.Dictionary<Vector2I, Chunk>();
		chunkPool = new Array<Chunk>();
		_cameraSize = GetCanvasTransform().AffineInverse().BasisXform(GetViewportRect().Size) * 1.2f;
		_cam = GetParent().GetNode<Camera2D>("Camera2D");
		Vector2 cameraTopLeft = _cam.GetScreenCenterPosition() - _cameraSize / 2;
		Vector2 cameraBottomRight = _cam.GetScreenCenterPosition() + _cameraSize / 2;
		Vector2I chunkTopLeft = ToChunkPosition((Vector2I)cameraTopLeft);
		Vector2I chunkBottomRight = ToChunkPosition((Vector2I)cameraBottomRight) + Vector2I.One;
		_loadedChunks = new Rect2I(0,0,0,0); // Empty Rectangle (none are loaded)
		_currChunk = ToChunkPosition((Vector2I)_cam.GetScreenCenterPosition());

		// Rect2I chunksToLoad = new Rect2I(chunkTopLeft, chunkBottomRight - chunkTopLeft + Vector2I.One);
		Rect2I chunksToLoad = new Rect2I(chunkTopLeft, 3, 3);
		_loadedChunks = chunksToLoad;
		
		WorldGenerator.GenerateWorld(this, chunksToLoad);
    }

    public static Vector2I ToChunkPosition(Vector2I pos)
	{
		return new Vector2I(Mathf.FloorToInt(pos.X / (float)Chunk.size), Mathf.FloorToInt(pos.Y / (float)Chunk.size));
	}

	// Global Coords
	public void SetPixel(Vector2I pos, Pixel pixel)
	{
		Vector2I chunkPos = ToChunkPosition(pos);

		if (chunkStorage.ContainsKey(chunkPos)) {
			Chunk chunk = chunkStorage[chunkPos];
			chunk.SetPixel(pos, pixel);
		}
	}

	// Local Coords
	public void SetPixel(Vector2I chunkPos, Vector2I pos, Pixel pixel)
	{
		if (chunkStorage.ContainsKey(chunkPos)) {
			Chunk chunk = chunkStorage[chunkPos];
			chunk.SetPixelLocal(pos, pixel);
		}
	}

	private void LoadChunks(Rect2I newChunks)
	{
		// TODO: Implement Chunk Pooling, Possibly research RenderingServer
		// Chunk Pooling: Never deallocate, instead move the chunk to a new position and reassign data
		// Perhaps have a stack of chunks in the pool (because will never have all of them allocated at once due to air chunks)
		Rect2I intersection = newChunks.Intersection(_loadedChunks);

		// GD.Print(chunkStorage);

		// loop through old chunks to remove
		for (int i = 0; i < _loadedChunks.Size.X; i++) {
			for (int j = 0; j < _loadedChunks.Size.Y; j++) {
				Vector2I position = new Vector2I(i, j) + _loadedChunks.Position;
				if (!intersection.HasPoint(position)) {
					// Remove Old Chunk
					if (chunkStorage.ContainsKey(position)) {
						chunkStorage[position].Hide();
						chunkPool.Add(chunkStorage[position]);
						chunkStorage.Remove(position);
					}
				}
			}
		}

		// loop through next chunks to add
		for (int i = 0; i < newChunks.Size.X; i++) {
			for (int j = 0; j < newChunks.Size.Y; j++) {
				Vector2I position = new Vector2I(i, j)  + newChunks.Position;
				if (!intersection.HasPoint(position)) {
					// Create new Chunk
					// if (chunkStorage.ContainsKey(position)) {
					// 	Chunk chunk = chunkStorage[position];
					// 	chunk.Show();
					// }
					// else {
						// GD.Print("Overriding chunk in pool");
						Chunk chunk = chunkPool[0];
						chunk.Clear();
						if (WorldGenerator.GenerateChunk(ref chunk, position)) {
							chunkPool.RemoveAt(0);
							chunkStorage[position] = chunk;
							chunk.Show();
						}
					// }
				}
			}
		}

		GD.Print(chunkPool.Count);

		// Update Variables
		_loadedChunks = newChunks;
	}

    public override void _Process(double delta)
    {
		Vector2I nextChunk = ToChunkPosition((Vector2I)_cam.GetScreenCenterPosition());

		if (nextChunk != _currChunk) {
			Vector2I diff = nextChunk - _currChunk;

			Rect2I newChunks = new Rect2I(_loadedChunks.Position + diff, _loadedChunks.Size);
			LoadChunks(newChunks);

			_currChunk = nextChunk;
		}
    }
}
