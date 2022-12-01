using Godot;
using System;

public class Wave : Area2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.

    public bool attack = false;
    AnimatedSprite sprite;

	public delegate void Callback();
	void DoInFrames(int frame1, int frame2, Callback callback) {
		if (sprite.Frame >= frame1 && sprite.Frame <= frame2) {
			callback();
		}
	}
	public override void _Ready()
    {
		this.Visible = false;
        sprite = this.GetNode<AnimatedSprite>("AnimatedSprite");
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (attack) {
			sprite.Play("default");
			void f() {
				this.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
			}
			DoInFrames(3, 3, f);
        }
    }
	public void Attack() {
		attack = true;
	}


    public void _on_Area2D_area_entered(Area2D area) {
		if (area.IsInGroup("hurtbox")) {
			//	sprite.SpeedScale -= 0.5f;
			area.GetParent().Call("TakeDamage", 10, 4000);
		}
	}
	public void _on_AnimatedSprite_animation_finished() {
        this.attack = false;
		this.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
        this.Visible = false;
		sprite.Stop();
	}
}
