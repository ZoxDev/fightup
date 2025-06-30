using System;
using System.Numerics;
using Sandbox.Citizen;
using Sandbox.UI;
using static Sandbox.Citizen.CitizenAnimationHelper;

public class PlayerController2D : Component, Component.IDamageable
{
	protected override void OnAwake()
	{
		base.OnAwake();
		Mouse.Visibility = MouseVisibility.Visible;

		itemComponentList = new List<Item>();
		playerCollider = GameObject.GetComponent<CapsuleCollider>();
		body = GameObject.Children.Find( go => go.Name == "Body" );
	}

	public TimeUntil NextPunch;
	public TimeUntil NextGroundPunch;
	protected override void OnFixedUpdate()
	{
		/* -----------------------------------------------------------------------------
		 * Movement
		 * -----------------------------------------------------------------------------*/
		Vector3 wishVelocity = getWishVelocity();

		Move( wishVelocity );
		bool isJumping = Input.Pressed( "Jump" );
		if ( isJumping ) Jump();
		if ( _isGroundPunch == false )
		{
			LookAt();
			Animate( wishVelocity, isJumping );
		}

		Pressing();
		UseItem();

		/* -----------------------------------------------------------------------------
		 * Combat
		 * -----------------------------------------------------------------------------*/
		//  primary
		if ( Input.Pressed( "attack1" ) && NextPunch <= 0 && !_isGroundPunch )
		{
			PrimaryAttack();
			NextPunch = PunchCoolDown;
		}

		// ground punch
		// TODO: if is above 100f height he can ground punch
		if ( Input.Pressed( "Duck" ) && NextGroundPunch <= 0 && !characterController.IsOnGround )
		{
			InitializeGroundPunch();
			NextGroundPunch = GroundPunchCoolDown;
		}

		if ( _isGroundPunch )
		{
			GroundPunching();
		}

		if ( _isGroundPunch && Input.Released( "Duck" ) )
		{
			characterController.Punch( Vector3.Up * (GroundPunchForce * 0.5f) );
			StopGroundPunching();
		}
	}

	/* -----------------------------------------------------------------------------
	 * Movement
	 * -----------------------------------------------------------------------------*/
	[Group( "Movement" )]
	[Property] public float AirFriction { get; set; } = 0.5f;
	[Group( "Movement" )]
	[Property] public float GroundFriction { get; set; } = 4.0f;
	[Group( "Movement" )]
	[Property] public float Speed { get; set; } = 220f;
	[Group( "Movement" )]
	[Property] public float JumpForce { get; set; } = 400f;
	[Group( "Movement" )]
	[Property] public GameObject cameraGameObject { get; set; }
	[Group( "Movement" )]
	[Property] public GameObject head { get; set; }
	[RequireComponent] CharacterController characterController { get; set; }
	[RequireComponent] CitizenAnimationHelper animationHelper { get; set; }
	private GameObject body { get; set; }

	Vector3 getWishVelocity()
	{
		Vector3 analogMove = Input.AnalogMove;
		Vector3 wishVelocity = -analogMove.WithX( 0 ).WithZ( 0 ) * Speed;

		return wishVelocity;
	}

	const float PLAYER_EYES_POSITION_Z = 60f;
	Vector2 getMousePosition()
	{
		float cameraZPos = cameraGameObject.WorldPosition.z;
		float playerZPos = GameObject.WorldPosition.z;
		float zToSubstract = (cameraZPos - playerZPos) - PLAYER_EYES_POSITION_Z;

		Vector2 screenCenter = new Vector2( Screen.Size.x, Screen.Size.y ) * 0.5f;
		Vector2 mousePosition = new Vector2( Mouse.Position.x - screenCenter.x, (Mouse.Position.y - screenCenter.y) - zToSubstract );

		return mousePosition;
	}

