public sealed class CollisionTest : Component, Component.ICollisionListener
{
    void ICollisionListener.OnCollisionStart( Collision collision )
    {
        Log.Info( collision.Other.GameObject );
    }
}