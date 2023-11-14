using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using static System.Net.WebRequestMethods;

public class PlayerController : MonoBehaviour
{

    private Rigidbody rb;
    private int count;
    private float movementX;
    private float movementZ;
    private float turn;
    private float flip;
    public float speed = 0;
    public float jumpPower = 0.0f;
    public float shotJumpPower = 0.0f;
    public float flipSpeed = 0.0f;

    private bool canShotJump;
    private bool landed;

    private int shootTime;
    private int shotRechargeTimer;
    private int rechargeLength = 80;

    private Vector3 tankFront;
    private Vector3 tankBack;
    private Vector3 tankLeft;
    private Vector3 tankRight;

    public TextMeshProUGUI countText;
    public GameObject winTextObject;
    public GameObject rechargeTextObject;
    public GameObject shotObject;
    public GameObject finishLine;


    // Start is called before the first frame update
    void Start()
    {
        //set default states of everything
        rb = GetComponent<Rigidbody>();
        count = 0;
        SetCountText();
        winTextObject.SetActive(false);
        rechargeTextObject.SetActive(true);
        canShotJump = false;
        landed = false;
        shotRechargeTimer = 0;
        shotObject.SetActive(false);
        finishLine.SetActive(false);
    }
    private void FixedUpdate()
    {
        //for ground movement
        Vector3 movement = new Vector3(movementX, 0.0f, movementZ);
        
        //for ground collision raycasts later
        tankBack = transform.position + new Vector3(0f, 0f, 3f);
        tankFront = transform.position + new Vector3(0f, 0f, -3f);
        tankLeft = transform.position + new Vector3(-2f, 0f, 0f);
        tankRight = transform.position + new Vector3(2f, 0f, 0f);

        //if grounded you can move and turn
        if (IsGrounded())
        {
            //reset flip on landing for non-flying abilities
            //had to look up nonsensical quaternion witchcraft, I get the euler stuff but this syntax is ungodly
            if (landed==false )
            {
                transform.rotation = Quaternion.Euler(new Vector3(0, transform.localRotation.eulerAngles.y, 0));
                
            }
            transform.Rotate(new Vector3(0, turn, 0));
            landed = true;
            //changed to relative force, otherwise same as Roll-A-Ball
            rb.AddRelativeForce(movement * speed);
            transform.Rotate(new Vector3(0, turn, 0));
        }
        //if not grounded you can flip
        else
        {
            transform.Rotate(new Vector3(flip, 0, 0));
            landed = false;     

        }

        //timing of shot flash
        if (shootTime == 3)
        {
            shootTime = 0;
            shotObject.SetActive(false);
        }
        
        //nevernding if statements, CS100 style
        if (shotObject.activeSelf) {
            shootTime++;
        }

        //recharges shot
        if (shotRechargeTimer > 0)
        {
            shotRechargeTimer--;
        }
        else
        {
            canShotJump = true;
            rechargeTextObject.SetActive(true);
        }

        
        //gone to hell detection
        if (transform.position.y <= -21.0f)
        {
            rb.velocity.Set(0.0f, 0.0f, 0.0f);
            transform.position = new Vector3(0.0f, 5.0f, 0.0f);
        }

    }

    //modified to set finish gate active instead of instant win, also added detection for win gate.
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PickUp") && shootTime==0)
        {

            other.gameObject.SetActive(false);
            count+=1;
            SetCountText();
            if (count >= 12)
            {
                finishLine.SetActive(true);
            }
        }
        else if (other.gameObject.CompareTag("Exit") && shootTime == 0)
        {
            winTextObject.SetActive(true);
        }
    }

    // The move function has only been changed to detect if the player is on the ground or not.
    // Since I use relative force and rotation, this was necessary to avoid the" Chitty Chitty Bang Bang" incident of '68
    void OnMove (InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();

        if (IsGrounded())
        {
            movementX = movementVector.x;
            //named this more appropriately for 3D space 
            movementZ = movementVector.y;
        }
        else {
            movementZ = 0f;
            movementZ = 0f;
        }

    }
    // Can jump on ground and fire in air with Spacebar
    void OnJump()
    {
        if (IsGrounded())
        {
            rb.AddForce(0.0f, jumpPower, 0.0f);
        }
        if (!IsGrounded() && canShotJump)
        {
            //do shooting stuff, this should be a function but oh well
            // Nevermind, I made it a function
            shoot();

        }
    }
    //lets you fire on ground with F only
    void OnFire()
    {
        if (canShotJump)
        {
            shoot();
        }
        else
        {
            print("cannot shoot yet");
        }
    }

    //If grounded then allow turning but prohibit flipping, else vice versa. The velocity check may be redundant but I forgot so I'm leaving it there.
    void OnRotate (InputValue turnValue)
    {
        Vector2 turnVector = turnValue.Get<Vector2>();
        if ((IsGrounded() || rb.velocity.y==0) )
        {
            turn = turnVector.x;
            flip = 0.0f;
        }

        if (!IsGrounded())
        {
            flip = turnVector.y * flipSpeed;
            turn = 0f;
        }
        

    }
    //Modified to display amount needed, and change objective on full collection.
    void SetCountText()
    {   if (count < 12)
            countText.text = "Count: " + count.ToString() + "/12";
        else
            countText.text = "To the finish line!";
    }

    // part of this solution (the raycast syntax) grabbed from https://discussions.unity.com/t/how-can-i-check-if-my-rigidbody-player-is-grounded/256346/2
    // tried not using raycast first but they all sucked, this does too.
    // the multiples are to try and help the player from getting stuck on edges but
    // luckily the flip can help out in those remaining situations
    //going to try sphere collision in the future
    bool IsGrounded()
    {
        return (Physics.Raycast(transform.position, -Vector3.up, 1.25f)|| 
                 Physics.Raycast(tankFront, -Vector3.up, 1.25f) || 
                 Physics.Raycast(tankBack, -Vector3.up, 1.25f)||
                 Physics.Raycast(tankLeft, -Vector3.up, 1.25f) ||
                 Physics.Raycast(tankRight, -Vector3.up, 1.25f) 
                 );
    }

    //do shooty stuff
    void shoot()
    {
        //recoil moment
        rb.AddRelativeForce(0.0f, 0.0f, -shotJumpPower);
        //show very poorly made and oddly short range krakatoa
        shotObject.SetActive(true);
        //reset shot recharge 
        canShotJump = false;
        rechargeTextObject.SetActive(false);
        shotRechargeTimer = rechargeLength;
    }

}
