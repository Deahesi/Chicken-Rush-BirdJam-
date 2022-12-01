using Godot;
using Godot.Collections;
using System.Reflection.Emit;

public class Objects : Node
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    Array groups;
	public override void _Ready()
    {
		Node objects = this.GetNode<Node>("Objects");
		groups = objects.GetChildren();
    }

    public override void _Process(float delta) {
		for (int i = 0; i < groups.Count; i++) {
            Node2D group = (Node2D)groups[i];
			if (!IsInstanceValid(group)) {
				groups.Remove(group);
				continue;
			}
			Array layers = group.GetChildren();
			for (int j = 0; j < layers.Count; j++) {
				CanvasLayer layer = (CanvasLayer)layers[j];
				if (layer.GetChildOrNull<Node2D>(0) == null) {
					layers.Remove(layer);
					layer.QueueFree();
					continue;
				}
				layer.SetLayer(Mathf.Abs(Mathf.FloorToInt(layer.GetChild<Node2D>(0).GlobalPosition.y)));
			}
		}
	}

	public void UpdateObj() {
		Node objects = this.GetNode<Node>("Objects");
		groups = objects.GetChildren();
	}
}
