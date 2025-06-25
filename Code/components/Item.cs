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

    private GameObject itemListInPlayer { get; set; }
    private PlayerController2D playerController { get; set; }

    protected override void OnAwake()
    {
        base.OnAwake();
        GameObject player = GameObject.Parent.Parent;

        if ( player.Tags.Has( "player" ) )
        {
            itemListInPlayer = player.Children.Find( child => child.Tags.Has( "item-list" ) );
            playerController = player.GetComponent<PlayerController2D>();
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        itemListInPlayer.Children.Remove( GameObject );
        playerController.itemComponentList.Find( item => item == this );
        playerController.itemComponentList.Remove( this );
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
