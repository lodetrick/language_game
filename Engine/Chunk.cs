using Godot;
using Godot.Collections;

// Credit to:
// https://www.reddit.com/r/godot/comments/1ci4s71/efficient_way_to_draw_129600_rects/

public partial class Chunk : Sprite2D
{
	public static int size = 32;
	public Pixel[] pixels;
	private Image _image;
	public bool _imageUpdated {get; private set;}

	public Chunk()
	{
		pixels = new Pixel[size * size];
		_image = Image.Create(size,size,false,Image.Format.Rgba8);
		Texture = ImageTexture.CreateFromImage(_image);
		_imageUpdated = false;
		Centered = false;
		ShowBehindParent = true;
	}

    public override void _Ready()
    {
		AddToGroup("Chunks");
    }

	private Vector2I GlobalToLocal(Vector2I global)
	{
		return global - (Vector2I)Position;
	}

	private int LocalToIndex(Vector2I local)
	{
		return size * local.X + local.Y;
	}

	public void SetPixel(Vector2I global, Pixel pixel)
	{
		Vector2I local = GlobalToLocal(global);
		int index = LocalToIndex(local);

		pixels[index] = pixel;

		_image.SetPixel(local.X, local.Y, pixel.color);
		_imageUpdated = true;
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
		DrawRect(new Rect2(0,0,size,size), new Color(0.5f,0.5f,1.0f,0.3f), false, 2);
		if (Engine.IsEditorHint()) {
			// DrawRect(new Rect2(0,0,size,size), new Color(0.5f,0.5f,1.0f,0.3f), false, 5);
		}
		else if (_imageUpdated) {
			((ImageTexture)Texture).Update(_image);
			_imageUpdated = false;
		}
    }

	public Array<Vector2[]> GetPolygons() {
		Bitmap bitmap = new Bitmap();
		bitmap.CreateFromImageAlpha(_image);

		return bitmap.OpaqueToPolygons(new Rect2I(new Vector2I(), bitmap.GetSize()));
	}
}
