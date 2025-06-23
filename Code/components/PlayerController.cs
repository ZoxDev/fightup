using System;
using Sandbox.Citizen;

public sealed class PlayerController2D : Component
{
	[Group( "Movement" )]
	[Property] public float AirFriction { get; set; } = 0.5f;
	[Group( "Movement" )]
	[Property] public float GroundFriction { get; set; } = 4.0f;
	[Group( "Movement" )]
	[Property] public float Speed { get; set; } = 220f;
	[Group( "Movement" )]
	[Property] public float JumpForce { get; set; } = 400f;

	[RequireComponent] CharacterController characterController { get; set; }
	[RequireComponent] CitizenAnimationHelper animationHelper { get; set; }

	// Items
	[Group( "Items" )]
	[Property] public List<Item> itemList { get; set; }

	private CapsuleCollider playerCollider { get; set; }
	protected override void OnAwake()
	{
		base.OnAwake();
		playerCollider = GameObject.GetComponent<CapsuleCollider>();
	}

	protected override void OnFixedUpdate()
	{
		var isJumping = Input.Pressed( "Jump" );

		Vector3 wishVelocity = getWishVelocity();

		Move( wishVelocity );
		if ( isJumping ) Jump();
		Rotate( wishVelocity );
		Animate( wishVelocity, isJumping );
		Pressing();
		// UseItem();
	}

	/* -----------------------------------------------------------------------------
	 * Movement
	 * -----------------------------------------------------------------------------*/

	Vector3 getWishVelocity()
	{
		Vector3 analogMove = Input.AnalogMove;
		Vector3 wishVelocity = -analogMove.WithX( 0 ).WithZ( 0 ) * Speed;

		return wishVelocity;
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

	void Rotate( Vector3 wishVelocity )
	{
		GameObject body = Scene.Directory.FindByName( "Body" ).First();
		Angles currentAngles = body.WorldRotation.Angles();


		if ( wishVelocity.IsNearlyZero() )
		{
			body.WorldRotation = Rotation.FromYaw( 0 );
		}

		currentAngles.yaw = Rotation.LookAt( wishVelocity ).Angles().yaw;
		body.WorldRotation = Rotation.From( currentAngles );
	}

	void Jump()
	{
		if ( !characterController.IsOnGround ) return;

		characterController.Punch( Vector3.Up * JumpForce );
	}

	void Animate( Vector3 wishVelocity, bool isJumping = false )
	{
		if ( !animationHelper.IsValid() ) return;

		animationHelper.WithVelocity( wishVelocity );
		if ( isJumping && characterController.IsOnGround ) animationHelper?.TriggerJump();

		animationHelper.IsGrounded = characterController.IsOnGround;
	}

	/* -----------------------------------------------------------------------------
	 * Press
	 * -----------------------------------------------------------------------------*/
	void Pressing()
	{
		foreach ( Collider obj in playerCollider.Touching )
		{
			IPressable pressable = obj.GetComponent<IPressable>();
			if ( pressable == null ) return;

			if ( Input.Pressed( "use" ) )
			{
				IPressable.Event e = new IPressable.Event();
				e.Source = GameObject.GetComponent<PlayerController2D>();
				pressable.Press( e );
			}
		}
	}

	/* -----------------------------------------------------------------------------
	 * Items
	 * -----------------------------------------------------------------------------*/
	void UseItem()
	{
		// if ( itemList.Count > 0 )
		// {
		// 	foreach ( Item item in itemList )
		// 	{
		// 		Item itemComponent = item.GetComponent<Item>();

		// 		bool isUsing = itemComponent.pressType == PressTypeEnum.Pressed ? Input.Pressed( itemComponent.inputAction.KeyboardCode ) : Input.Down( itemComponent.inputAction.KeyboardCode );
		// 		if ( isUsing )
		// 		{
		// 			itemComponent.OnUseItem();
		// 		}
		// 	}
		// }
	}
}
