using System;
using Sandbox.Citizen;
using Sandbox.UI;
using static Sandbox.Citizen.CitizenAnimationHelper;

public class PlayerController2D : Component, Component.IDamageable, Component.INetworkSpawn
{
	[Group( "Clothing" )]
	[Property] public SkinnedModelRenderer BodyRenderer { get; set; }
	void INetworkSpawn.OnNetworkSpawn( Connection owner )
	{
		var clothing = new ClothingContainer();
		clothing.Deserialize( owner.GetUserData( "avatar" ) );
		clothing.Apply( BodyRenderer );
	}
	public static GameObject LocalPlayer { get; private set; } = null;
	protected override void OnAwake()
	{
		base.OnAwake();
		Mouse.Visibility = MouseVisibility.Visible;

		itemComponentList = new List<Item>();
		_playerCollider = GameObject.GetComponentInChildren<CapsuleCollider>();


		_body = GameObject.Children.Find( go => go.Name == "Body" );

		if ( !IsProxy )
		{
			LocalPlayer = GameObject;
		}
	}

	[Sync] private bool isJumping { get; set; }
	public TimeUntil NextPunch;
	public TimeUntil NextGroundPunch;
	protected override void OnFixedUpdate()
	{
		/* -----------------------------------------------------------------------------
		 * Movement
		 * -----------------------------------------------------------------------------*/
		if ( !IsProxy )
		{
			Vector3 wishVelocity = getWishVelocity();

			isJumping = Input.Pressed( "Jump" );

			if ( isJumping )
			{
				Jump();

			}
			Move( wishVelocity );
			Pressing();
			UseItem();
			Animate( wishVelocity, isJumping );
		}

		if ( _isGroundPunch == false )
		{
			if ( !IsProxy )
			{
				LookAt();
			}
		}

		/* -----------------------------------------------------------------------------
		 * Combat
		 * -----------------------------------------------------------------------------*/

		if ( !IsProxy )
		{
			//  primary
			if ( Input.Pressed( "attack1" ) && NextPunch <= 0 && !_isGroundPunch )
			{
				PrimaryAttack();
				NextPunch = PunchCoolDown;
			}

			// ground punch
			// TODO: if is above 100f height he can ground punch
			if ( Input.Pressed( "Duck" ) && NextGroundPunch <= 0 && !CharacterController.IsOnGround )
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
				CharacterController.Punch( Vector3.Up * (GroundPunchForce * 0.5f) );
				StopGroundPunching();
			}
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
	[Property] public GameObject Head { get; set; }
	[Property] public Rigidbody rigidbody;
	[RequireComponent] CharacterController CharacterController { get; set; }
	[RequireComponent] CitizenAnimationHelper AnimationHelper { get; set; }
	[Sync] private Vector2 _mousePosition { get; set; }
	public GameObject CameraGameObject { get; set; }
	private GameObject _body { get; set; }


	Vector3 getWishVelocity()
	{
		Vector3 analogMove = Input.AnalogMove;
		Vector3 wishVelocity = -analogMove.WithX( 0 ).WithZ( 0 ) * Speed;

		return wishVelocity;
	}

	const float PLAYER_EYES_POSITION_Z = 60f;
	Vector2 getMousePosition()
	{
		if ( CameraGameObject == null || GameObject == null ) return Vector2.Zero;

		float cameraZPos = CameraGameObject.WorldPosition.z;
		float playerZPos = GameObject.WorldPosition.z;
		float zToSubstract = (cameraZPos - playerZPos) - PLAYER_EYES_POSITION_Z;

		Vector2 screenCenter = new Vector2( Screen.Size.x, Screen.Size.y ) * 0.5f;
		Vector2 mousePosition = new Vector2( Mouse.Position.x - screenCenter.x, (Mouse.Position.y - screenCenter.y) - zToSubstract );

		return mousePosition;
	}

	void Move( Vector3 wishVelocity )
	{
		var gravity = Scene.PhysicsWorld.Gravity;

		if ( CharacterController.IsOnGround )
		{
			CharacterController.Velocity = CharacterController.Velocity.WithX( 0 ).WithZ( 0 );
			CharacterController.Accelerate( wishVelocity );
			CharacterController.ApplyFriction( GroundFriction );
		}
		else
		{
			CharacterController.Velocity += gravity * Time.Delta * 0.5f;
			CharacterController.Accelerate( wishVelocity );
			CharacterController.ApplyFriction( AirFriction );
		}

		rigidbody.Velocity = CharacterController.Velocity;

		CharacterController.UseCollisionRules = true;
		CharacterController.Move();
	}

	void Jump()
	{
		if ( !CharacterController.IsOnGround ) return;

		AnimateJunp();
		CharacterController.Punch( Vector3.Up * JumpForce );
	}
	[Rpc.Broadcast]
	void AnimateJunp()
	{
		AnimationHelper?.TriggerJump();

	}

	[Rpc.Broadcast]
	void LookAt()
	{
		_mousePosition = getMousePosition().Normal;
		Vector3 directionX = new Vector3( 0, _mousePosition.x, 0 );

		if ( _mousePosition.x == 0 ) return;

		_body.WorldRotation = Rotation.LookAt( directionX ).Angles();
	}

	[Rpc.Broadcast]
	void Animate( Vector3 wishVelocity, bool isJumping = false )
	{
		if ( !AnimationHelper.IsValid() ) return;

		AnimationHelper.IsGrounded = CharacterController.IsOnGround;
		AnimationHelper.WithVelocity( wishVelocity );

		AnimationHelper.HoldType = HoldTypes.Punch;
		AnimationHelper.MoveStyle = MoveStyles.Run;

		_mousePosition = getMousePosition().Normal;
		Vector3 directionUp = new Vector3( 0, _mousePosition.x, -_mousePosition.y );

		AnimationHelper.WithLook( directionUp );
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
	private CapsuleCollider _playerCollider { get; set; }
	void Pressing()
	{
		foreach ( Collider obj in _playerCollider.Touching )
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
	[Property] public float AttackCoolDown { get; set; }
	[Group( "Combat" )]
	[Property] public float Health { get; set; } = 100f;
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
		Health -= damage.Damage;

		OnDamageAnimation( damage );

		if ( Health == 0 )
		{
			Dead();
		}
	}

	[Rpc.Broadcast]
	private void Dead()
	{
		// TODO: implement dead system (ragdoll + like smash goes away fast in the hit direction)
		GameObject.Destroy();
	}

	[Rpc.Broadcast]
	void OnDamageAnimation( DamageInfo damage )
	{
		Vector3 attackerPosition = damage.Attacker.WorldPosition;
		Vector3 myPosition = GameObject.WorldPosition;

		bool isAttackFromLeft = (myPosition - attackerPosition).y <= 0;
		CharacterController.Punch( isAttackFromLeft ? Vector3.Right * 500 : Vector3.Left * 500 );

		AnimationHelper.Target.Set( "hit", true );
		AnimationHelper.Target.Set( "hit_strength", 100 );
		AnimationHelper.Target.Set( "hit_direction", (myPosition - attackerPosition) );

		Log.Info( isAttackFromLeft );
	}

	void PrimaryAttack()
	{
		_mousePosition = getMousePosition();

		Vector3 start = GameObject.WorldPosition.WithZ( GameObject.WorldPosition.z + PLAYER_EYES_POSITION_Z );
		Vector3 end = start + new Vector3( 0, _mousePosition.x, -_mousePosition.y ).Normal * 50;

		AnimatePrimaryAttack();

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
	[Rpc.Broadcast]
	void AnimatePrimaryAttack()
	{
		AnimationHelper.Target.Set( "b_attack", true );
	}

	void InitializeGroundPunch()
	{
		_isGroundPunch = true;
		CharacterController.Punch( Vector3.Down * GroundPunchForce );

		// animating
		AnimateGroundPunch();
	}

	[Rpc.Broadcast]
	void AnimateGroundPunch()
	{
		AnimationHelper.IsSitting = true;
		AnimationHelper.Sitting = SittingStyle.Floor;
		AnimationHelper.HoldType = HoldTypes.None;
		AnimationHelper.Target.Set( "b_grounded", true );
		_body.WorldRotation = Rotation.FromYaw( 0 );

	}

	[Sync] private bool _isGroundPunch { get; set; } = false;
	void GroundPunching()
	{
		if ( CharacterController.IsOnGround )
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
		AnimationStopGroundPunch();
		_isGroundPunch = false;
	}

	[Rpc.Broadcast]
	void AnimationStopGroundPunch()
	{
		AnimationHelper.IsSitting = false;
		AnimationHelper.Sitting = SittingStyle.None;
		AnimationHelper.HoldType = HoldTypes.Punch;

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
