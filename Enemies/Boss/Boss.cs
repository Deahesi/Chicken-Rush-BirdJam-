using Godot;
using System;

enum BossAttacks {
	STANDART, LASER, HOOK
}
public class Boss : KinematicBody2D
{
	[Signal]
	public delegate void Dead();

	[Export]
	public float speed = 350;
	[Export]
	public float SToChangeV = 10000;
	[Export]
	private int attack_size = 400;
	[Export]
	private int laser_attack_size = 1000;
	[Export]
	private int hp = 2000;
	[Export]
	private int attack_damage = 100;
	[Export]
	public AudioStream attackSound;
	[Export]
	public AudioStream laserSound;
	[Export]
	public AudioStream hookSound;
	[Export]
	public AudioStream hurtSound;
	[Export]
	public AudioStream hurtSoundDamage;

	Vector2 velocity = new Vector2(0, 0);

	AnimatedSprite sprite;
	Area2D kickpos;
	VisibilityNotifier2D visibility;
	Timer AttackCD;
	Timer stopTM;
	Timer damageTimer;
	Timer damageDealTimer;
	Particles2D damageParticle;
	AudioStreamPlayer sounds;
	PackedScene hook;

	private float S = 0;
	private float mody = 0;
	private float hitboxX;

	private Area2D player = new Area2D();
	private bool isPlayer = false;

