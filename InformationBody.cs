using Godot;
using System;
using System.ComponentModel.DataAnnotations;

public class InformationBody
{
	static int MAX_CONTAIN_RECURSION = 3;
	static float COLLISION_IMPACT_PERCENTAGE = 0.20f;

	private Material.MaterialTemplate _currentMaterial;
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
			// TODO: Do something bad if the trycontain fails
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
		// TODO: Do something bad when trycontain fails
		return TryContain(contain);
	}

	public Shape.Types GetTrueShape()
	{
		if (_shape == Shape.Types.Container && _contained != null) {
			return _contained.GetTrueShape();
		}
		return _shape;
	}

	private void CalculateInteractionBurst(InformationBody other, float relativeMomenta, float thisShapeModifier, float otherShapeModifier)
	{
		// First, find the relative kinetic mana. This tells how hard these two bumped into each other
		// Compare instability, two strengths, etc to decide what to do for collisions

		if (_stability * thisShapeModifier <= relativeMomenta) {
			// TODO: This Bursts
		}

		if (other._stability * otherShapeModifier <= relativeMomenta) {
			// TODO: Other bursts
		}
	}

	private void CalculateInteractionFlammable(InformationBody other)
	{
		// Check flammability
		if (other._manaFire * other._manaTotal >= _currentMaterial.GetCombustionManaNeeded()) {
			// TODO: This combusts
		}

		if (_manaFire * _manaTotal >= other._currentMaterial.GetCombustionManaNeeded()) {
			// TODO: Other combusts
		}
	}

	private void CalculateInteractionDamage(InformationBody other, float thisShapeModifier, float otherShapeModifier, float totalManaLoss)
	{
		// Damages Each other, need to make sure this keeps the density the same (they shrink)
		// Total Percentage that we want to lose: COLLISION_IMPACT_PERCENTAGE * relativeMomenta
		// thisChipMana : otherChipMana - ratio of this percentage loss to other percentage loss
		float thisChipMana = _currentMaterial.GetCohesiveness() / thisShapeModifier;
		float otherChipMana = other._currentMaterial.GetCohesiveness() / otherShapeModifier;

		// TODO: Need to make this a safe subtraction, isopycnal (preserving density, so area decreases)

		// Should give bonus damage if the object is destroyed
		// If the damage is too small it should be ignored entirely, material dependent
		// How to deal with knockback? What about gases?
		_manaTotal -= (thisChipMana / (thisChipMana + otherChipMana)) * totalManaLoss;
		other._manaTotal -= (otherChipMana / (thisChipMana + otherChipMana)) * totalManaLoss;
	}

	private void CalculateInteractionReaction(InformationBody other, Material.Reaction reaction, float totalManaLoss)
	{
		// Perform Reaction, We got the ratios, we just need to know how much to create
		// Find the ratios for each body
		float thisRatio, otherRatio, thisManaLoss, otherManaLoss, resultantManaGain;
		if ((int)_currentMaterial.MATERIAL_ID < (int)other._currentMaterial.MATERIAL_ID) {
			thisRatio = reaction.ratioL; otherRatio = reaction.ratioH;
		}
		else {
			thisRatio = reaction.ratioH; otherRatio = reaction.ratioL;
		}

		// Determine Limiting Reactant, then Find the mana values for all three objects
		if (_manaTotal / thisRatio <= other._manaTotal / otherRatio) {
			// This is limiting reactant
			thisManaLoss = Mathf.Clamp(thisRatio / (thisRatio + otherRatio) * totalManaLoss, 0.0f, _manaTotal);
			otherManaLoss = otherRatio / thisRatio * thisManaLoss;
			resultantManaGain = reaction.ratioR / thisRatio * thisManaLoss;
		}
		else {
			// Other is limiting reactant
			otherManaLoss = Mathf.Clamp(otherRatio / (thisRatio + otherRatio) * totalManaLoss, 0.0f, other._manaTotal);
			thisManaLoss = otherManaLoss * thisRatio / otherRatio;
			resultantManaGain = otherManaLoss * reaction.ratioR / otherRatio;
		}

		// Perform Reaction with mana loss levels:
		// TODO: Add Physics for Reactions
		_manaTotal -= thisManaLoss;
		other._manaTotal -= otherManaLoss;
		// create new body with the reactant
	}

	// This function and its helpers are what I'm going to be modifying the most of on the physics side.
	public void CalculateInteraction(InformationBody other, float relativeMomenta)
	{
		float thisShapeModifier = 1.0f, otherShapeModifier = 1.0f; // Multiplicative Identity; Does nothing

		// Based on Kinetic Energy, we determine a defender and attacker
		if (_manaKinetic >= other._manaKinetic) {
			// This is attacker
			thisShapeModifier = Shape.attackModifiers[(int)_trueShape];
			otherShapeModifier = Shape.defenseModifiers[(int)other._trueShape];
		}
		else {
			// Other is attacker
			thisShapeModifier = Shape.defenseModifiers[(int)_trueShape];
			otherShapeModifier = Shape.attackModifiers[(int)other._trueShape];
		}

		CalculateInteractionBurst(other, relativeMomenta, thisShapeModifier, otherShapeModifier);
		CalculateInteractionFlammable(other);

		// Check for reactions
		float totalManaLoss = 0.5f * (_manaTotal + other._manaTotal) * COLLISION_IMPACT_PERCENTAGE * relativeMomenta;
		Material.Reaction reaction = Material.GetReaction(_currentMaterial.MATERIAL_ID, other._currentMaterial.MATERIAL_ID);
		if (reaction.result == Material.Types.None) {
			// Don't want to do this if either is a gas, if one is a gas then turn the damage into knockback
			CalculateInteractionDamage(other, thisShapeModifier, otherShapeModifier, totalManaLoss);
		}
		else {
			CalculateInteractionReaction(other, reaction, totalManaLoss);
		}
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