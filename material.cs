using Godot;

public static class Material {
	public class Reaction {
		public Types result; // Resultant Type
		public ushort ratioL; // Ratio of lower number
		public ushort ratioH; // Ratio of higher number
		public ushort ratioR; // Ratio of resultant

		public Reaction() {
			result = Types.None;
			ratioL = ratioH = ratioR = 0;
		}

		public Reaction(Types type, ushort L, ushort H, ushort R) {
			result = type;
			ratioL = L;
			ratioH = H;
			ratioR = R;
		}
	}

	public enum Types : ushort {
		Water,
		Fire,
		Earth,
		Air,
		Glass,
		Oil,
		Wood,
		Smoke,
		Steam,
		Iron,
		Dust,
		Clay,
		Gold,
		Flesh,
		Ice,
		Slag,
		None,
	}

	static Reaction[,] reactions = new Reaction[16,16];

	static Material() {
		for (int i = 0; i < 16; i++) {
			for (int j = 0; j < 16; j++) {
				reactions[i,j] = new Reaction();
			}
		}
		reactions[0,1] = new Reaction(Types.Steam, 1, 2, 3); // Water and Fire produce Steam
	}

	public static Reaction GetReaction(Types lhs, Types rhs) {
		return reactions[Mathf.Min((int)lhs, (int)rhs),Mathf.Max((int)lhs, (int)rhs)];
	}

	public abstract class MaterialTemplate {
		public InformationBody _information { get; set; } // THIS BETTER BE A POINTER
		public Material.Types MATERIAL_ID { get; private set; } // Identifying number, unique for each material type

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
}