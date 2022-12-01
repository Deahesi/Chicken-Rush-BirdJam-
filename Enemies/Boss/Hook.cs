using Godot;
using System;

public class Hook : KinematicBody2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	[Export]
	Vector2 velocity = new Vector2(1, 0);

	[Export]
	float speed = 1000;
	[Export]
	float hookDistance = 1000;

	float Sx = 0;
	bool back = false;

	Sprite sprite;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		sprite = this.GetNode<Sprite>("Sprite");
	}

	public void _on_Area2D_area_entered(Area2D area) {
		if (area.IsInGroup("PlayerHurt")) {
			area.GetParent().Call("TakeDamage", 10, sprite.FlipH ? -500 : 500);
		}
	}
	public override void _PhysicsProcess(float delta) {
		var collision = MoveAndCollide(velocity.Normalized() * speed * delta);
		if (collision != null) {
			this.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
		}
		if (!back) {
			Sx += Mathf.Abs((velocity.Normalized() * speed * delta).x);
		} else {
			Sx -= Mathf.Abs((velocity.Normalized() * speed * delta).x);
		}
		if (Sx >= hookDistance) {
			velocity.x *= -1;
			back = true;
		} else if (!back) {
			if (velocity.x < 0) {
				sprite.FlipH = true;
			} else {
				sprite.FlipH = false;
			}
		}

		try {
			if (Sx < 0) {
				QueueFree();
			}
		} catch {
		}
	}
}