	private float otkForce = 0;
	private bool tweencompleted = true;
	private bool attack = false;
	private BossAttacks attack_type;
	private bool damage = false;
	private bool damage_taken_otk = false;
	private bool cantakedamage = false;
	private bool cantakedamageafter = false;
	private bool shot = false;
	[Export]
	public bool cutscene = false;
	public bool enabled = false;
	[Export]
	public bool death = false;
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
		damageTimer = this.GetNode<Timer>("DamageTimer");
		damageDealTimer = this.GetNode<Timer>("DamageDealTimer");
		damageParticle = this.GetNode<Particles2D>("DamageParticle");
		sounds = this.GetNode<AudioStreamPlayer>("Sounds");
		hook = (PackedScene)ResourceLoader.Load("res://Enemies/Boss/Hook.tscn");
		hitboxX = kickpos.Position.x;
		this.SetTransform(new Transform2D(new Vector2(-1, 0), new Vector2(0, 1), this.GlobalPosition));
	}

	float fx() {
		return 0.002f * this.GlobalPosition.y + 0.1213f;
	}

	public void KickAreaEntered(Area2D area) {
		if (area.IsInGroup("Player")) {
			area.GetParent().Call("TakeDamage", attack_damage, 100);
		}
	}
	void AttackBeetweenFrames(int frame1, int frame2) {
		if (sprite.Frame >= frame1 && sprite.Frame <= frame2) {
			kickpos.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
		} else {
			kickpos.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
		}
	}

	public delegate void Callback();
	void DoInFrames(int frame1, int frame2, Callback callback) {
		if (sprite.Frame >= frame1 && sprite.Frame <= frame2) {
			callback();
		}
	}

	public override void _Process(float delta) {
		//if (!enabled && !isPlayer) return;
		if (cutscene) {
			this.Visible = false;
			return;
		} else if (!this.Visible) {
			this.Visible = true;
			enabled = true;
			damageTimer.Start();
		}
		if (death && sprite.Animation != "DeathLoop") {
			sprite.Play("Death");
			return;
		} else if (death || sprite.Animation == "Stun") {
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

	void AttackBeetweenFrames(int[] frame1, int[] frame2) {
		for (int i = 0; i < frame1.Length; i++) {
			if (sprite.Frame >= frame1[i] && sprite.Frame <= frame2[i]) {
				if (attack_type == BossAttacks.STANDART)
					kickpos.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
				else if (attack_type == BossAttacks.LASER)
					kickpos.GetNode<CollisionShape2D>("CollisionShape2D2").Disabled = false;
				break;
			} else {
				kickpos.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
				kickpos.GetNode<CollisionShape2D>("CollisionShape2D2").Disabled = true;
				//kickpos.GetNode<CollisionShape2D>("CollisionShape2D3").Disabled = true;
			}
		}
	}

	public override void _PhysicsProcess(float delta) {
		if (hp <= 0) {
			death = true;
		}
		if (death) return;
		if (!enabled || !isPlayer) return;
		if (damageTimer.IsStopped()) {
			velocity = Vector2.Zero;
			if (!cantakedamage && !cantakedamageafter) {
				kickpos.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
				kickpos.GetNode<CollisionShape2D>("CollisionShape2D2").Disabled = true;
				sprite.Play("PreStun");
			}
			else if (damageDealTimer.IsStopped()) {
				cantakedamage = false;
				cantakedamageafter = true;
				sprite.Play("AfterStun");
			}
		} else if (attack) {
			velocity = Vector2.Zero;
			switch (attack_type) {
				case BossAttacks.STANDART:
					sprite.Play("Attack");
					AttackBeetweenFrames(2, 3);
					if (!sounds.Playing && sprite.Frame >= 2) {
						sounds.Stream = (attackSound);
						sounds.Play();
					}
					break;
				case BossAttacks.LASER:
					sprite.Play("Laser");
					sounds.Stream = (laserSound);
					sounds.Play();
					AttackBeetweenFrames(new int[] { 4, 5, 6, 7 }, new int[] { 4, 5, 6, 7 });
					break;
				case BossAttacks.HOOK:
					sprite.Play("Hook");
					sounds.Stream = (hookSound);
					void hookSpawn() {
						if (!shot) {
							CanvasLayer h = hook.Instance<CanvasLayer>();
							KinematicBody2D hook_inst = h.GetChild<KinematicBody2D>(0);
							this.GetParent().GetParent().AddChild(h);

							hook_inst.GlobalPosition = this.GetNode<Position2D>("ShootPos").GlobalPosition;
							hook_inst.Set("velocity", this.GlobalPosition.DirectionTo(player.GlobalPosition).x > 0 ? new Vector2(1, 0) : new Vector2(-1, 0));
							this.GetParent().GetParent().GetParent().GetParent().Call("UpdateObj");
							shot = true;
						}
					}
					DoInFrames(2, 2, hookSpawn);
					sounds.Play();
					break;
			}
		} else if (this.GlobalPosition.DistanceTo(player.GlobalPosition) <= laser_attack_size && this.GlobalPosition.DistanceTo(player.GlobalPosition) >= attack_size && AttackCD.IsStopped() && !damage_taken_otk && stopTM.IsStopped() && !attack) {
			int rand = rnd.Next(-14, 5);
			if (rand == 1) {
				attack = true;
				attack_type = BossAttacks.LASER;
			} else if (rand == 2) {
				attack = true;
				attack_type = BossAttacks.HOOK;
			}
		} else if (this.GlobalPosition.DistanceTo(player.GlobalPosition) >= attack_size && stopTM.IsStopped() && !damage && !attack) {
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
				attack_type = BossAttacks.STANDART;

			} else if (sprite.Animation != "Stun") {
				sprite.Play("Walk");
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
		return (velocity.x > 0 || velocity.x < 0 || velocity.y > 0 || velocity.y < 0) && sprite.Animation != "Stun";
	}

	public void TakeDamage(int dmg, int otk = 0) {
		if (death) return;
		sounds.Stream = (hurtSound);
		sounds.Play();
		if (sprite.Animation != "Stun") return;
		sounds.Stop();
		if (!damage) {
			damageParticle.Emitting = true;
		}
		tweencompleted = false;
		damage = true;
		damage_taken_otk = true;
		AttackCD.Start();
		hp -= dmg;
		otkForce = otk;
		if (hp <= 0) {
			Vector2 dir = this.GlobalPosition.DirectionTo(player.GlobalPosition).Normalized();
			velocity.x = (1000 * -dir.x) - (1000 * 0.2f * dir.x);
		}
		sounds.Stream = (hurtSoundDamage);
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
		} else if (sprite.Animation == "Laser") {
			AttackCD.Start();
			attack = false;
			sounds.Stop();
		} else if (sprite.Animation == "AfterStun") {
			damageTimer.Start();
			cantakedamageafter = false;
		} else if (sprite.Animation == "PreStun") {
			damageDealTimer.Start();
			cantakedamage = true;
			sprite.Play("Stun");
		} else if (sprite.Animation == "Death") {
			EmitSignal("Dead");
			sprite.Play("DeathLoop");
		} else if (sprite.Animation == "Hook") {
			AttackCD.Start();
			attack = false;
			shot = false;
			sounds.Stop();
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
