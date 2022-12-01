using Godot;
using System;
using System.IO;

enum STATE {
	IDLE, WALK, ATTACK, JUMP
}

enum ATTACK_TYPE {
	STANDART, STRONG, SHOT, WAVE, PRESHOT
}



public class Player : KinematicBody2D
{
	[Export]
	public float speed = 400;
	[Export]
	public bool cutsceneStart = true;
	[Export]
	public int shakeAmount = 3;
	[Export]
	public float hp = 100;
	[Export]
	public AudioStream jumpSound;
	[Export]
	public AudioStream attackSound;
	[Export]
	public AudioStream waveSound;
	[Export] 
	public AudioStream shootSound;
	[Export]
	public AudioStream deathSound;

	Vector2 velocity = new Vector2(0, 0);
	//private bool jump = false;
	//private bool prejump = false;
	//private bool acceleratejump = false;

	private AnimatedSprite sprite;

	private Area2D kickpos;
	private Camera2D cam;
	private Timer timer;
	private Timer strongtimer;
	private Timer shaketm;
	private Timer deathtm;
	private Timer wavecd;
	private PackedScene bullet;
	private AudioStreamPlayer sounds;

	private float currY;
	private float hitboxX;
	private float hitboxShootX;
	private STATE state;
	private ATTACK_TYPE attack_type;
	bool shot = false;
	public bool stairs = false;

	private int pushes = 0;

	public bool cutscene = false;
	private Random rnd = new Random();
	private Node global_hp;
	private float otk = 0;
	

	public override void _Ready() {
		sprite = this.GetNode<AnimatedSprite>("AnimatedSprite");
		timer = this.GetNode<Timer>("Kick3Timer");
		strongtimer = this.GetNode<Timer>("StrongTimer");
		wavecd = this.GetNode<Timer>("WaveCD");
		shaketm = this.GetNode<Timer>("Shake");
		deathtm = this.GetNode<Timer>("Death");
		kickpos = this.GetNode<Area2D>("KickPos");
		cam = this.GetNode<Camera2D>("Camera2D");
		sounds = this.GetNode<AudioStreamPlayer>("Sounds");
		bullet = (PackedScene)ResourceLoader.Load("res://Player/Bullet.tscn");
		hitboxX = kickpos.Position.x;
		hitboxShootX = this.GetNode<Position2D>("ShootPos").Position.x;
		global_hp = GetNode<Node>("/root/Globals");
		hp = (float)global_hp.Get("hp");
		
		cutscene = cutsceneStart;
	}

	private int AttackDamage(ATTACK_TYPE type) {
		switch (type) {
			case ATTACK_TYPE.STANDART:
				return 50;
			case ATTACK_TYPE.STRONG:
				return 120;
			default:
				return 0;
		}
	}

	private int AttackOtk(ATTACK_TYPE type, int p) {
		if (type == ATTACK_TYPE.STANDART && p >= 2) {
			if (sprite.Frame >= 9 && sprite.Frame <= 9) {
				cam.SetOffset(new Vector2(rnd.Next(-10, 11) / 10.0f * shakeAmount, rnd.Next(-10, 11) / 10.0f * shakeAmount));
				return 2000;
			} else {
				return 100;
			}
		} else if (type == ATTACK_TYPE.STRONG && p >= 1) {
			if (sprite.Frame >= 11 && sprite.Frame <= 12) {
				cam.SetOffset(new Vector2(rnd.Next(-10, 11) / 10.0f * shakeAmount, rnd.Next(-10, 11) / 10.0f * shakeAmount));
				return 2000;
			} else {
				return 100;
			}
		} else {
			return 100;
		}
	}

	public void Shake() {
		if (!shaketm.IsStopped())
			cam.SetOffset(new Vector2(rnd.Next(-10, 11) / 10.0f * shakeAmount, rnd.Next(-10, 11) / 10.0f * shakeAmount));
	}

	public void KickAreaEntered(Area2D area) {
		if (area.IsInGroup("hurtbox")) {
			shaketm.Start();
			//	sprite.SpeedScale -= 0.5f;
			//cam.SetOffset(new Vector2(rnd.Next(-10, 11) / 10.0f * shakeAmount, rnd.Next(-10, 11) / 10.0f * shakeAmount));
			area.GetParent().Call("TakeDamage", AttackDamage(attack_type), AttackOtk(attack_type, pushes));
		}
	}