	void Move( Vector3 wishVelocity )
	{
		var gravity = Scene.PhysicsWorld.Gravity;

		if ( characterController.IsOnGround )
		{
			characterController.Velocity = characterController.Velocity.WithX( 0 ).WithZ( 0 );
			characterController.Accelerate( wishVelocity );
			characterController.ApplyFriction( GroundFriction );
		}
		else
		{
			characterController.Velocity += gravity * Time.Delta * 0.5f;
			characterController.Accelerate( wishVelocity );
			characterController.ApplyFriction( AirFriction );
		}

		characterController.Move();
	}

	void Jump()
	{
		if ( !characterController.IsOnGround ) return;

		characterController.Punch( Vector3.Up * JumpForce );
	}
	void LookAt()
	{
		Vector2 mousePosition = getMousePosition().Normal;
		Vector3 directionX = new Vector3( 0, mousePosition.x, 0 );

		body.WorldRotation = Rotation.LookAt( directionX ).Angles();
	}


	void Animate( Vector3 wishVelocity, bool isJumping = false )
	{
		if ( !animationHelper.IsValid() ) return;

		animationHelper.IsGrounded = characterController.IsOnGround;
		if ( isJumping && characterController.IsOnGround ) animationHelper?.TriggerJump();
		animationHelper.WithVelocity( wishVelocity );

		animationHelper.HoldType = HoldTypes.Punch;
		animationHelper.MoveStyle = MoveStyles.Run;

		Vector2 mousePosition = getMousePosition().Normal;
		Vector3 directionUp = new Vector3( 0, mousePosition.x, -mousePosition.y );

		animationHelper.WithLook( directionUp );
	}

	[Button]
	public void Ragdoll()
	{
		// TODO: implem ragdoll
	}

	[Button]
	public void UnRagdoll()
	{
		// TODO: implem unragdoll

	}

	/* -----------------------------------------------------------------------------
	 * Press
	 * -----------------------------------------------------------------------------*/
	private CapsuleCollider playerCollider { get; set; }
	void Pressing()
	{
		foreach ( Collider obj in playerCollider.Touching )
		{
			IPressable pressable = obj.GetComponent<IPressable>();
			if ( pressable == null ) return;

			if ( Input.Pressed( "use" ) )
			{
				IPressable.Event e = new IPressable.Event();
				e.Source = this;
				pressable.Press( e );
			}
		}
	}

	/* -----------------------------------------------------------------------------
	 * Combat
	 * -----------------------------------------------------------------------------*/
	[Group( "Combat" )]
	[Property] public float attackCoolDown { get; set; }
	[Group( "Combat" )]
	[Property] public float health { get; set; } = 100f;
	[Group( "Combat" )]
	[Property] public float PunchCoolDown { get; set; } = 0.5f;
	[Group( "Combat" )]
	[Property] public float GroundPunchForce { get; set; } = 100f;
	[Group( "Combat" )]
	[Property] public float GroundPunchCoolDown { get; set; } = 3f;
	[Group( "Combat" )]
	[Property] public float GroundPunchRadius { get; set; } = 50f;
	private GameObject _groundPunchObject { get; set; }

	void IDamageable.OnDamage( in DamageInfo damage )
	{
		health -= damage.Damage;

		// hit animation
		Vector3 attackerPosition = damage.Attacker.WorldPosition;
		Vector3 myPosition = GameObject.WorldPosition;
		bool isAttackFromLeft = (myPosition - attackerPosition).y >= 0;
		characterController.Punch( isAttackFromLeft ? Vector3.Right * 1000 : Vector3.Left * 1000 );


		animationHelper.Target.Set( "hit", true );
		animationHelper.Target.Set( "hit_strength", 100 );
		// TODO: fix hit direction
		animationHelper.Target.Set( "hit_direction", (myPosition - attackerPosition) );

		if ( health == 0 )
		{
			Dead();
		}
	}

