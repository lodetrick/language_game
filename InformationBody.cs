using Godot;
using System;
using System.ComponentModel.DataAnnotations;

public class InformationBody
{
	static int MAX_CONTAIN_RECURSION = 3;
	static float COLLISION_IMPACT_PERCENTAGE = 0.20f;

	private MaterialTemplate _currentMaterial;
	private MagicBody _physicsHook;

	private float _manaEarth, _manaWater, _manaAir, _manaFire, _manaTotal, _density;
	private float _pressure, _manaKinetic; // Density Difference & Movement
	private float _stability = 1.0f;

	private Shape.Types _shape = Shape.Types.Ball, _trueShape = Shape.Types.Ball;
	private InformationBody _contained = null;
	private int _containLevel = 0; // current level of container (top is zero), want to have a max

	public void UpdatePhysicalProperties(float speedSquared, float currentArea) // Call every frame?
	{
		_density = _manaTotal / currentArea;
		_manaKinetic = 0.5f * _manaTotal * speedSquared; // Based on Movement & total Mana (1/2 m v^2)
		_pressure = _currentMaterial.GetDensityCoefficient()
					* (_currentMaterial.GetDesiredDensity() - _density);
		// if containing: update 
		if (_shape == Shape.Types.Container && _contained != null) {
			_contained.UpdatePhysicalProperties(speedSquared, currentArea);
			TryContain();
		}
	}
	private void UpdateMaterial() 
	{
		// Calculate the nearest predetermined material
		// Based on mana values, density
		// Very important to get right
	}

	private bool TryContain(InformationBody contain = null) 
	{
		// Check if force by contained object (density difference) is stronger than containing force
		// If force difference is within threshold, this becomes unstable (but can contain it)
		// If it's too much, it breaks immediately
		// Otherwise, it is able to contain it.

		// Stable object
		_stability = _currentMaterial.GetCompressionFactor();

		if (_shape != Shape.Types.Container) {
			return true; // Not containing anything, so can contain everything
		}

		if (contain == null) {
			contain = _contained;
		}
		else if (_contained != null) {
			return _contained.TryContain(contain);
		}

		if (contain._pressure >= 0.0f) {
			_stability = _currentMaterial.GetExpansionFactor() - contain._pressure;
		}
		else {
			_stability = _currentMaterial.GetCompressionFactor() + contain._pressure;
		}

		if (_stability < 0.0f) {
			return false;
		}

		_contained = contain;
		return UpdateContainLevels() <= MAX_CONTAIN_RECURSION; // If false, failure occurs and the container breaks
	}

	private int UpdateContainLevels() 
	{ // Helper
		if (_contained != null) {
			_contained._containLevel = _containLevel + 1;
			return _contained.UpdateContainLevels();
		}
		return _containLevel;
	}

	public bool ChangeShape(Shape.Types newShape, InformationBody contain = null) 
	{
		_shape = newShape;
		_trueShape = GetTrueShape();
		return TryContain(contain);
	}

	public Shape.Types GetTrueShape()
	{
		if (_shape == Shape.Types.Container && _contained != null) {
			return _contained.GetTrueShape();
		}
		return _shape;
	}

	public void CalculateInteraction(InformationBody other, float relativeMomenta)
	{
		// First, find the relative kinetic mana. This tells how hard these two bumped into each other
		// We pass that in as an input
		// Call this function once per interaction

		// Compare instability, two strengths, etc to decide what to do for collisions
		float thisShapeModifier = 1.0f, otherShapeModifier = 1.0f; // Multiplicative Identity; Does nothing

		// Based on Kinetic Energy, we determine a defender and attacker
		if (this._manaKinetic >= other._manaKinetic) {
			// This is attacker
			thisShapeModifier = Shape.attackModifiers[(int)_trueShape];
			otherShapeModifier = Shape.defenseModifiers[(int)other._trueShape];
		}
		else {
			// Other is attacker
			thisShapeModifier = Shape.defenseModifiers[(int)_trueShape];
			otherShapeModifier = Shape.attackModifiers[(int)other._trueShape];
		}

		if (_stability * thisShapeModifier <= relativeMomenta) {
			// This Bursts
		}

		if (other._stability * otherShapeModifier <= relativeMomenta) {
			// Other bursts
		}

		// Check flammability
		if (other._manaFire * other._manaTotal >= _currentMaterial.GetCombustionManaNeeded()) {
			// This combusts
		}

		if (_manaFire * _manaTotal >= other._currentMaterial.GetCombustionManaNeeded()) {
			// Other combusts
		}

		// Check for reactions

		// Damages Each other, need to make sure this keeps the density the same (they shrink)
		// Ratio
		float thisChipMana = _currentMaterial.GetCohesiveness() / thisShapeModifier;
		float otherChipMana = other._currentMaterial.GetCohesiveness() / otherShapeModifier;

		// Total Percentage that we want to lose: COLLISION_IMPACT_PERCENTAGE * relativeMomenta
		// thisChipMana : otherChipMana - ratio of this percentage loss to other percentage loss

		// FOR LATER: Need to make this a safe subtraction, isopycnal (preserving density, so area decreases)
		_manaTotal -= _manaTotal * (thisChipMana / (thisChipMana + otherChipMana)) * COLLISION_IMPACT_PERCENTAGE * relativeMomenta;
		other._manaTotal -= other._manaTotal * (otherChipMana / (thisChipMana + otherChipMana)) * COLLISION_IMPACT_PERCENTAGE * relativeMomenta;
	}

	// private void ContainerBurst() {} // Simulate the bursting of a container. Perhaps should be on physics side?

	// private void _on_area_entered(Area2D area) // Put this in a bigger class (want to separate physics and magic)
	// {
	// 	// Determine Type of Interaction
	// 	// If object, then calculate the interaction (and possibly the physics)
	// 	// If creature, calculate damage
	// }
}

public static class Shape {
	public enum Types : int {
		Ball, // Circle
		Spear, // Capsule?
		Wall, // Rectangle
		Container, // Same as containing but larger
		Creature, // Custom shape, based on the creature
	}

	public readonly static float[] attackModifiers = [1,1,1,1.0f,1];
	public readonly static float[] defenseModifiers = [1,1,1,1.0f,1];
}