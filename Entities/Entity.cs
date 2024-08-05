using Godot;

public partial class Entity : Sprite2D
{
	private Rect2 _bounding;
	private Vector2 _velocity = Vector2.Zero;
	private Vector2I _oldPos = Vector2I.Zero;

	public Entity(Rect2 bounding)
	{
		_bounding = bounding;
	}

    public override void _Ready()
    {

    }

    public override void _PhysicsProcess(double delta)
    {
		float deltaf = (float)delta;
		TryMove(deltaf);
    }


	private void TryMove(float delta)
	{
		// Try to translate Position
		// If it is different from the old position, then run collision
		Vector2 newPos = Position + _velocity * delta;
		if ((Vector2I)newPos != _oldPos) {

			if (Global.ContainsSolid((Rect2I)_bounding)) {
				return; // Move failed
			}

		}
		Position = newPos;
		_bounding.Position = newPos;
	}

    public override void _Draw()
    {
		DrawRect(new Rect2(0,0, _bounding.Size), Colors.Red, false, 2);
    }
}