	void PrimaryAttack()
	{
		Vector2 mousePosition = getMousePosition();

		Vector3 start = GameObject.WorldPosition.WithZ( GameObject.WorldPosition.z + PLAYER_EYES_POSITION_Z );
		Vector3 end = start + new Vector3( 0, mousePosition.x, -mousePosition.y ).Normal * 50;

		animationHelper.Target.Set( "b_attack", true );

		SceneTraceResult trace = Scene.Trace.Ray( start, end )
		.WithTag( "player" )
		.IgnoreGameObjectHierarchy( GameObject )
		.Run();

		DebugOverlay.Line( start, end, Color.Green, 0.5f );

		if ( !trace.Hit ) return;

		IDamageable hitDamageable = trace.GameObject.GetComponent<IDamageable>();

		DamageInfo damageInfo = new DamageInfo();
		damageInfo.Damage = 5;
		damageInfo.Attacker = GameObject;
		damageInfo.Position = trace.HitPosition;

		hitDamageable.OnDamage( damageInfo );
	}

	void InitializeGroundPunch()
	{
		_isGroundPunch = true;
		characterController.Punch( Vector3.Down * GroundPunchForce );

		// animating
		animationHelper.IsSitting = true;
		animationHelper.Sitting = SittingStyle.Floor;
		animationHelper.HoldType = HoldTypes.None;
		animationHelper.Target.Set( "b_grounded", true );
		body.WorldRotation = Rotation.FromYaw( 0 );
	}

	private bool _isGroundPunch = false;
	void GroundPunching()
	{
		if ( characterController.IsOnGround )
		{
			SceneTraceResult trace = Scene.Trace.Sphere( GroundPunchRadius, GameObject.WorldPosition, GameObject.WorldPosition )
			.WithTag( "player" )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();

			Sphere debugSphere = new Sphere() { Center = trace.StartPosition, Radius = GroundPunchRadius };
			DebugOverlay.Sphere( debugSphere, Color.Green, 0.5f );

			if ( trace.Hit )
			{
				IDamageable hitDamageable = trace.GameObject.GetComponent<IDamageable>();

				DamageInfo damageInfo = new DamageInfo();
				damageInfo.Damage = 5;
				damageInfo.Attacker = GameObject;
				damageInfo.Position = trace.HitPosition;

				hitDamageable.OnDamage( damageInfo );
			}

			StopGroundPunching();
			_isGroundPunch = false;
		}
	}

	void StopGroundPunching()
	{
		animationHelper.IsSitting = false;
		animationHelper.Sitting = SittingStyle.None;
		animationHelper.HoldType = HoldTypes.Punch;

		_isGroundPunch = false;
	}

	private void Dead()
	{
		// TODO: implement dead system (ragdoll + like smash goes away fast in the hit direction)
		GameObject.Destroy();
	}

	/* -----------------------------------------------------------------------------
	 * Items
	 * -----------------------------------------------------------------------------*/
	[Group( "Items" )]
	[Property] public float itemCooldown { get; set; }
	[Group( "Items" )]
	[Property] public List<Item> itemComponentList { get; set; }
	public void FetchItems()
	{
		GameObject itemListGameObject = GameObject.Children.Find( child => child.Tags.Has( "item-list" ) );
		Log.Info( "itemListGameObject" + itemListGameObject.Children.First() );

		foreach ( GameObject itemGameObject in itemListGameObject.Children )
		{
			Item itemComnponent = itemGameObject.GetComponent<Item>();

			if ( itemComnponent == null )
			{
				Log.Error( "the item has fail to fetch" );
				return;
			}

			itemComponentList.Add( itemComnponent );
		}
	}
	void UseItem()
	{
		foreach ( Item item in itemComponentList )
		{
			bool isUsing = item.pressType == PressTypeEnum.Pressed ? Input.Pressed( item.inputAction.Name ) : Input.Down( item.inputAction.Name );
			if ( isUsing )
			{
				item.OnUseItem();
				AnimationItem( item );
			}
		}
	}

	void AnimationItem( Item item )
	{
		// TODO: implement this
	}
}
