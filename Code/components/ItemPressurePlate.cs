using System;

public sealed class ItemPressurePlate : Component, Component.ITriggerListener
{
	[Property] public List<GameObject> prefabItemList { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();

		PickRandomItem();
		ApplyItemModel();
	}

	// TODO: implem IPressable to character controller so it can be used everywhere (get all collision and if they have pressable component)
	private bool canPress { get; set; } = false;
	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		// TODO: don't allow press when user already have an item of the same type
		canPress = true;
	}

	void ITriggerListener.OnTriggerExit( Collider other )
	{
		canPress = false;
	}

	private void PressUse()
	{
		if ( !canPress ) return;
		if ( Input.Pressed( "use" ) )
		{
		}
	}
	protected override void OnUpdate()
	{
		base.OnUpdate();
		PressUse();

		ItemMovement();

		DebugText();
	}

	private GameObject pickedPrefabItem { get; set; }
	private void PickRandomItem()
	{
		int randomItemIndex = new Random().Next( 0, prefabItemList.Count );
		pickedPrefabItem = prefabItemList[randomItemIndex];
	}

	private void ApplyItemModel()
	{
		Item itemComponent = pickedPrefabItem.GetComponent<Item>();

		Model itemModel = itemComponent.pressurePlateModel;
		Material itemMaterial = itemComponent.pressurePlateMaterial;

		GameObject pressurePlateItemModelGameObject = GameObject.Children.Find( go => go.Name == "item-model" );
		ModelRenderer pressurePlateItemModelRenderer = pressurePlateItemModelGameObject.GetOrAddComponent<ModelRenderer>();
		pressurePlateItemModelRenderer.Model = itemModel;
		pressurePlateItemModelRenderer.SetMaterial( itemMaterial );
	}

	private void ItemMovement()
	{
		GameObject itemModelGameObject = GameObject.Children.Find( go => go.Name == "item-model" );

		itemModelGameObject.LocalRotation = Rotation.FromYaw( itemModelGameObject.LocalRotation.Yaw() + 0.75f );

		float t = Time.Now % 1f;
		itemModelGameObject.LocalPosition = Vector3.Lerp(
			new Vector3( 0, 0, 20 ),
			new Vector3( 0, 0, 30 ),
			MathF.Sin( t * MathF.PI * 2 ) * 0.5f + 0.5f
		);
	}

	private void DebugText()
	{
		DebugOverlay.Text( WorldPosition + Vector3.Up * 50, "picked: " + pickedPrefabItem, 32, TextFlag.None, Color.Red );
	}
}
