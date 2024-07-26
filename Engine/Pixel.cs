using Godot;
using System;

public partial class Pixel : Resource
{
	public Color color;

	public Pixel(Color _color) {
		color = _color;
	}
}
