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


    [Header("Player Abilities")]

    public bool canWallJump;
    public bool canJumpAfterWalljump;
    public bool canDoubleJump;
    public bool canTripleJump;


    [Header("Player Properties")]

    public float wallJumpX = 15f;
    public float wallJumpY = 15f;
    public float gravity = 20f;
    public float jumpSpeed = 15f;
    public float extraJumpSpeed = 10f;
    public float walkspeed = 10f;
    

    [Header("Input flags")]

    private bool _jumpButtonPressed;
    private bool _jumpButtonReleased;


    private CharacterController2D _characterController;  
    private Vector2 _input;                               // What we receive from our input system
                                                          // that will be translated into a moveDirection( see next line below)
    private Vector2 _moveDirection;


    void Start()
    {

        _characterController = gameObject.GetComponent<CharacterController2D>();

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
            isJumping = false;
            isWallJumping= false; 
            isDoubleJumping= false;
            isTripleJumping= false;
        
            if(_jumpButtonPressed)
            {
                
                _jumpButtonPressed= false;
                isJumping = true;
                _moveDirection.y = jumpSpeed; 
                _characterController.DisableCheckIfGrounded();
                
            }

        }
        else                                            
        // then player is in the air and we need to apply gravity                   
        {
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
            // Player pressed jump button while in the air

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

        _moveDirection.y -= gravity * Time.deltaTime;
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


}
