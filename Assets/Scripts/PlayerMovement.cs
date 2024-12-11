using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D _rigidbody;
    private Animator _anim;

    [Header("Move info")]
    [SerializeField] public float speed;
                     private float horizontalInput;

    [Header("Jump")]
    [SerializeField] public float jumpForce;
                     private bool canDoubleJump;
    

    [Header("Wall Sliding")]
    private bool canWallSlide;
    private bool isWallSliding;

    [Header("Wall Jumping")]
    private bool isWallJumping;
    private float wallJumpDirection;
    private float wallJumpTime = 0.2f;
    private float wallJumpCounter;
    private float wallJumpDuration = 0.4f;
    public Vector2 wallJumpForce = new Vector2(5f, 5f);
    
   
    [Header("Collision info")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private LayerMask whatIsGround;
                     private bool isGrounded;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckDistance;
    private bool isWallDetected;


     // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        Jump();
        WallJump();
        CollisionCheck();
        Flip();
        Animation();
        if (!isWallJumping)
        {
            Flip();
        }
    }

    private void FixedUpdate()
    {
        if (isGrounded)
        {   
            canDoubleJump = true;           
        }

        if (isWallDetected && canWallSlide)
        {
            isWallSliding = true;
            _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, _rigidbody.velocity.y * 0.1f);
        }
        else
        {
            isWallSliding = false;
            Move();
        }

        if (!isWallJumping)
        {
            Move();
        }
    }


    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (isGrounded)
            { 
                _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, jumpForce);
            }
            else if (canDoubleJump)
            {
                _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, jumpForce);
                canDoubleJump = false;
            }
        }
    }

    private void WallJump()
    {
        if (isWallSliding == true)
        {
            isWallJumping = false;
            wallJumpDirection = -transform.localScale.x;
            wallJumpCounter = wallJumpTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpCounter -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) && wallJumpCounter > 0f)
        {
            isWallJumping = true;
            _rigidbody.velocity = new Vector2(wallJumpDirection * wallJumpForce.x, wallJumpForce.y);
            wallJumpCounter = 0f;

            if (transform.localScale.x != wallJumpDirection)
            {
                if (horizontalInput > 0.01f)
                {
                    transform.localScale = Vector3.one;
                }
                else if (horizontalInput < -0.01f)
                {
                    transform.localScale = new Vector3(-1, 1, 1);
                }
            }
            Invoke(nameof(StopWallJumping), wallJumpDuration);
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }


    private void Move()
    {
        _rigidbody.velocity = new Vector2(horizontalInput * speed, _rigidbody.velocity.y);
    }

    private void Flip()
    {
        //facingDirection = facingDirection * -1;
        if (horizontalInput > 0.01f)
        {
            transform.localScale = Vector3.one;
        }
        else if (horizontalInput < -0.01f)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void Animation()
    {
        bool isMoving = _rigidbody.velocity.x != 0;

        _anim.SetFloat("yVelocity", _rigidbody.velocity.y);
        _anim.SetBool("isMoving", isMoving);
        _anim.SetBool("isGrounded", isGrounded);
        _anim.SetBool("isWallSliding", isWallSliding);
    }

    private void CollisionCheck()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
        isWallDetected = Physics2D.Raycast(wallCheck.position, Vector2.right, wallCheckDistance, whatIsGround);

        if (!isGrounded && _rigidbody.velocity.y < 0) 
        {
            canWallSlide = true;
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance,
                                                        wallCheck.position.y, 
                                                        wallCheck.position.z));
    }
}
