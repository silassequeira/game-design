using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer spr;
    float moveForce=2;
    float jumpForce=7;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask Ground;
    bool isGrounded;
    public Sprite newSprite; 
     bool spriteChanged = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb=GetComponent<Rigidbody2D>();
        anim=GetComponent<Animator>();
        spr=GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        rb.AddForce(Vector2.right*moveForce*Input.GetAxis("Horizontal"), ForceMode2D.Force);
        if(rb.linearVelocity.magnitude>0.01f){
            anim.SetBool("isWalking",true);
        }
        else{
            anim.SetBool("isWalking",false);
        }
        if(rb.linearVelocity.x<0){
            spr.flipX=true;
        }
        else{
            spr.flipX=false;
        }

        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, Ground);

        if(Input.GetKeyDown(KeyCode.UpArrow) && isGrounded){
            rb.AddForce(transform.up*jumpForce, ForceMode2D.Impulse);
            anim.SetTrigger("Jump");
        }
        if(Input.GetKeyDown(KeyCode.DownArrow)){
            anim.SetTrigger("Duck");
        }

         if (!spriteChanged && transform.position.x >= 64f)
        {
            spr.sprite = newSprite;
            spriteChanged = true; // Only change once!
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
