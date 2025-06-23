using Sandbox;

public sealed class RocketLauncherItem : Item
{
	public override void OnUseItem()
	{
		Log.Info( "throw rocket launcher" );
	}
}
