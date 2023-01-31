using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalTypes;
using Unity.VisualScripting;

public class CharacterController2D : MonoBehaviour
{
    private Vector2 _moveAmount;    
    private Vector2 _currentPos;
    private Vector2 _lastPos;

    private Rigidbody2D _rigigbody;
    private CapsuleCollider2D _capsuleCollider;
    

    public LayerMask layerMask;
    public float raycastDistance = 0.2f;
    
    private RaycastHit2D[] _raycastHits = new RaycastHit2D[3];
    private Vector2[] _raycastPos = new Vector2[3];

    public GroundType groundType;            // Used to report back what kind of ground the player is standing on
    public bool playerHitsGroundOnFrame;     // Boolean that gets set to true the exact frame that we hit the ground
    public bool playerhitWallOnFrame;        // Boolean that gets set to true the exat frame the player hits a wall
    public bool somethingBelow;              // If true there is something below us if false then there is nothing below
    private bool _disableGroundCheck;
    private bool _lastFrameInAir;
    private bool _noSideCollisionsOnLastFrame;



    // TODO: Change to private
    public Vector2 _slopeNormalVector;
    public float _angleOfSlope;
    public float angleOfSlopeLimit = 35f;
    public float downForceIncrement = 1.2f;
    private float ContactExtender = 2f;
    public bool contactAbove;
    public bool contactLeft;
    public bool contactRight;



    // Start is called before the first frame update
    void Start()
    {

        _rigigbody = gameObject.GetComponent<Rigidbody2D>();

        _capsuleCollider = gameObject.GetComponent<CapsuleCollider2D>();

        
    }

    // FixedUpdate is tied to physics simulations, it only runs a set number of times per second
    void Update()
    {

        _lastFrameInAir = !somethingBelow;

        _noSideCollisionsOnLastFrame = (!contactRight && !contactLeft);

        _lastPos = _rigigbody.position;         //Where the player is at the start of the loop/begginig of the frame

        if(_angleOfSlope != 0 && somethingBelow == true)
        {

            if((_moveAmount.x > 0f && _angleOfSlope > 0f ) || _moveAmount.x < 0f && _angleOfSlope < 0)
            {

                _moveAmount.y = - Mathf.Abs(Mathf.Tan(_angleOfSlope * Mathf.Deg2Rad) * _moveAmount.x);

              //  _moveAmount.y *= downForceIncrement;
            }


        }

        _currentPos = _lastPos + _moveAmount;   // The position where we want to be = last position + move amount ( from player controller)

        _rigigbody.MovePosition(_currentPos);   // Get reference to rigibody moveposition set to current position/ performs physics calcualtions and
                                                // interactions with other objects as it translates

        _moveAmount = Vector2.zero;             // Reset _moveAmount after its done i

        if(!_disableGroundCheck)
        {
            CheckIfGrounded();
        }

        CheckOtherContacts();

        if(somethingBelow && _lastFrameInAir)                                   // This code is to determine the exact frame where
        {                                                                       // the player hit the ground

            playerHitsGroundOnFrame = true;
        }
        else
        {
            playerHitsGroundOnFrame= false;
        }

        if((contactLeft || contactRight) && _noSideCollisionsOnLastFrame)       // This code is to determine the exact frame where
        {                                                                       // the player hit a wall

            playerhitWallOnFrame = true;

        }
        else
        {
            playerhitWallOnFrame = false;
        }


    }


    //  Move method will be called by Update loop
    public void Move(Vector2 movement)
    {
        _moveAmount += movement;                // Movement equals a cumulative move amount from the Update loop
    }
     
    private void CheckIfGrounded()
    {
        RaycastHit2D hit = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, _capsuleCollider.size, CapsuleDirection2D.Vertical,
            0f, Vector2.down, raycastDistance, layerMask);

        if(hit.collider)
        {

            groundType = GetGroundType(hit.collider);

            _slopeNormalVector = hit.normal;

            _angleOfSlope = Vector2.SignedAngle(_slopeNormalVector, Vector2.up);

            if(_angleOfSlope > angleOfSlopeLimit || _angleOfSlope < -angleOfSlopeLimit )
            {
                somethingBelow = false;
            }

            else
            {
                somethingBelow = true;
            }

        }

