using Godot;
using System;
using System.Drawing;

public class Bullet : KinematicBody2D
{
	[Export]
	Vector2 velocity = new Vector2(1, 0);

    [Export]
    float speed = 2000;

	AnimatedSprite sprite;

	bool free = false;


    public override void _Ready()
    {
		sprite = this.GetNode<AnimatedSprite>("AnimatedSprite");

	}
	public override void _PhysicsProcess(float delta) {
		if (free) {
			QueueFree();
			return;
		}
		var collision = MoveAndCollide(velocity.Normalized() * speed * delta);
		try {
			if ((collision != null || !this.GetNode<VisibilityNotifier2D>("VisibilityNotifier2D").IsOnScreen()) && !((Node2D)collision.Collider).IsInGroup("Player")) {
				if (((Node2D)collision.Collider).HasMethod("TakeDamage"))
					((Node2D)collision.Collider).Call("TakeDamage", 200, 100);
				this.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
				sprite.Play("Collided");
				velocity = Vector2.Zero;
			}
		} catch {
		}
	}

	public void _on_AnimatedSprite_animation_finished() {
		if (sprite.Animation == "Collided")
			QueueFree();
	}

	public void OnVisibilityNotifier2DScreenExited() {
		free = true;
	}
}
