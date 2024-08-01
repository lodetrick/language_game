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
		Stone,
		Gold,
		Flesh,
		Ice,
		Slag,
		Plant,
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
		public static Material.Types MATERIAL_ID { get; private set; } // Identifying number, unique for each material type

		public Material.Types GetId() => MATERIAL_ID;
		
		private float _manaTotal;

		// Flammability
		public float GetCombustionManaNeeded() { return 1000; } // When the material starts to burn, total mana (not percentage)
		public MaterialTemplate GetCombustionProducts() { return new Water(); }

		// Phase Changes
		// public abstract (float, MaterialTemplate) GetPhaseChange();

		// Combat Attributes
		public float GetCompressionFactor() { return 10; } // How much compressive pressure the material can hold before breaking
		public float GetExpansionFactor() { return 10; } // How much expansive pressure the material can hold before breaking
		public float GetCohesiveness() { return 10; } // Coefficient for amount of mana to lose when hitting something

		// Desired Density
		public float GetDesiredDensity() { return 1.0f; }
		public float GetDensityCoefficient() { return 1.0f; }
	}

    public class Water : MaterialTemplate
    {
        // Flammability
        // public override float GetCombustionManaNeeded() {} // When the material starts to burn, total mana (not percentage)
		// public override MaterialTemplate GetCombustionProducts() {}

		// // Phase Changes
		// // public override (float, MaterialTemplate) GetPhaseChange() {}

		// // Combat Attributes
		// public override float GetCompressionFactor() {} // How much compressive pressure the material can hold before breaking
		// public override float GetExpansionFactor() {} // How much expansive pressure the material can hold before breaking
		// public override float GetCohesiveness() {} // Coefficient for amount of mana to lose when hitting something

		// // Desired Density
		// public override float GetDesiredDensity() {}
		// public override float GetDensityCoefficient() {}
    }
}