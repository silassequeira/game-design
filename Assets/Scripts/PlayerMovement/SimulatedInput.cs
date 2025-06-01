// Check this class is correctly implemented
public class SimulatedInput : IPlayerInput
{
    public float HorizontalInput { get; set; }
    public bool JumpInput { get; set; }
    public bool JumpInputHeld { get; set; }
    
    public float GetHorizontalInput()
    {
        return HorizontalInput;
    }
    
    public bool GetJumpInputDown()
    {
        return JumpInput;
    }
    
    public bool GetJumpInputHeld()
    {
        return JumpInputHeld;
    }
}