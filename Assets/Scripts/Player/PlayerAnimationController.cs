using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;
    private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int VerticalVelocity = Animator.StringToHash("VerticalVelocity");
    private static readonly int HorizontalVelocity = Animator.StringToHash("HorizontalVelocity");
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    
    public void SetGrounded(bool grounded)
    {
        animator.SetBool(IsGrounded, grounded);
    }
    
    public void SetVerticalVelocity(float velocity)
    {
        animator.SetFloat(VerticalVelocity, velocity);
    }
    
    public void SetHorizontalVelocity(float velocity)
    {
        animator.SetFloat(HorizontalVelocity, velocity);
    }
}