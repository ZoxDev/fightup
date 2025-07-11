using System;

public sealed class ItemPressurePlate : Component, Component.ITriggerListener, Component.IPressable
{
	[Property] public List<GameObject> prefabItemList { get; set; }

	private GameObject _pickedPrefabItem { get; set; }
	private Item _itemComponent { get; set; }
	private void PickRandomItem()
	{
		int randomItemIndex = new Random().Next( 0, prefabItemList.Count );
		_pickedPrefabItem = prefabItemList[randomItemIndex];
		_pickedPrefabItem.NetworkSpawn();
		_itemComponent = _pickedPrefabItem.GetComponent<Item>();
	}
	protected override void OnAwake()
	{
		base.OnAwake();

		PickRandomItem();
		ApplyItemModel();
	}

	private bool allowPress { get; set; } = false;
	private void TakeItem( GameObject gameObject )
	{
		if ( !gameObject.Tags.Has( "player" ) ) return;

		PlayerController2D playerController = gameObject.GetComponent<PlayerController2D>();

		bool isSameItemType = playerController.itemComponentList.Find( item => item.itemType == _itemComponent.itemType ) != null;
		if ( isSameItemType && !allowPress )
		{
			allowPress = true;
			return;
		}

		GameObject itemListGameObject = playerController.GameObject.Children.Find( child => child.Tags.Has( "item-list" ) );

		CloneConfig cloneConfig = new CloneConfig();
		cloneConfig.Name = _pickedPrefabItem.Name;
		cloneConfig.Parent = itemListGameObject;
		cloneConfig.StartEnabled = true;

		_pickedPrefabItem.Clone( cloneConfig );

		playerController.FetchItems();

		if ( !isSameItemType ) GameObject.Destroy();
	}

	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		TakeItem( other.GameObject );
	}

	bool IPressable.Press( IPressable.Event e )
	{
		if ( allowPress ) TakeItem( e.Source.GameObject );
		// TODO: remove old the item that you replace
		GameObject.Destroy();

		return true;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		ItemMovement();
		DebugText();
	}

	private void ApplyItemModel()
	{
		Model itemModel = _itemComponent.pressurePlateModel;
		Material itemMaterial = _itemComponent.pressurePlateMaterial;


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
		DebugOverlay.Text( WorldPosition + Vector3.Up * 50, "picked: " + _pickedPrefabItem, 32, TextFlag.None, Color.Red );
	}
}
