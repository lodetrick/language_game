using Godot;

// Credit to:
// https://www.reddit.com/r/godot/comments/1ci4s71/efficient_way_to_draw_129600_rects/

[Tool]
public partial class Chunk : Sprite2D
{
	public static int size = 64;
	public Pixel[] pixels;
	private Image image;
	private bool imageUpdated;

    public override void _Ready()
    {
		pixels = new Pixel[size * size];
		image = Image.Create(size,size,false,Image.Format.Rgb8);
		image.Fill(new Color(1,0,0));
		Texture = ImageTexture.CreateFromImage(image);
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

		image.SetPixel(local.X, local.Y, pixel.color);
		imageUpdated = true;
	}

    public override void _Draw()
    {
		if (Engine.IsEditorHint()) {
			DrawRect(new Rect2(0,0,size,size), new Color(0.5f,0.5f,1.0f,0.3f), false, 5);
		}
		else if (imageUpdated) {
			((ImageTexture)Texture).Update(image);
			imageUpdated = false;
		}
    }
}
