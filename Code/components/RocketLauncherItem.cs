public sealed class RocketLauncherItem : Item
{
	public override void OnUseItem()
	{
		if ( IsProxy ) return;

		base.OnUseItem();

		Log.Info( "throw rocket launcher" );
		Log.Info( this.UseCount );
	}
}
