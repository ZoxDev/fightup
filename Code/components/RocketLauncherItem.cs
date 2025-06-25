using Sandbox;

public sealed class RocketLauncherItem : Item
{
	public override void OnUseItem()
	{
		base.OnUseItem();

		Log.Info( "throw rocket launcher" );
		Log.Info( this.useCount );
	}
}
