using Godot;

public abstract class MaterialState
{
	// private float _manaEarth, _manaWater, _manaAir, _manaFire, _manaTotal; // should be in pixel?

	public abstract void ApplyHeat(ref Pixel p, float amount);

	public abstract void TouchMaterial(ref Pixel p, ref Pixel other);
}
