extends Camera2D

func _physics_process(delta):
	var dir = Input.get_vector("left","right","up","down")
	position += delta * 100 * dir;