        else
        {
            groundType = GroundType.None;

            somethingBelow= false;
        }


    }

    private void CheckOtherContacts()
    {
        // check on the left


        // Box cast created
        RaycastHit2D leftHit = Physics2D.BoxCast(_capsuleCollider.bounds.center, _capsuleCollider.size * 0.75f, 0f, 
            Vector2.left, raycastDistance * ContactExtender, layerMask);
       
        // Now we check if it hit anything        
        if(leftHit.collider)
        {

            contactLeft = true;
        }

        else
        { 
            contactLeft = false;        
        }


        // check on the right

        // Box cast created
        RaycastHit2D rightHit = Physics2D.BoxCast(_capsuleCollider.bounds.center, _capsuleCollider.size * 0.75f, 0f,
            Vector2.right, raycastDistance * ContactExtender, layerMask);

        // Now we check if it hit anything        
        if (rightHit.collider)
        {

            contactRight = true;
        }

        else
        {
            contactRight = false;
        }


        // check above

        RaycastHit2D aboveHit = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, _capsuleCollider.size, CapsuleDirection2D.Vertical,
            0f, Vector2.up, raycastDistance, layerMask);

        if(aboveHit.collider)
        {
            contactAbove = true;
        }

        else
        {
            contactAbove = false; 
        }


    }


    /*
    private void CheckIfGrounded()
    {

        Vector2 rcasttOrigin = _rigigbody.position - new Vector2(0, _capsuleCollider.size.y / 2f); // center of capsule collider

        _raycastPos[0] = rcasttOrigin + (Vector2.left * _capsuleCollider.size.x / 4f + new Vector2(0f,0.1f)); // left most raycast

        _raycastPos[1] = rcasttOrigin;          // center raycast

        _raycastPos[2] = rcasttOrigin + (Vector2.right * _capsuleCollider.size.x / 4f + new Vector2(0f, 0.1f)); // right most raycast

        DrawDebugRays(Vector2.down, Color.green);


        DrawDebugRays(Vector2.down, Color.green);

        int numberOfGroundHits = 0;

        for (int i = 0; i < _raycastPos.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(_raycastPos[i], Vector2.down, raycastDistance, layerMask);

            if (hit.collider)
            {
                _raycastHits[i] = hit;
                numberOfGroundHits++;
            }
        }

        if (numberOfGroundHits > 0)
        {
            if (_raycastHits[1].collider)
            {
                groundType = GetGroundType(_raycastHits[1].collider);
                _slopeNormalVector = _raycastHits[1].normal;
                _angleOfSlope = Vector2.SignedAngle(_slopeNormalVector, Vector2.up);
            }
            else
            {
                for (int i = 0; i < _raycastHits.Length; i++)
                {
                    if (_raycastHits[i].collider)
                    {
                        groundType = GetGroundType(_raycastHits[i].collider);
                        _slopeNormalVector = _raycastHits[i].normal;
                        _angleOfSlope = Vector2.SignedAngle(_slopeNormalVector, Vector2.up);
                    }
                }
            }

            if (_angleOfSlope > angleOfSlopeLimit || _angleOfSlope < -angleOfSlopeLimit)
            {
                somethingBelow = false;
            }
            else
            {
                somethingBelow = true;
            }

        }
        else
        {
            groundType = GroundType.None;
            somethingBelow = false;
        }

        System.Array.Clear(_raycastHits, 0, _raycastHits.Length);

    }

    */

    private void DrawDebugRays(Vector2 direction, Color color)
    {
        for (int i = 0; i < _raycastPos.Length; i++)
        {
            Debug.DrawRay(_raycastPos[i], direction * raycastDistance, color);
        }
    }

    public void DisableCheckIfGrounded()
    {
        somethingBelow = false;
        _disableGroundCheck = true;

        StartCoroutine("EnableGroundCheck");
    }

    IEnumerator EnableGroundCheck()
    {
        yield return new WaitForSeconds(0.1f);
        _disableGroundCheck = false;
    }

    private GroundType GetGroundType(Collider2D collider)
    {
        if(collider.GetComponent<GroundEffectScript>())
        {
            GroundEffectScript groundeffect = collider.GetComponent<GroundEffectScript>();
            return groundeffect.groundType;
        }

        else
        {
            return GroundType.LevelGeom;
        }
    }
}
