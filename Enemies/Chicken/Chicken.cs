using Godot;
using System;

public class Chicken : KinematicBody2D
{
	[Export]
	public float speed = 350;
	[Export]
	public float SToChangeV = 10000;
	[Export]
	private int attack_size = 100;
	[Export]
	private int hp = 100;

	Vector2 velocity = new Vector2(0, 0);

	AnimatedSprite sprite;
	Area2D kickpos;
	VisibilityNotifier2D visibility;
	Timer AttackCD;
	Timer stopTM;
	Particles2D damageParticle;

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
		if (body2D.IsInGroup("Player")) {
			player = body2D;
			isPlayer = true;
		}
	}

	public override void _Ready() {
		sprite = this.GetNode<AnimatedSprite>("AnimatedSprite");
		kickpos = this.GetNode<Area2D>("KickPos");
		visibility = this.GetNode<VisibilityNotifier2D>("Visible");
		AttackCD = this.GetNode<Timer>("AttackCD");
		stopTM = this.GetNode<Timer>("StopTimer");
		damageParticle = this.GetNode<Particles2D>("DamageParticle");
		hitboxX = kickpos.Position.x;
	}

	float fx() {
		return 0.002f * this.GlobalPosition.y + 0.1213f;
	}
	public override void _Process(float delta) {
		//if (!enabled && !isPlayer) return;
		if (death) {
			if (rnd.Next(1, 11) == 1) {
				sprite.Play("SecretDeath");
			} else {
				if (otkForce > 100)
					sprite.Play("Death2");
				else
					sprite.Play("Death");
			}
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
			} else {
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
		} else if (!attack && !damage) {
			sprite.Play("Idle");
		}


		if (damage) {
			sprite.Play("TakenDamage");
		}

		if (hp <= 0) {
			death = true;
			return;
		}


		if (damage) {
			if (sprite.Modulate.r >= 40) {
				tweencompleted = true;
			}
			if (sprite.Modulate.r <= 1.1f && tweencompleted) {
				damage = false;
			}

			if (!tweencompleted) {
				sprite.Modulate = sprite.Modulate.LinearInterpolate(new Color(250, 250, 250), 0.1f);
			} else {
				sprite.Modulate = sprite.Modulate.LinearInterpolate(new Color(1, 1, 1), 1f);
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
		sprite.Stop();
		AttackCD.Start();
		hp -= dmg;
		otkForce = otk;
	}

	public void _on_DamageTween_tween_all_completed() {
		tweencompleted = true;
	}

	public void _on_AnimatedSprite_animation_finished() {
		if (sprite.Animation == "Attack") {
			AttackCD.Start();
			attack = false;
		}
	}

	public void _on_Visible_screen_exited() {
		if (death) {
			QueueFree();
		}
	}
}
