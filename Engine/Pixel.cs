using Godot;
using System;

public partial class Pixel
{
	public Color color;
	private MaterialState _currentMat;

	public Pixel()
	{
		Clear();
	}

	public Pixel(Color _color, MaterialState mat)
	{
		color = _color;
		_currentMat = mat;
	}

	public void Clear()
	{
		color = Colors.Transparent;
		_currentMat = Global.materialStates["Air"];
	}

	public void Replace(Color _color, MaterialState mat)
	{
		color = _color;
		_currentMat = mat;
	}
}
