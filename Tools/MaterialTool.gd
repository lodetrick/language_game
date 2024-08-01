extends Node

func _ready():
	# call all the stuff
	# list of classes
	var materials: String = \
"Water,Fire,Earth,Air,Glass,Oil,Wood,Smoke,Steam,Iron,Dust,Stone,Gold,Flesh,Ice,Slag,Plant"
	#WriteMaterialClass(materials)
	PrintMaterialStatics(materials)

func PrintMaterialStatics(materials: String):
	var materialList = materials.split(",")
	var string = ""
	for i in range(len(materialList)):
		string += "materialStates[%s] = new %sMat();\n" % [i, materialList[i]]
	print(string)

func WriteMaterialClass(materials: String):
	var materialList = materials.split(",")
	var file = FileAccess.open("res://Engine/Materials.cs", FileAccess.WRITE)
	var string = ""
	for material in materialList:
		string += \
"""public class %sMat : MaterialState
{
	public override void ApplyHeat(ref Pixel p, float amount)
	{
		
	}
	
	public override void TouchMaterial(ref Pixel p, ref Pixel other)
	{
		
	}
}

""" % [material]
	
	file.store_string(string)
