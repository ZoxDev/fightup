public sealed class PlatformCollision : Component, Component.ICollisionListener, Component.ITriggerListener

{
	void ICollisionListener.OnCollisionStart( Collision other )
	{
		if ( other.Other.GameObject.IsProxy ) return;

		GameObject collidedGameObject = other.Other.GameObject;

		if ( !collidedGameObject.Tags.Has( "player" ) ) return;

		CharacterController characterController = collidedGameObject.GetOrAddComponent<CharacterController>();
		if ( characterController.Velocity.z > 0 )
		{
			Collider platformCollider = GameObject.GetComponent<Collider>();
			platformCollider.IsTrigger = true;
		}
	}

	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		if ( other.GameObject.IsProxy ) return;

		GameObject collidedGameObject = other.GameObject;
		if ( !collidedGameObject.Tags.Has( "player" ) ) return;

		CharacterController characterController = collidedGameObject.GetOrAddComponent<CharacterController>();
		if ( characterController.Velocity.z <= 0 )
		{
			Collider platformCollider = GameObject.GetComponent<Collider>();
			platformCollider.IsTrigger = false;
		}
	}
}