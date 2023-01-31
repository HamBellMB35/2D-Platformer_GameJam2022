using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player State")]

    public bool isJumping;
    public bool isWallJumping;
    public bool isDoubleJumping;
    public bool isTripleJumping;
    public bool isRuningUpWall;
    public bool isSlidingDownAWall;
    public bool isCrouching;
    public bool isCrouchingandMoving;

    [Header("Player Abilities")]

    public bool canWallJump;
    public bool canJumpAfterWalljump;
    public bool canDoubleJump;
    public bool canTripleJump;
    public bool canRunUpWall;
    public bool canRunUpManyWalls;
    public bool canSlideDownWalls;


    [Header("Player Properties")]

    public float upWalldistance = 8f;
    public float wallJumpX = 15f;
    public float wallJumpY = 15f;
    public float wallSlideDownDistance = 0.11f;
    public float gravity = 20f;
    public float jumpSpeed = 15f;
    public float extraJumpSpeed = 10f;
    public float walkspeed = 10f;
    public float crouchingMoveSpeed = 5f;
    

    [Header("Input flags")]

    private bool _jumpButtonPressed;
    private bool _jumpButtonReleased;
    private bool _RunUpWallEnabled = true;

    private CharacterController2D _characterController;  
    private CapsuleCollider2D _capsuleCollider;
    private Vector2 _initialCapsulecolliderSize;          // Reference to the original collider size is stored as a vector 2
    private Vector2 _input;                               // What we receive from our input system
                                                          // that will be translated into a moveDirection( see next line below)
    private Vector2 _moveDirection;
    private SpriteRenderer _spriteRenderer;                // Remove when not needed


    void Start()
    {
        _capsuleCollider = gameObject.GetComponent<CapsuleCollider2D>();
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        _characterController = gameObject.GetComponent<CharacterController2D>();
        _initialCapsulecolliderSize = _capsuleCollider.size;
    }

    void Update()
    {
        
        //Debug.Log("Force on y is " + _moveDirection.y);

        if(!isWallJumping)
        {
            _moveDirection.x = _input.x;
            _moveDirection.x *= walkspeed;

            if (_moveDirection.x < 0)                        // Conditional to make the player face direction based on input
            {
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            }

            else if (_moveDirection.x > 0)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }

        if (_characterController.somethingBelow)        // Player is on the ground layerMask
        {
                                                        
            _moveDirection.y = 0f;                     // Prevens downward force from accumulating once we jump from a plataform
                                                
            isJumping = false;                          // Booleans for air abilities
            isWallJumping= false; 
            isDoubleJumping= false;
            isTripleJumping= false;
        
            // Jumping parameters
            if(_jumpButtonPressed)
            {
                
                _jumpButtonPressed= false;
                isJumping = true;
                _moveDirection.y = jumpSpeed; 
                _characterController.DisableCheckIfGrounded();
                _RunUpWallEnabled = true;
                
            }

            // Crouching
            // STEPS:
            // Change the size of the capsule
            // Change the position of the capsule a little
            // Change the sprite *

            if (_input.y < 0)                           
            {                                           

                if(!isCrouching && !isCrouchingandMoving)
                {
                    _capsuleCollider.size = new Vector2(_capsuleCollider.size.x, _capsuleCollider.size.y/2);
                    transform.position = new Vector2(transform.position.x, transform.position.y - (_initialCapsulecolliderSize.y / 4));
                    isCrouching= true;
                    _spriteRenderer.sprite = Resources.Load<Sprite>("directionSpriteUp_crouching");
                }

            }

            else
            {
                if(isCrouching || isCrouchingandMoving)
                {
                    RaycastHit2D hitCeling = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, transform.localScale,
                                             CapsuleDirection2D.Vertical, 0, Vector2.up, _initialCapsulecolliderSize.y/2,
                                             _characterController.layerMask);

                    if(!hitCeling.collider)
                    {
                        _capsuleCollider.size = _initialCapsulecolliderSize;
                        transform.position = new Vector2(transform.position.x, transform.position.y + (_initialCapsulecolliderSize.y / 4));
                        _spriteRenderer.sprite = Resources.Load<Sprite>("directionSpriteUp");
                        isCrouchingandMoving = false;
                        isCrouching = false;

                    }
                    
                }
            }

            if(isCrouching && _moveDirection.x != 0)
            {
                isCrouchingandMoving= true;
            }

            else
            {
                isCrouchingandMoving= false;
            }

        }

        else                                            
        // then player is in the air and we need to apply gravity                   
        {

            if((isCrouching || isCrouchingandMoving) && _moveDirection.y > 0)
            {
                StartCoroutine("ResetCrouchingState");

            }



            if(_jumpButtonReleased)
            {
               // _jumpButtonPressed = false;
                _jumpButtonReleased = false;

                if(_moveDirection.y > 0)
                {
                    _moveDirection.y *= 0.5f;
                }
            }

            // Double and triple jumping ability code below
            // What happens if player pressed jump button while in the air

            if(_jumpButtonPressed)
            {
                // Conditionals for triple jumping
                if (canTripleJump && (!_characterController.contactLeft && !_characterController.contactRight))
                {
                
                    if(isDoubleJumping && !isTripleJumping) // If theplayer is double jumping but not triplejumping
                                                            // We check first or else the triple jum will fire automatically
                    {
                        _moveDirection.y = extraJumpSpeed;
                        isTripleJumping = true;
                    }
                
                }

                // Conditionals for double jumping
                if(canDoubleJump && (!_characterController.contactLeft && !_characterController.contactRight))
                {

                    if(!isDoubleJumping)                    // If its not doublejumping already
                                                           
                    {
                        _moveDirection.y = extraJumpSpeed;  // we apply doubleJump speed to the y axis
                        isDoubleJumping = true;
                    }

                }   
                
                // Conditionals for wall jumping
                if(canDoubleJump && _characterController.contactLeft || _characterController.contactRight)
                {
                    // Conditionals to handle wall jumping to the right
                    if(_moveDirection.x <= 0 && _characterController.contactLeft)
                    {
                        _moveDirection.x = wallJumpX;
                        _moveDirection.y = wallJumpY;
                        transform.rotation = Quaternion.Euler(0, 0, 0);
                    }
                    // Conditionals to handle wall jumping to the left
                    else if (_moveDirection.x >= 0 && _characterController.contactRight)
                    {
                        _moveDirection.x = -wallJumpX;
                        _moveDirection.y = wallJumpY;
                        transform.rotation = Quaternion.Euler(0,180,0);
                    }

                    //isWallJumping= true;

                    StartCoroutine("WallJumpTimer");

                    if(canJumpAfterWalljump)                // Resets the jump so we can double and
                    {                                       // triple jump after jumping off a wall

                        isDoubleJumping = false;
                        isTripleJumping= false;
                    }    
                }

                _jumpButtonPressed= false;
            }

            // The following code refers to running up walls

            if (canRunUpWall && (_characterController.contactLeft || _characterController.contactRight))
            {
                if (_input.y > 0 && _RunUpWallEnabled)
                {
                    _moveDirection.y = upWalldistance;

                    if (_characterController.contactLeft)
                    {
                        transform.rotation = Quaternion.Euler(0, 180, 0);
                    }

                    else if (_characterController.contactRight)
                    {
                        transform.rotation = Quaternion.Euler(0, 0, 0);
                    }

                    StartCoroutine("UpWallTimer");
                }
            }

            else
            {
                if (canRunUpManyWalls)
                {
                    StopCoroutine("UpWallTimer");
                    canRunUpManyWalls = true;
                    isRuningUpWall = false;
                }
            }

            CalculateGravity();
        }

        

        // Now we have our move direction on the x and y, we pass that to our character controler and the move method

        _characterController.Move(_moveDirection * Time.deltaTime);
    }

    private void CalculateGravity()
    {

        if(_moveDirection.y > 0 && _characterController.contactAbove)
        {
            _moveDirection.y = 0f;
        }

        if(canSlideDownWalls && (_characterController.contactLeft || _characterController.contactRight))
        {

            if(_characterController.playerhitWallOnFrame) 
            {
                _moveDirection.y = 0f;                          // 0s out movedirection on all
          
            }

            if(_moveDirection.y <= 0)
            {
                _moveDirection.y -= (gravity * wallSlideDownDistance) * Time.deltaTime;
            }

            else
            {
                _moveDirection.y -= gravity * Time.deltaTime;
            }
        }

        else
        {
            _moveDirection.y -= gravity * Time.deltaTime;
        }

       
    }


    // Input methods

    public void OnMovement(InputAction.CallbackContext context)
    {
        // This will represent any input we get on wasd keys or left hand stick

        _input = context.ReadValue<Vector2>();

    }


    public void Jump (InputAction.CallbackContext context)
    {

        if(context.started)
        {
            _jumpButtonPressed = true;
            _jumpButtonReleased= false;

        }

        else if(context.canceled)
        {
            _jumpButtonReleased = true;
            _jumpButtonPressed= false;
        }

    }

    // All coroutines will be in the section of code below

    IEnumerator WallJumpTimer()                         // Resets jump so we can change direction
    {                                                   // after wall jumping    
        isWallJumping= true;
        yield return new WaitForSeconds(0.4f);

        isWallJumping= false;
    }


    IEnumerator UpWallTimer()                           // Resets the ability to run up walls back to false
    {                                                   // after 0.5secs    
        isRuningUpWall= true;                               

        yield return new WaitForSeconds(0.5f);
        
        isRuningUpWall= false;

        if(!isWallJumping)
        {
            _RunUpWallEnabled = false;
        }
       
    }

    IEnumerator ResetCrouchingState()
    {
        yield return new WaitForSeconds(0.05f);


        RaycastHit2D hitCeling = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, transform.localScale,
                                            CapsuleDirection2D.Vertical, 0, Vector2.up, _initialCapsulecolliderSize.y / 2,
                                            _characterController.layerMask);

        if (!hitCeling.collider)
        {
            _capsuleCollider.size = _initialCapsulecolliderSize;
          //  transform.position = new Vector2(transform.position.x, transform.position.y + (_initialCapsulecolliderSize.y / 4));
            _spriteRenderer.sprite = Resources.Load<Sprite>("directionSpriteUp");
            isCrouchingandMoving = false;
            isCrouching = false;

        }
    }

}