	//Put it to Strong attack method for optimize
	public void ChangeFrameOffset() {
		if (sprite.Animation == "AttackStrong") {
			if (sprite.Frame == 4) {
				sprite.SetOffset(new Vector2(0, -150f));
			} else {
				sprite.SetOffset(new Vector2(0, -40f));
			}
		} else if (sprite.Animation == "DoubleAttackStrong") {
			sprite.SetOffset(new Vector2(0, -40f));	
		} else {
			sprite.SetOffset(Vector2.Zero);
		}
	}

	public void TakeDamage(int dmg, int otkv = 0) {
		
		sprite.Play("TakenDamage");
		hp -= dmg;
		shaketm.Start();
		global_hp.Set("hp", hp);
		/*sounds.Stream = (hurtSound);
		sounds.Play();*/
		otk = otkv;
	}



	void Debug(float delta) {
		GD.Print("State: " + state);
	}

	float fx() {
		return 0.002f * this.GlobalPosition.y + 0.1213f;
	}

	void PlaySound() {
		if (state != STATE.ATTACK || sounds.Playing) return;
		switch(attack_type) {
			case ATTACK_TYPE.STANDART:
				sounds.Stream = attackSound;
				sounds.Play();
				break;
			case ATTACK_TYPE.STRONG:
				sounds.Stream = (jumpSound);
				sounds.Play();
				break;
			case ATTACK_TYPE.SHOT:
				sounds.Stream = (shootSound);
				sounds.Play();
				break;
			case ATTACK_TYPE.WAVE:
				sounds.Stream = (waveSound);
				sounds.Play();
				break;
		}
	}

	void AttackBeetweenFrames(int frame1, int frame2) {
		if (sprite.Frame >= frame1 && sprite.Frame <= frame2) {
			kickpos.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
		} else {
			kickpos.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
		}
	}

	void AttackBeetweenFrames(int[] frame1, int[] frame2) {
		for (int i = 0; i < frame1.Length; i++) {
			if (sprite.Frame >= frame1[i] && sprite.Frame <= frame2[i]) {
				sounds.Stop();
				kickpos.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
				PlaySound();
				break;
			} else {
				kickpos.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
			}
		}
	}

	public delegate void Callback();
	void DoInFrames(int frame1, int frame2, Callback callback) {
		if (sprite.Frame >= frame1 && sprite.Frame <= frame2) {
			callback();
		}
	}

	public override void _Process(float delta) {
		//ChangeFrameOffset();
	}

	public override void _PhysicsProcess(float delta) {
		if (hp <= 0) {
			sprite.Play("Death");
			if (deathtm.IsStopped()) {
				sounds.Stop();
				sounds.Stream = deathSound;
				sounds.Play();

				deathtm.Start();
			}
			return;
		}

		if (cutscene) {
			this.Visible = false;
			return;
		} else if (!this.Visible) {
			this.Visible = true;
		}

		if (otk != 0) {
			otk *= delta;
			Move(delta);
			otk = 0;
		}

		Animation();

		//Debug(delta);

		Attacks(delta);
		Shake();


		if (Input.IsActionPressed("ui_left") && CanMoveLeft()) {
			velocity.x = -1;
			if (stairs)
				velocity.y += 0.3f;
			kickpos.Position = new Vector2(-hitboxX, 1);
			this.GetNode<Position2D>("ShootPos").Position = new Vector2(-hitboxShootX, this.GetNode<Position2D>("ShootPos").Position.y);
			state = state == STATE.ATTACK ? STATE.ATTACK : STATE.WALK;
		} else if (Input.IsActionPressed("ui_right") && CanMoveRight()) {
			velocity.x = 1;
			if (stairs)
				velocity.y -= 0.3f;
			kickpos.Position = new Vector2(hitboxX, 1);
			this.GetNode<Position2D>("ShootPos").Position = new Vector2(hitboxShootX, this.GetNode<Position2D>("ShootPos").Position.y);
			state = state == STATE.ATTACK ? STATE.ATTACK : STATE.WALK;
		} else if (isIdleLR()) {
			state = STATE.IDLE;
		}


		if (Input.IsActionPressed("ui_up") && CanMoveUp()) {
			//this.GetParent<CanvasLayer>().FollowViewportScale = fx();
			velocity.y = -1;
			velocity.x += 0.3f;
			state = state == STATE.ATTACK ? STATE.ATTACK : STATE.WALK;
		} else if (Input.IsActionPressed("ui_down") && CanMoveDown()) {
			//this.GetParent<CanvasLayer>().FollowViewportScale = fx();
			velocity.y = 1;
			velocity.x -= 0.3f;
			state = state == STATE.ATTACK ? STATE.ATTACK : STATE.WALK;
		} else if (isIdleUD()) {
			state = STATE.IDLE;
		}

		if (state == STATE.WALK || state == STATE.ATTACK || state == STATE.JUMP) {
			Move(delta);
		}


		velocity = Vector2.Zero;
	}
	
