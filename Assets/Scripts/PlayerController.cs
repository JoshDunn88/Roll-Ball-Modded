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
    private float movementY;
    private float movementZ;
    private float turn;
    private float flip;
    public float speed = 0;
    public float jumpPower = 0.0f;
    public float shotJumpPower = 0.0f;
    public float flipSpeed = 0.0f;
    private bool canShotJump;
    private bool landed;
    public TextMeshProUGUI countText;
    public GameObject winTextObject;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        count = 0;
        SetCountText();
        winTextObject.SetActive(false);
        canShotJump = false;
        landed = false;
    }
    private void FixedUpdate()
    {
        Vector3 movement = new Vector3(movementX, 0.0f, movementZ);
        //changed to relative force
       // rb.AddRelativeForce(movement * speed);
        //rotate tank
       // transform.Rotate(new Vector3(flip, turn, 0));
        //for groound detection
       // movementY = rb.velocity.y;

        //reset flip on landing for non-flying abilities
        if(IsGrounded())
        {
            canShotJump = true;
           // transform.Rotate(new Vector3(0, turn, 0));
           //had to look up this nonsense
            if (landed==false)
                transform.rotation = Quaternion.Euler(new Vector3(0, transform.localRotation.eulerAngles.y, 0));
            transform.Rotate(new Vector3(0, turn, 0));
            landed = true;
            //transform.rotation.x = 0f;
            // transform.position.Set(transform.position.x, 0.0f, transform.position.z);
           // print(movement);
                 rb.AddRelativeForce(movement * speed);
                transform.Rotate(new Vector3(0, turn, 0));

        }
        else
        {
            //canShotJump = true;
            transform.Rotate(new Vector3(flip, 0, 0));
            landed = false;
            

        }

        
        //gone to hell detection
        if (transform.position.y <= -20.0f)
        {
            rb.velocity.Set(0.0f, 0.0f, 0.0f);
            transform.position = new Vector3(0.0f, 5.0f, 0.0f);

        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PickUp"))
        {
            other.gameObject.SetActive(false);
            count+=1;
            SetCountText();
            if (count >= 4)
            {
                winTextObject.SetActive(true);
            }
        }
    }

    void OnMove (InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();

        if (IsGrounded()/* && transform.rotation.x==0*/){
            movementX = movementVector.x;
            movementZ = movementVector.y;
            print(movementX + ": " + movementZ);
        }
        else {
            movementZ = 0f;
            movementZ = 0f;
        }

    }
    void OnJump()
    {
        if (IsGrounded())
        {
            rb.AddForce(0.0f, jumpPower, 0.0f);
        }
        if (!IsGrounded() && canShotJump)
        {
            rb.AddRelativeForce(0.0f, 0.0f, -shotJumpPower);
            canShotJump = false;
        }
    }
    void OnRotate (InputValue turnValue)
    {
        Vector2 turnVector = turnValue.Get<Vector2>();
        if (IsGrounded())
        {
            turn = turnVector.x;
            flip = 0.0f;
        }

        else
        {
            flip = turnVector.y * flipSpeed;
            turn = 0f;
        }
        

    }
    void SetCountText()
    {
        countText.text = "Count: " + count.ToString();
    }

    //temporary solution grabbed from https://discussions.unity.com/t/how-can-i-check-if-my-rigidbody-player-is-grounded/256346/2
    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, 1.25f);
    }

}
