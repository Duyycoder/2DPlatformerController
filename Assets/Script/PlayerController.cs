using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private float movementInputDerection;
    private Rigidbody2D rb;
    private bool isFacingRight = true;
    private int amountOfJumpLeft;
    private int facingDirection = 1;
    
    private bool isWalking;
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool canJump;
    public int amountOfJump = 1;
    
    private Animator anim;


    
    public float groundCheckRadious;
    public float movemetSpeed = 10.0f;
    public float jumpForce = 16.0f;
    
    
    
    public Vector2 wallHopDirection;
    public Vector2 wallJumpDirection;
    public LayerMask whatIsGround;
    public Transform groundCheck;
    public float wallCheckDistance;
    public float wallSlideSpeed;
    public float movementForceInAir;
    public float airDragMultiplier = 0.95f;
    public float variableJumpHeightMultiplier = 0.5f;
    public float wallHopForce;
    public float wallJumpForce;
    

    public Transform wallCheck; 
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        amountOfJumpLeft = amountOfJump;
        wallJumpDirection.Normalize();
        wallHopDirection.Normalize();   

    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
        CheckMovementDirection();
        UpdateAnimations();
        CheckIfCanJump();
        CheckIfWallSliding();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        CheckSurroundings();
    }

    private void CheckInput()
    {   
        movementInputDerection = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }

        if (Input.GetButtonUp("Jump"))
        {
            rb.velocity= new Vector2(rb.velocity.x, rb.velocity.y * variableJumpHeightMultiplier);
        }
    }

    private void CheckSurroundings()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadious, whatIsGround);


        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);
    }

    private void CheckMovementDirection()
    {
        if (isFacingRight && movementInputDerection < 0 )
        {
            Flip();
        } else if (!isFacingRight && movementInputDerection > 0)
        {
            Flip();
        }

        if (rb.velocity.x != 0)
        { 
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }
    }
    private void ApplyMovement()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector2(movemetSpeed * movementInputDerection, rb.velocity.y);
        }else if (!isGrounded && !isWallSliding && movementInputDerection != 0)
        {
            Vector2 forceToAdd = new Vector2(movementForceInAir * movementInputDerection, 0);
            rb.AddForce(forceToAdd);
            if (Mathf.Abs(rb.velocity.x)>movemetSpeed)
            {
                rb.velocity = new Vector2(movemetSpeed * movementInputDerection, rb.velocity.y);
            }
        } else if (!isGrounded && !isWallSliding && movementInputDerection == 0)
        {
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);
        }
        

        if (isWallSliding)
        {
            if (rb.velocity.y < -wallSlideSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, - wallSlideSpeed);
            }
        }
    }

    private void UpdateAnimations()
    {
        anim.SetBool("isWalking",isWalking);
        anim.SetBool("isGrounded",isGrounded);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isWallSliding", isWallSliding);
    }
    private void Flip()
    {
        facingDirection *= -1;
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f,0f);
    }

    private void Jump()
    {
        if (canJump && !isWallSliding)
        {
           rb.velocity = new Vector2(rb.velocity.x, jumpForce);
           amountOfJumpLeft--;
        }
        else if(isWallSliding && movementInputDerection == 0 && canJump)
        {
            isWallSliding = false;
            amountOfJumpLeft--;
            Vector2 forceToAdd = new Vector2(wallHopForce * wallHopDirection.x * -facingDirection, wallHopDirection.y * wallHopForce);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
        } else if((isWallSliding || isTouchingWall) && movementInputDerection != 0 && canJump)
        {
            isWallSliding = false;
            amountOfJumpLeft--;
            Vector2 forceToAdd = new Vector2(wallHopForce * wallJumpDirection.x * movementInputDerection, wallJumpDirection.y * wallJumpForce);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
        }
        
    }

    private void CheckIfCanJump()
    {
        if ((isGrounded && rb.velocity.y <= 0) || isWallSliding )
        {
            amountOfJumpLeft = amountOfJump;
        }

        if (amountOfJumpLeft<=0)
        {
            canJump = false;
        }
        else
        {
            canJump = true;
        }
    }

    private void CheckIfWallSliding()
    {
        if (isTouchingWall && !isGrounded && rb.velocity.y < 0)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;  
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position,groundCheckRadious);
        
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y,wallCheck.position.z));
    }
}
