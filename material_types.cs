using System;

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
		return reactions[Math.Min((int)lhs, (int)rhs),Math.Max((int)lhs, (int)rhs)];
	}
}