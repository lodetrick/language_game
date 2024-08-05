using System;
using Godot;
using Godot.Collections;

// Credit to:
// https://www.reddit.com/r/godot/comments/1ci4s71/efficient_way_to_draw_129600_rects/

public partial class Chunk : StaticBody2D
{
	public static int size;
	private static Rect2 bounding;
	public Vector2I _pos;
	private int numAirTiles = size * size;
	public bool _imageUpdated {get; private set;}
	public Pixel[] pixels;
	private Image _image;
	private ImageTexture _texture;
	private Bitmap _bitmap;

	static Chunk()
	{
		size = 128;
		bounding = new Rect2(0, 0, size, size);
	}

	public Chunk()
	{
		pixels = new Pixel[size * size];
		
		for (int i = 0; i < size * size; i++) {
			pixels[i] = new Pixel();
		}

		_image = Image.Create(size,size,false,Image.Format.Rgba8);
		// Texture = ImageTexture.CreateFromImage(_image);
		_texture = ImageTexture.CreateFromImage(_image);
		_bitmap = new Bitmap();
		_bitmap.CreateFromImageAlpha(_image);
		_imageUpdated = false;
		ShowBehindParent = true;

		ProcessMode = ProcessModeEnum.Disabled;
	}

    public override void _Ready()
    {
		AddToGroup("Chunks");
    }

	public void Clear()
	{
		for (int i = 0; i < size * size; i++) {
			pixels[i].Clear();
		}
		_image.Fill(Colors.Transparent);
		_bitmap.CreateFromImageAlpha(_image);
	}

	public void SetPos(Vector2 nPos)
	{
		_pos = (Vector2I)nPos;
		// Position = nPos;
	}

	private Vector2I GlobalToLocal(Vector2I global)
	{
		return global - _pos;
	}

	private int LocalToIndex(Vector2I local)
	{
		return size * local.X + local.Y;
	}

	public Pixel GetPixel(Vector2I global)
	{
		Vector2I local = GlobalToLocal(global);

		if (!bounding.HasPoint(local)) {
			GD.PrintErr("Chunk does not contain pixel at [", global,"]");
			return null;
		}

		int index = LocalToIndex(local);

		return pixels[index];
	}

	public void SetPixel(Vector2I global, Pixel pixel)
	{
		Vector2I local = GlobalToLocal(global);
		
		if (!bounding.HasPoint(local)) {
			return;
		}

		int index = LocalToIndex(local);

		pixels[index] = pixel;

		_image.SetPixel(local.X, local.Y, pixel.color);
		_imageUpdated = true;

		_bitmap.SetBitv(local, true);
		numAirTiles--;

		if (numAirTiles == 0) {
			// Disable Collision
		}
	}

	public void SetPixelLocal(Vector2I local, Pixel pixel)
	{
		int index = LocalToIndex(local);

		pixels[index] = pixel;

		_image.SetPixel(local.X, local.Y, pixel.color);
		_imageUpdated = true;
	}

    public override void _Draw()
    {
		if (_imageUpdated) {
			_texture.Update(_image);
			_imageUpdated = false;
		}

		Rect2 pos = new Rect2(_pos,size,size);
		DrawTexture(_texture, _pos);
		DrawRect(pos, new Color(0.5f,0.5f,1.0f,0.1f), false, 2);
    }

    public bool ContainsSolid(Rect2I rect) // Global coords
    {
		Vector2I localPos = GlobalToLocal(rect.Position);

		for (int i = 0; i < rect.Size.X; i++) {
			for (int j = 0; j < rect.Size.Y; j++) {
				if (_bitmap.GetBit(localPos.X + i, localPos.Y + j)) {
					return true;
				}
			}
		}
		return false;
    }

    // public void UpdateCollision() 
    // {
    // 	_bitmap.OpaqueToPolygons(new Rect2I(new Vector2I(), _bitmap.GetSize()));
    // }
}
