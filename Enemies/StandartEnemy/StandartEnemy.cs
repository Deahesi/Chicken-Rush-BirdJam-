using Godot;
using System;
using System.Drawing;

public class StandartEnemy : KinematicBody2D
{
	[Export]
	public float speed = 350;
	[Export]
	public float SToChangeV = 10000;
	[Export]
	private int attack_size = 400;
	[Export]
	private int hp = 100;
	[Export]
	private int attack_damage = 50;
	[Export]
	private int otk = 0;
	[Export]
	private byte attackf1 = 0;
	[Export]
	private byte attackf2 = 0;
	[Export]
	public AudioStream attackSound;
	[Export]
	public AudioStream hurtSound;

	Vector2 velocity = new Vector2(0, 0);

	AnimatedSprite sprite;
	Area2D kickpos;
	VisibilityNotifier2D visibility;
	Timer AttackCD;
	Timer stopTM;
	Particles2D damageParticle;
	AudioStreamPlayer sounds;

	private float S = 0;
	private float mody = 0;
	private float hitboxX;

	private Area2D player = new Area2D();
	private bool isPlayer = false;

	private float otkForce = 0;
	private bool tweencompleted = true;
	private bool attack = false;
	private bool damage = false;
	private bool damage_taken_otk = false;

	public bool enabled = true;
	[Export]
	public bool death = false;
	Random rnd = new Random();
	public void _on_PlayerEnter_area_entered(Area2D body2D) {
		if (body2D.IsInGroup("PlayerHurt")) {
			player = body2D;
			isPlayer = true;
		}
	}

	public override void _Ready()
    {
		sprite = this.GetNode<AnimatedSprite>("AnimatedSprite");
		kickpos = this.GetNode<Area2D>("KickPos");
		visibility = this.GetNode<VisibilityNotifier2D>("Visible");
		AttackCD = this.GetNode<Timer>("AttackCD");
		stopTM = this.GetNode<Timer>("StopTimer");
		damageParticle = this.GetNode<Particles2D>("DamageParticle");
		sounds = this.GetNode<AudioStreamPlayer>("Sounds");
		hitboxX = kickpos.Position.x;
	}

	float fx() {
		return 0.002f * this.GlobalPosition.y + 0.1213f;
	}

	public void KickAreaEntered(Area2D area) {
		if (area.IsInGroup("Player")) {
			area.GetParent().Call("TakeDamage", attack_damage, otk);
		}
	}
	void AttackBeetweenFrames(int frame1, int frame2) {
		if (sprite.Frame >= frame1 && sprite.Frame <= frame2) {
			kickpos.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
		} else {
			kickpos.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
		}
	}

	public override void _Process(float delta) {
		//if (!enabled && !isPlayer) return;
		if (death) {
			sprite.Play("Death");
			return;
		}
		if (isPlayer) {
			if (this.GlobalPosition.DirectionTo(player.GlobalPosition).x > 0) {
				this.SetTransform(new Transform2D(new Vector2(1, 0), new Vector2(0, 1), this.GlobalPosition));
			} else if (this.GlobalPosition.DirectionTo(player.GlobalPosition).x < 0) {
				this.SetTransform(new Transform2D(new Vector2(-1, 0), new Vector2(0, 1), this.GlobalPosition));
			}
		} else {
			this.SetTransform(new Transform2D(new Vector2(-1, 0), new Vector2(0, 1), this.GlobalPosition));
		}

	}

