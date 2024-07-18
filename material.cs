public abstract class MaterialTemplate {
	public InformationBody _information { get; set; } // THIS BETTER BE A POINTER
	public static Material.Types MATERIAL_ID { get; private set; } // Identifying number, unique for each material type

	public MaterialTemplate(InformationBody information) {
		this._information = information;
	}

	// Flammability
	public abstract float GetCombustionManaNeeded(); // When the material starts to burn, total mana (not percentage)
	public abstract MaterialTemplate GetCombustionProducts();

	// Phase Changes
	public abstract (float, MaterialTemplate) GetPhaseChange();

	// Combat Attributes
	public abstract float GetCompressionFactor(); // How much compressive pressure the material can hold before breaking
	public abstract float GetExpansionFactor(); // How much expansive pressure the material can hold before breaking
	public abstract float GetCohesiveness(); // Coefficient for amount of mana to lose when hitting something

	// Desired Density
	public abstract float GetDesiredDensity();
	public abstract float GetDensityCoefficient();
}