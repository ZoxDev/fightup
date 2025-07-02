public sealed class PlatformCollision : Component, Component.ICollisionListener, Component.ITriggerListener

{
	void ICollisionListener.OnCollisionStart( Collision other )
	{
		Log.Info( "collision start with: " + other.Other.GameObject );
		GameObject collidedGameObject = other.Other.GameObject;
		if ( !collidedGameObject.Tags.Has( "player" ) || !collidedGameObject.IsProxy ) return;


		CharacterController characterController = collidedGameObject.GetOrAddComponent<CharacterController>();
		if ( characterController.Velocity.z > 0 )
		{
			Collider platformCollider = GameObject.GetComponent<Collider>();
			platformCollider.IsTrigger = true;
		}
	}

	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		Log.Info( "trigger emter with: " + other.GameObject );

		GameObject collidedGameObject = other.GameObject;
		if ( !collidedGameObject.Tags.Has( "player" ) || !collidedGameObject.IsProxy ) return;

		CharacterController characterController = collidedGameObject.GetOrAddComponent<CharacterController>();
		if ( characterController.Velocity.z <= 0 )
		{
			Collider platformCollider = GameObject.GetComponent<Collider>();
			platformCollider.IsTrigger = false;
		}
	}

}