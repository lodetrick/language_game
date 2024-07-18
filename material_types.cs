public static class Material {
	class Reaction {
		Types result; // Resultant Type
		ushort ratioL; // Ratio of lower number
		ushort ratioH; // Ratio of higher number
		ushort ratioR; // Ratio of resultant
	}

	public enum Types : ushort {
		None = 0x0000,
		Water = 0x0001,
		Fire = 0x0002,
		Earth = 0x0004,
		Air = 0x0008,
		Glass = 0x0010,
		Oil = 0x0020,
		Wood = 0x0040,
		Smoke = 0x0080,
		Steam = 0x0100,
		Iron = 0x0200,
		Dust = 0x0400,
		Clay = 0x0800,
		Gold = 0x1000,
		Flesh = 0x2000,
		Ice = 0x4000,
		Slag = 0x8000,
	}

	
}