	private void Attacks(float delta) {
		if (!timer.IsStopped() && Input.IsActionJustPressed("ui_accept") && state == STATE.ATTACK) {
			pushes++;
		} else if (pushes == 2 && attack_type == ATTACK_TYPE.STANDART) {
			TripleStandartAttack();
		}

		if (Input.IsActionPressed("ui_accept") && state != STATE.JUMP && state != STATE.ATTACK && pushes <= 1) {
			timer.Start();
			attack_type = ATTACK_TYPE.STANDART;
			state = STATE.ATTACK;
			PlaySound();
		}

		if (state == STATE.ATTACK && pushes <= 1 && attack_type == ATTACK_TYPE.STANDART) {
			StandartAttack();
		}



		if (!strongtimer.IsStopped() && Input.IsActionJustPressed("attack_strong") && state == STATE.ATTACK) {
			pushes++;
		} else if (pushes == 1 && attack_type == ATTACK_TYPE.STRONG) {
			strongtimer.Stop();
			//pushes = 0;
			void MoveAttack() {
				velocity.x = sprite.FlipH ? -1 : 1;
				this.SetCollisionLayer(0);
				this.SetCollisionMask(0);
				Move(delta, 2);
			}

			DoInFrames(3, 5, MoveAttack);
			if (sprite.Frame >= 5) {
				this.SetCollisionLayer(2);
				this.SetCollisionMask(1);
			}
			AttackBeetweenFrames(new int[] { 6, 11 }, new int[] { 7, 12 });
			state = STATE.ATTACK;
			attack_type = ATTACK_TYPE.STRONG;

			sprite.Play("DoubleAttackStrong");
		}

		if (Input.IsActionPressed("attack_strong") && state != STATE.JUMP && state != STATE.ATTACK && pushes == 0) {
			strongtimer.Start();
			attack_type = ATTACK_TYPE.STRONG;
			state = STATE.ATTACK;
		}

		if (state == STATE.ATTACK && attack_type == ATTACK_TYPE.STRONG && state != STATE.JUMP && pushes == 0) {
			AttackBeetweenFrames(6, 7);

			void MoveAttack() {
				velocity.x = sprite.FlipH ? -1 : 1;
				this.SetCollisionLayer(0);
				this.SetCollisionMask(0);
				Move(delta, 2);
			}

			DoInFrames(3, 5, MoveAttack);
			DoInFrames(6, 7, PlaySound);
			if (sprite.Frame >= 5) {
				this.SetCollisionLayer(2);
				this.SetCollisionMask(1);
			}
			state = STATE.ATTACK;
			attack_type = ATTACK_TYPE.STRONG;
			sprite.Play("AttackStrong");
		}


		if (Input.IsActionPressed("attack_strong") && state != STATE.JUMP && state == STATE.ATTACK && attack_type == ATTACK_TYPE.STANDART && wavecd.IsStopped()) {
			this.SetCollisionLayer(2);
			this.SetCollisionMask(1);
			sounds.Stop();
			strongtimer.Start();
			attack_type = ATTACK_TYPE.WAVE;
			state = STATE.ATTACK;
			PlaySound();
		}
		if (state == STATE.ATTACK && attack_type == ATTACK_TYPE.WAVE) {
			sprite.Play("Wave");
			void vc() {
				Area2D wave = this.GetNode<Area2D>("Wave");
				wave.Visible = true;
				wave.Call("Attack");
			}
			DoInFrames(0, 0, vc);
		}

		if (Input.IsActionPressed("shot_egg") && state != STATE.JUMP && state == STATE.ATTACK && attack_type == ATTACK_TYPE.STANDART) {
			attack_type = ATTACK_TYPE.PRESHOT;
			state = STATE.ATTACK;
		}
		if (Input.IsActionPressed("attack_strong") && state != STATE.JUMP && state == STATE.ATTACK && attack_type == ATTACK_TYPE.PRESHOT) {
			sounds.Stop();
			strongtimer.Start();
			attack_type = ATTACK_TYPE.SHOT;
			state = STATE.ATTACK;
			PlaySound();
		}


		if (state == STATE.ATTACK && attack_type == ATTACK_TYPE.SHOT) {
			sprite.Play("Shot");
			void vc() {
				if (!shot) {
					CanvasLayer b = bullet.Instance<CanvasLayer>();
					KinematicBody2D bullet_inst = b.GetChild<KinematicBody2D>(0);
					this.GetParent().GetParent().AddChild(b);
					
					bullet_inst.GlobalPosition = this.GetNode<Position2D>("ShootPos").GlobalPosition;
					bullet_inst.Set("velocity", sprite.FlipH ? new Vector2(-1, 0) : new Vector2(1, 0));
					this.GetParent().GetParent().GetParent().GetParent().Call("UpdateObj");
					shot = true;
				}
			}
			DoInFrames(6, 6, vc);
		}
		
	}

