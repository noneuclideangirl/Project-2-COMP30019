﻿using UnityEngine;
using System.Collections;

public abstract class PlayerBehaviour : MonoBehaviour {
    void OnControllerColliderHit(ControllerColliderHit hit) {
        // Separate into two logical cases for platforming
        if (hit.normal == Vector3.up || hit.normal == Vector3.down) {
            OnUpDownCollision(hit);
        } else {
            OnOtherCollision(hit);
        }
    }

    protected virtual void OnUpDownCollision(ControllerColliderHit hit) { }
    protected virtual void OnOtherCollision(ControllerColliderHit hit) { }
}

[RequireComponent(typeof(CharacterController),
                  typeof(PlayerStamina),
                  typeof(PlayerJump))]
public class PlayerMove : PlayerBehaviour {
    public float Speed = 4;
    public float YSpeed { get; set; }

    public float RollTime = 0.4f;
    public float RollSpeed = 8;

    public float RollStaminaCost = 40;
    
    private Vector3 destination;
    // Unit vector that stores the most recent movement direction
    private Vector3 direction = Vector3.forward;
    public Vector3 Direction { get { return direction; } set { direction = value; } }

    private CharacterController controller;
    private PlayerStamina playerStamina;
    private PlayerJump playerJump;

    // Only check for Collision objects
    private int CollisionMask;

    // State variables
    private bool rolling = false;
    public bool Rolling { get { return rolling; } }

    private bool moving = false;
    public bool Moving { get { return moving; } }

    public bool AutoMove { get; set; }
    public float AutoMoveSpeed { get; set; }

    // Coroutine to roll for a given period
    private IEnumerator Roll() {
        if (!rolling && playerStamina.DeductStamina(RollStaminaCost)) {
            AutoMove = true;
            AutoMoveSpeed = RollSpeed;
            rolling = true;
            // Roll for a certain time period
            yield return new WaitForSeconds(RollTime);
            StopRoll();
        }
        yield break;
    }

    private void StopRoll() {
        rolling = false;
        AutoMove = false;
        // Update destination (if we weren't still moving)
        if (!moving) {
            destination = transform.position;
        }
    }

    // If we hit a wall
    override protected void OnOtherCollision(ControllerColliderHit hit) {
        if (rolling) {
            StopRoll();
            // Rolling into a wall means we should stop moving
            destination = transform.position;
            if (moving) {
                //// TODO: Generate impact with wall here
            }
        }
    }
    
    void Start() {
        destination = transform.position;
        controller = GetComponent<CharacterController>();
        playerStamina = GetComponent<PlayerStamina>();
        playerJump = GetComponent<PlayerJump>();
            
        CollisionMask = 1 << LayerMask.NameToLayer("Collision");
    }
    
    private void SetDestination() {
        if (!rolling) {
            // Raycast to find where the user clicked
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, CollisionMask)) {
                destination = new Vector3(hit.point.x,
                                          transform.position.y,
                                          hit.point.z);
                direction = (destination - transform.position).normalized;
            }
            moving = true;
            // Take back control of movement
            AutoMove = false;
            playerJump.TouchedWall = false;
        }
    }
    
    private void MoveToDestination() {
        // Move only in x and z
        destination.Set(destination.x,
                        transform.position.y,
                        destination.z);
        // If we're more than a given distance away, move closer
        if (moving && (destination - transform.position).magnitude > Speed * Time.fixedDeltaTime) {
            // Don't actually move if another routine has control, but don't stop from moving afterwards
            if (!AutoMove) {
                direction = (destination - transform.position).normalized;
                controller.Move(direction * Speed * Time.fixedDeltaTime);
            }
        } else {
            // issue with rolling here
            destination = transform.position;
            moving = false;
        }
    }
        
    void Update() {
        if (Input.GetKeyDown(KeyCode.X)) {
            StartCoroutine("Roll");
        }

        // On left mouse click
        if (Input.GetMouseButtonDown(0)) {
            SetDestination();
        }

        // Kill non-roll auto move on the ground
        if (AutoMove && !rolling && controller.isGrounded) {
            AutoMove = false;
            destination = transform.position;
        }
        Debug.Log(SystemInfo.supportsAccelerometer);
    }

    // Do physics here
    void FixedUpdate() {
        if (AutoMove) {
            // Move in a fixed direction if we're rolling
            controller.Move(direction * AutoMoveSpeed * Time.fixedDeltaTime);
        }
        MoveToDestination();
        // isGrounded fails if Move isn't handled like this. Set to 0 to allow superposition of velocity
        controller.Move(YSpeed * Vector3.up * Time.fixedDeltaTime);
        YSpeed = 0;
    }
}
