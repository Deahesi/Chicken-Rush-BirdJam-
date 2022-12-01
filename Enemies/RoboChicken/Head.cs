using Godot;
using System;

public class Head : KinematicBody2D
{
	[Signal]
	public delegate void Back();


	[Export]
	public float speed = 200;
	[Export]
	public float dist_to_attack = 10000;
	[Export]
	public float SToChangeV = 10000;
	[Export]
	private int attack_size = 400;
	[Export]
	private int hp = 100;
	[Export]
	public AudioStream hurtSound;

	Vector2 velocity = new Vector2(0, 0);
	Vector2 dirlong = Vector2.Zero;

	AnimatedSprite sprite;
	Area2D kickpos;
	Timer AttackCD;
	Timer stopTM;
	AudioStreamPlayer sounds;

	private float S = 0;
	private float Sx = 0;
	private float mody = 0;
	private float hitboxX;

	private Area2D player = new Area2D();
	private bool isPlayer = false;

	private float otkForce = 0;
	private bool tweencompleted = true;
	private bool attack = false;
	private bool damage = false;
	private bool damage_taken_otk = false;

	public bool enabled = false;
	[Export]
	public bool death = false;
	public bool back = false;
	public Vector2 backCords;

	Random rnd = new Random();
	public override void _Ready() {
		this.Visible = false;
		sprite = this.GetNode<AnimatedSprite>("AnimatedSprite");
		kickpos = this.GetNode<Area2D>("KickPos");
		AttackCD = this.GetNode<Timer>("AttackCD");
		stopTM = this.GetNode<Timer>("StopTimer");
		sounds = this.GetNode<AudioStreamPlayer>("Sounds");
		//damageParticle = this.GetNode<Particles2D>("DamageParticle");
		hitboxX = kickpos.Position.x;
	}

	float fx() {
		return 0.002f * this.GlobalPosition.y + 0.1213f;
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
			sprite.Play("Death");
			death = true;
		}
		if (death) return;
		if (!enabled || !isPlayer) return;
		else if (back) {
			velocity = this.GlobalPosition.DirectionTo(backCords).Normalized() * speed;
			if (this.GlobalPosition.DistanceTo(backCords) < 50) {
				this.EmitSignal("Back");
				back = false;
				this.Visible = false;
				enabled = false;
			}
		} else if (stopTM.IsStopped() && !damage && !attack) {
			attack = false;
			Vector2 dir;
			if (this.GlobalPosition.DistanceTo(player.GlobalPosition) <= attack_size) {
				dir = -this.GlobalPosition.DirectionTo(player.GlobalPosition);
			} else {
				dir = this.GlobalPosition.DirectionTo(player.GlobalPosition);
			}
			
			//dir.x *= rnd.Next(5, 10);
			dir.y += mody;

			velocity = dir * speed * (rnd.Next(5, 10) * 0.1f);
			
			//velocity.y *= (rnd.Next(0, 2) == 0 ? -1 : 1) * rnd.Next(1, 3);

		} else {
			if (!damage)
				velocity = Vector2.Zero;
			/*if (AttackCD.IsStopped() && !damage_taken_otk && stopTM.IsStopped()) {
				attack = true;
				sprite.Play("Attack");
			} else {
				sprite.Stop();
			}*/
		}
		if ((this.GlobalPosition.y <= player.GlobalPosition.y - 30 || this.GlobalPosition.y >= player.GlobalPosition.y + 30) && stopTM.IsStopped() && !damage && !attack && !back) {
			velocity.y = this.GlobalPosition.DirectionTo(player.GlobalPosition).y * speed;
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
		if (S >= SToChangeV && !attack) {
			S = 0;
			mody = rnd.Next(5, 10) * 0.1f * (rnd.Next(0, 2) == 0 ? -1 : 1);
			stopTM.WaitTime = rnd.Next(2, 8) * 0.1f;
			stopTM.Start();
		}
		if (Moving()) {
			sprite.Play("Walk");

			S += Mathf.Sqrt((velocity.x * velocity.x) + (velocity.y * velocity.y));
			Sx += velocity.x;

			MoveAndSlide(velocity);
		}
		if (damage) {
			//sprite.Play("TakenDamage");
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
		if (death || !enabled) return;
		tweencompleted = false;
		damage = true;
		damage_taken_otk = true;
		sprite.Stop();
		AttackCD.Start();
		hp -= dmg;
		otkForce = otk;
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
		} else if (sprite.Animation == "AttackLong") {
			AttackCD.Start();
			attack = false;
		}
	}

	public void _on_KinematicBody2D2_HeadOut(Area2D p, Vector2 pos) {
		this.GlobalPosition = pos;
		player = p;
		this.Visible = true;
		enabled = true;
		isPlayer = true;
		death = false;
		back = false;
	}
	public void _on_Visible_screen_exited() {
		if (death) {
			QueueFree();
		}
	}

	public void OnHeadIn(Vector2 pos) {
		back = true;
		backCords = pos;
	}
}
