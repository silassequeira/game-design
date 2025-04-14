using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer spr;
    float moveForce=2;
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
    }
}
