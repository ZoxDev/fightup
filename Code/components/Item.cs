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
    [Property] public ItemTypeEnum itemType { get; set; }

    [Group( "Uses" )]
    [Property, InputAction] public InputAction inputAction { get; set; }
    [Group( "Uses" )]
    [Property] public PressTypeEnum pressType { get; set; }
    [Group( "Uses" )]
    [Property] public int useCount { get; set; } = 1;

    [Group( "Models" )]
    [Property] public Model pressurePlateModel { get; set; }
    [Group( "Models" )]
    [Property] public Material pressurePlateMaterial { get; set; }
    [Group( "Models" )]
    [Property] public Model prefabModel { get; set; }

    private GameObject _itemListInPlayer { get; set; }
    private PlayerController2D _playerController { get; set; }
    private GameObject _player { get; set; } = null;

    protected override void OnUpdate()
    {
        base.OnUpdate();
        if ( PlayerController2D.LocalPlayer != null && _player == null )
        {
            _player = PlayerController2D.LocalPlayer;
            _itemListInPlayer = _player.Children.Find( child => child.Tags.Has( "item-list" ) );
            _playerController = _player.GetComponent<PlayerController2D>();
        }
    }

    protected override void OnDestroy()
    {
        _itemListInPlayer.Children.Remove( GameObject );
        _playerController.itemComponentList.Find( item => item == this );
        _playerController.itemComponentList.Remove( this );

        base.OnDestroy();
    }

    public virtual void OnUseItem()
    {
        useCount -= 1;

        if ( useCount == 0 )
        {
            GameObject.Destroy();
            return;
        }
    }

}
