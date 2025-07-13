using System;
using System.Security.Cryptography.X509Certificates;

public sealed class ItemPressurePlate : Component, Component.ITriggerListener, Component.IPressable
{
	[Property] public List<GameObject> prefabItemList { get; set; }
	protected override void OnAwake()
	{
		base.OnAwake();
		PickRandomItem();
		ApplyItemModel();
	}

	private bool _allowPress { get; set; } = false;
	private void TakeItem( GameObject gameObject )
	{
		if ( gameObject.Tags.Has( "player" ) == false ) return;
		if ( gameObject.IsProxy ) return;

		PlayerController2D playerController = PlayerController2D.LocalPlayer;

		bool isSameItemType = playerController.itemComponentList.Find( item => item.ItemType == _itemComponent.ItemType ) != null;
		if ( isSameItemType )
		{
			_allowPress = true;
			return;
		}

		GameObject itemListGameObject = playerController.GameObject.Children.Find( child => child.Tags.Has( "item-list" ) );

		CloneConfig cloneConfig = new CloneConfig();
		cloneConfig.Name = _pickedPrefabItem.Name;
		cloneConfig.Parent = itemListGameObject;
		cloneConfig.StartEnabled = true;

		_pickedPrefabItem.Clone( cloneConfig ).NetworkSpawn();
		playerController.FetchItems();

		DestroyPressurePlate();
	}

	[Rpc.Broadcast]
	void DestroyPressurePlate()
	{
		GameObject.Destroy();
	}

	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		TakeItem( other.GameObject );
	}

	bool IPressable.Press( IPressable.Event e )
	{
		if ( !_allowPress ) return false;
		if ( e.Source.GameObject.Tags.Has( "player" ) == false ) return false;
		if ( e.Source.IsProxy ) return false;

		PlayerController2D playerController = PlayerController2D.LocalPlayer;
		GameObject itemListGameObject = playerController.GameObject.Children.Find( child => child.Tags.Has( "item-list" ) );

		// remove old item
		Item itemToRemove = playerController.itemComponentList.Find( item => item.ItemType == _itemComponent.ItemType );
		playerController.itemComponentList.Remove( itemToRemove );
		Log.Info( itemListGameObject.Children.Count );
		itemListGameObject.Children.Remove( itemToRemove.GameObject );
		Log.Info( itemListGameObject.Children.Count );


		// add the item
		CloneConfig cloneConfig = new CloneConfig();
		cloneConfig.Name = _pickedPrefabItem.Name;
		cloneConfig.Parent = itemListGameObject;
		cloneConfig.StartEnabled = true;

		_pickedPrefabItem.Clone( cloneConfig ).NetworkSpawn();
		playerController.FetchItems();

		DestroyPressurePlate();

		return true;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		ItemMovement();
	}

	[Sync] private GameObject _pickedPrefabItem { get; set; }
	private Item _itemComponent { get; set; }
	private void PickRandomItem()
	{
		int randomItemIndex = new Random().Next( 0, prefabItemList.Count );
		_pickedPrefabItem = prefabItemList[randomItemIndex];
		_pickedPrefabItem.NetworkSpawn();
		_itemComponent = _pickedPrefabItem.GetComponent<Item>();
	}

	private void ApplyItemModel()
	{
		Model itemModel = _itemComponent.PressurePlateModel;
		Material itemMaterial = _itemComponent.PressurePlateMaterial;

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
}
