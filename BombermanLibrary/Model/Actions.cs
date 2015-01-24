namespace Bomberman.Model
{
    /// <summary>
    /// Defines standard actions that may have graphical display with sprites.
    /// </summary>
    public enum Actions
    {
        Idle,

        // Movement actions
        Walk,
        Run,

        // Death actions
        Hit,
        Burn,

        // Miscellaneous
        Stun,
        Special,
    }
}