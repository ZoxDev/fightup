public enum ItemTypeEnum
{
    Weapon,
    JumpBoosts,
    StatusEffect
}

public enum PressTypeEnum
{
    Pressed,
    Hold
}

public class Item : Component
{
    [Property] public ItemTypeEnum ItemType { get; set; }

    [Group( "Uses" )]
    [Property, InputAction] public InputAction InputAction { get; set; }
    [Group( "Uses" )]
    [Property] public PressTypeEnum PressType { get; set; }
    [Group( "Uses" )]
    [Property] public int UseCount { get; set; } = 1;

    [Group( "Models" )]
    [Property] public Model PressurePlateModel { get; set; }
    [Group( "Models" )]
    [Property] public Material PressurePlateMaterial { get; set; }
    [Group( "Models" )]
    [Property] public Model PrefabModel { get; set; }

    private GameObject _player { get; set; } = null;
    private GameObject _itemListInPlayer { get; set; } = null;
    private PlayerController2D _playerController { get; set; } = null;

    protected override void OnUpdate()
    {
        if ( IsProxy ) return;

        base.OnUpdate();
        _playerController ??= PlayerController2D.LocalPlayer;
        _itemListInPlayer ??= _playerController.GameObject.Children.Find( child => child.Tags.Has( "item-list" ) );
    }

    protected override void OnDestroy()
    {
        if ( IsProxy ) return;
        if ( _playerController == null ) return;
        if ( _itemListInPlayer == null ) return;


        _playerController.itemComponentList.Find( item => item == this && item._player == this._player );
        _playerController.itemComponentList.Remove( this );
        _itemListInPlayer.Children.Remove( GameObject );

        base.OnDestroy();
    }

    public virtual void OnUseItem()
    {
        UseCount -= 1;

        if ( UseCount == 0 )
        {
            GameObject.Destroy();
            return;
        }
    }

}
