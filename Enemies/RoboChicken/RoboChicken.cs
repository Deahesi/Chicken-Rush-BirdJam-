using Godot;
using System;

public class RoboChicken : KinematicBody2D
{
	[Signal]
	public delegate void HeadOut(Area2D p, Vector2 pos);

	[Signal]
	public delegate void HeadIn(Vector2 pos);

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
	Vector2 dirlong = Vector2.Zero;

	AnimatedSprite sprite;
	Area2D kickpos;
	VisibilityNotifier2D visibility;
	Timer AttackCD;
	Timer stopTM;
	Particles2D damageParticle;
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

	public bool enabled = true;
	[Export]
	public bool death = false;
	public bool head = false;
	public bool end_head_an = false;
	public bool longattack = false;
	public bool reverse = false;

	Random rnd = new Random();
	public void _on_PlayerEnter_area_entered(Area2D body2D) {
		if (body2D.IsInGroup("PlayerHurt")) {
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
		sounds = this.GetNode<AudioStreamPlayer>("Sounds");
		hitboxX = kickpos.Position.x;
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


	float fx() {
		return 0.002f * this.GlobalPosition.y + 0.1213f;
	}
	public override void _Process(float delta) {
		//if (!enabled && !isPlayer) return;
		if (death) {
			sprite.Play("Death");
			return;
		}
		if (head) { return; }
		if (longattack) return;
		if (isPlayer) {
			if (this.GlobalPosition.DirectionTo(player.GlobalPosition).x > 0) {
				if (reverse) {
					reverse = false;
					this.SetTransform(new Transform2D(new Vector2(1, 0), new Vector2(0, 1), this.GlobalPosition));
				}

			} else if (this.GlobalPosition.DirectionTo(player.GlobalPosition).x < 0) {
				if (!reverse) {
					reverse = true;
					this.SetTransform(new Transform2D(new Vector2(-1, 0), new Vector2(0, 1), this.GlobalPosition));
				}

			}
		} else {
			this.SetTransform(new Transform2D(new Vector2(-1, 0), new Vector2(0, 1), this.GlobalPosition));
		}
	}


	public delegate void Callback();
	void DoInFrames(int frame1, int frame2, Callback callback) {
		if (sprite.Frame >= frame1 && sprite.Frame <= frame2) {
			callback();
		}
	}

	void DoInFrames(int frame1, int frame2, Callback callback, Callback elseCall) {
		if (sprite.Frame >= frame1 && sprite.Frame <= frame2) {
			callback();
		} else {
			elseCall();
		}
	}

	public override void _PhysicsProcess(float delta) {
		if (hp <= 0) {
			head = true;
			if (!end_head_an)
				sprite.Play("HeadOut");
		}
		if (death || head) return;
		if (!enabled || !isPlayer) return;
		else if ((this.GlobalPosition.DistanceTo(player.GlobalPosition) >= attack_size && this.GlobalPosition.DistanceTo(player.GlobalPosition) <= dist_to_attack && stopTM.IsStopped() && AttackCD.IsStopped() && !damage && !attack) || longattack) {
			if (!longattack) {
				Sx = 0;
				longattack = true;
				dirlong = this.GlobalPosition.DirectionTo(player.GlobalPosition);
			}

			void Run() {
				if (!sounds.Playing) {
					sounds.Stream = (attackSound);
					sounds.Play();
				}
				velocity = dirlong * speed * 2; 
			}
			void Stop() { 
				velocity = Vector2.Zero; 
			}
			DoInFrames(7, 13, Run, Stop);
			sprite.Play("AttackLong");
			AttackBeetweenFrames(9, 12);
			if (-Mathf.Abs(Sx) >= dist_to_attack) {
				longattack = false;
			}
		} else if (this.GlobalPosition.DistanceTo(player.GlobalPosition) >= attack_size && stopTM.IsStopped() && !damage && !attack && !longattack) {
			attack = false;
			Vector2 dir = this.GlobalPosition.DirectionTo(player.GlobalPosition);
			dir.y += mody;
			velocity = dir * speed * (rnd.Next(5, 10) * 0.1f);

		} else {
			if (!damage)
				velocity = Vector2.Zero;
			if (AttackCD.IsStopped() && !damage_taken_otk && stopTM.IsStopped() && !longattack) {
				attack = true;
				if (!sounds.Playing) {
					sounds.Stream = (attackSound);
					sounds.Play();
				}
				sprite.Play("Attack");
				AttackBeetweenFrames(attackf1, attackf2);
			} else if (sprite.Animation != "Walk") {
				sprite.Stop();
			}
		}
		if (this.GlobalPosition.y <= player.GlobalPosition.y - 30 || this.GlobalPosition.y >= player.GlobalPosition.y + 30 && stopTM.IsStopped() && !damage && !attack) {
			//velocity.y = this.GlobalPosition.DirectionTo(player.GlobalPosition).y * speed;
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
		if (S >= SToChangeV && !longattack && !attack) {
			S = 0;
			mody = rnd.Next(5, 10) * 0.1f * (rnd.Next(0, 2) == 0 ? -1 : 1);
			stopTM.WaitTime = rnd.Next(2, 8) * 0.1f;
			stopTM.Start();
		}
		if (Moving()) {
			if (!longattack)
				sprite.Play("Walk");

			S += Mathf.Sqrt((velocity.x * velocity.x) + (velocity.y * velocity.y));
			Sx += velocity.x;
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
		if (death || head) return;
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
		sprite.Play("Walk");
		if (hp > 0) {
			sounds.Stream = (hurtSound);
			sounds.Play();
		}
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
			longattack = false;
		} else if (sprite.Animation == "HeadOut") {
			end_head_an = true;
			Vector2 position = new Vector2(reverse ? this.GlobalPosition.x + 76 : this.GlobalPosition.x - 76, this.GlobalPosition.y + 25);
			this.EmitSignal("HeadOut", player, position);
			this.GetNode<Timer>("HeadTimer").Start();
			sprite.Play("WithoutHead");
		} else if (sprite.Animation == "HeadBack") {
			end_head_an = false;
			head = false;
			hp = 100;
		}
	}

	public void _on_HeadTimer_timeout() {
		Vector2 position = new Vector2(reverse ? this.GlobalPosition.x - 76 : this.GlobalPosition.x + 76, this.GlobalPosition.y + 25);
		this.EmitSignal("HeadIn", position);
	}

	public void _on_Visible_screen_exited() {
		if (death || (head && this.GetNode<Timer>("HeadTimer").IsStopped())) {
			QueueFree();
		}
	}

	public void OnBack() {
		sprite.Play("HeadBack");
	}
}
