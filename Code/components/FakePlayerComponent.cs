using System.Numerics;
using Sandbox;
using Sandbox.Citizen;

public sealed class FakePlayerComponent : Component, Component.IDamageable
{
	[Property] public float health { get; set; } = 100f;
	[Property] CitizenAnimationHelper AnimationHelper { get; set; }
	[Property] CharacterController CharacterController { get; set; }
	[Property] public float GroundFriction { get; set; } = 4.0f;
	[Property] public float AirFriction { get; set; } = 0.5f;
	private ModelRenderer _bodyModelRenderer { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		_bodyModelRenderer = GameObject.GetComponent<ModelRenderer>();
		Log.Info( CharacterController );
	}

	protected override void OnUpdate()
	{
		DebugText();
		Move();
	}

	void Move()
	{
		var gravity = Scene.PhysicsWorld.Gravity;

		if ( CharacterController.IsOnGround )
		{
			CharacterController.Velocity = CharacterController.Velocity.WithX( 0 ).WithZ( 0 );
			CharacterController.ApplyFriction( GroundFriction );
		}
		else
		{
			CharacterController.Velocity += gravity * Time.Delta * 0.5f;
			CharacterController.ApplyFriction( AirFriction );
		}

		CharacterController.Move();
	}

	void IDamageable.OnDamage( in DamageInfo damage )
	{
		health -= damage.Damage;

		// hit animation
		Vector3 attackerPosition = damage.Attacker.WorldPosition;
		Vector3 myPosition = GameObject.WorldPosition;
		bool isAttackFromLeft = (myPosition - attackerPosition).y >= 0;
		CharacterController.Punch( isAttackFromLeft ? Vector3.Right * 1000 : Vector3.Left * 1000 );

		AnimationHelper.Target.Set( "hit", true );
		AnimationHelper.Target.Set( "hit_strength", 100 );
		AnimationHelper.Target.Set( "hit_direction", (myPosition - attackerPosition) );

		if ( health == 0 )
		{
			GameObject.Destroy();
		}
	}

	private void DebugText()
	{
		DebugOverlay.Text( WorldPosition + Vector3.Up * 80, "Fake player", 48, TextFlag.None, Color.Red );
	}
}
