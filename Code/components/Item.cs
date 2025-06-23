using Sandbox;

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

    public virtual void OnUseItem()
    {
        if ( useCount == 0 )
        {
            GameObject.Destroy();
            return;
        }
        useCount -= 1;
    }

}