	public override void _PhysicsProcess(float delta) {
		if (hp <= 0) {
			death = true;
		}
		if (death) return;
		if (!enabled || !isPlayer) return;
		else if (this.GlobalPosition.DistanceTo(player.GlobalPosition) >= attack_size && stopTM.IsStopped() && !damage && !attack) {
			if (rnd.Next(0, 2) == 1 || true) {
				Vector2 dir = this.GlobalPosition.DirectionTo(player.GlobalPosition);
				//dir.x *= rnd.Next(5, 10);
				dir.y += mody;

				velocity = dir * speed * (rnd.Next(5, 10) * 0.1f);
				//velocity.y *= (rnd.Next(0, 2) == 0 ? -1 : 1) * rnd.Next(1, 3);
			} else {
				velocity = Vector2.Zero;
			}

		} else if (this.GlobalPosition.y <= player.GlobalPosition.y - 30 || this.GlobalPosition.y >= player.GlobalPosition.y + 30 && stopTM.IsStopped() && !damage && !attack) {
			velocity = new Vector2(0, this.GlobalPosition.DirectionTo(player.GlobalPosition).y * speed);
		} else {
			if (!damage)
				velocity = Vector2.Zero;
			if (AttackCD.IsStopped() && !damage_taken_otk && stopTM.IsStopped()) {
				attack = true;
				
				sprite.Play("Attack");
				AttackBeetweenFrames(attackf1, attackf2);
				if (!sounds.Playing && sprite.Frame >= 4) {
					sounds.Stream = (attackSound);
					sounds.Play();
				}
					
			} else if (sprite.Animation != "TakenDamage") {
				sprite.Stop();
			}
		}

		if (damage_taken_otk) {
			Vector2 dir = this.GlobalPosition.DirectionTo(player.GlobalPosition).Normalized();
			velocity.x = otkForce * -dir.x;
			damage_taken_otk = false;
		}
		if (damage) {
			Vector2 dir = this.GlobalPosition.DirectionTo(player.GlobalPosition).Normalized();
			//velocity = Vector2.Zero;
			velocity.x -= otkForce * 0.2f * dir.x;
			//velocity.y = -otkForce * delta;
			damage_taken_otk = false;
		}
		if (S >= SToChangeV) {
			S = 0;
			mody = rnd.Next(5, 10) * 0.1f * (rnd.Next(0, 2) == 0 ? -1 : 1);
			stopTM.WaitTime = rnd.Next(2, 8) * 0.1f;
			stopTM.Start();
		}
		if (Moving()) {
			sprite.Play("Walk");
			S += Mathf.Sqrt((velocity.x * velocity.x) + (velocity.y * velocity.y));
			MoveAndSlide(velocity);
		}
		if (damage) {
			sprite.Play("TakenDamage");
		}


		if (damage) {
			if (sprite.Modulate.r >= 40) {
				tweencompleted = true;
			}
			if (sprite.Modulate.r <= 1.1f && tweencompleted) {
				damage = false;
			}

			if (!tweencompleted) {
				sprite.Modulate = sprite.Modulate.LinearInterpolate(new Godot.Color(250, 250, 250), 0.1f);
			} else {
				sprite.Modulate = sprite.Modulate.LinearInterpolate(new Godot.Color(1, 1, 1), 1f);
			}
		}
	}

	private bool Moving() {
		return velocity.x > 0 || velocity.x < 0 || velocity.y > 0 || velocity.y < 0;
	}

	public void TakeDamage(int dmg, int otk = 0) {
		if (death) return;
		if (!damage) {
			damageParticle.Emitting = true;
		}
		tweencompleted = false;
		damage = true;
		damage_taken_otk = true;
		AttackCD.Start();
		sprite.Play("TakenDamage");
		hp -= dmg;
		otkForce = otk;
		if (hp <= 0) {
			Vector2 dir = this.GlobalPosition.DirectionTo(player.GlobalPosition).Normalized();
			velocity.x = (1000 * -dir.x) - (1000 * 0.2f * dir.x);
		}
		sounds.Stream = (hurtSound);
		sounds.Play();
	}

	public void _on_DamageTween_tween_all_completed() {
		tweencompleted = true;
	}

	public void _on_AnimatedSprite_animation_finished() {
		sounds.Stop();
		if (sprite.Animation == "Attack") {
			AttackCD.Start();
			attack = false;
		} else if (sprite.Animation == "TakenDamage") {

		}
		
	}

	public void _on_Visible_screen_exited() {
		if (death) {
			QueueFree();
		}
	}
	void _on_Sounds_finished() {
		sounds.Stop();
	}
}