	private void Move(float delta, float xmod = 1, float ymod = 1) {
		velocity = velocity.Normalized();
		velocity.x *= xmod;
		velocity.y *= ymod;
		if (otk != 0) {
			velocity.x -= otk;
		}
		velocity = MoveAndSlide(velocity * speed);
	}
	private bool isIdleLR() {
		return CanMoveRight() && CanMoveLeft() && state != STATE.ATTACK;
	}

	private bool isIdleUD() {
		return (state == STATE.IDLE || state == STATE.WALK) && isIdleLR();
	}

	private bool CanMove() {
		return (state == STATE.IDLE || state == STATE.WALK || state != STATE.ATTACK);
	}

	private bool CanMoveRight() {
		return !Input.IsActionPressed("ui_left") && CanMove() && this.GlobalPosition.x <= cam.LimitRight;
	}

	private bool CanMoveLeft() {
		return !Input.IsActionPressed("ui_right") && CanMove() && this.GlobalPosition.x >= cam.LimitLeft;
	}

	private bool CanMoveUp() {
		return !Input.IsActionPressed("ui_down") && CanMove() && state != STATE.JUMP;
	}

	private bool CanMoveDown() {
		return !Input.IsActionPressed("ui_up") && CanMove() && state != STATE.JUMP;
	}


	private void StandartAttack() {
		
		AttackBeetweenFrames(2, 3);
		sprite.Play("Kick");
	}

	private void TripleStandartAttack() {
		timer.Stop();
		AttackBeetweenFrames(new int[] { 2, 5, 7, 9 }, new int[] { 3, 5, 7, 9 });
		state = STATE.ATTACK;
		attack_type = ATTACK_TYPE.STANDART;

		sprite.Play("Kick3Times");
	}
	private void Animation() {
		if (state != STATE.ATTACK) {
			if (Input.IsActionPressed("ui_left") || Input.IsActionPressed("ui_right") || Input.IsActionPressed("ui_up") || Input.IsActionPressed("ui_down")) {
				sprite.Play("Walk");
				if (Input.IsActionPressed("ui_right")) {
					sprite.FlipH = false;
				} else if (Input.IsActionPressed("ui_left")) {
					sprite.FlipH = true;
				}
			} else {
				sprite.Play("Idle");
			}
		} else {
			if (Input.IsActionPressed("ui_right")) {
				sprite.FlipH = false;
			} else if (Input.IsActionPressed("ui_left")) {
				sprite.FlipH = true;
			}
		}
	}

	public void OnAnimationFinish() {
		if (sprite.Animation == "Kick") {
			if (pushes == 1) {
				
				pushes = 0;
				sprite.Play("Idle");
				return;
			}
			state = STATE.IDLE;
			timer.Stop();
			pushes = 0;
			kickpos.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
		} else if (sprite.Animation == "Kick3Times") {
			state = STATE.IDLE;
			pushes = 0;
			kickpos.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
			this.SetCollisionLayer(2);
				this.SetCollisionMask(1);
		} else if (sprite.Animation == "AttackStrong") {
			state = STATE.IDLE;
			pushes = 0;
			kickpos.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
			this.SetCollisionLayer(2);
			this.SetCollisionMask(1);
		} else if (sprite.Animation == "DoubleAttackStrong") {
			state = STATE.IDLE;
			pushes = 0;
			kickpos.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
			this.SetCollisionLayer(2);
			this.SetCollisionMask(1);
		} else if (sprite.Animation == "Wave") {
			state = STATE.IDLE;
			pushes = 0;
			wavecd.Start();
			this.SetCollisionLayer(2);
			this.SetCollisionMask(1);
		} else if (sprite.Animation == "Shot") {
			shot = false;
			state = STATE.IDLE;
			pushes = 0;
			this.SetCollisionLayer(2);
			this.SetCollisionMask(1);
		}
		sounds.Stop();
	}

	public void _on_Death_timeout() {
		global_hp.Call("reload");
	}

	void _on_AnimatedSprite_cutscene_started() {
		cutscene = true;
	}

	void _on_AnimatedSprite_cutscene_stopped() {
		cutscene = false;
	}

	void _on_Sounds_finished() {
		sounds.Stop();
	}
}
