public sealed class BoxGlovesItem : Item
{
	public override void OnUseItem()
	{
		if ( IsProxy ) return;

		base.OnUseItem();

		Log.Info( "Box gloves used !" );
		Log.Info( this.UseCount );
	}
}
