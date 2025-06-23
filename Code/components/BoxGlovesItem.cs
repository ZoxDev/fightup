public sealed class BoxGlovesItem : Item
{
	public override void OnUseItem()
	{
		base.OnUseItem();

		Log.Info( "Box gloves used !" );
	}
}
