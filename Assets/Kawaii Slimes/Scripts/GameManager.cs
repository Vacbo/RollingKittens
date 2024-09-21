using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject mainSlime; // Reference to the player-controlled character
    public Button idleBut, walkBut, jumpBut, attackBut, damageBut0, damageBut1, damageBut2;
    public Camera cam; // Reference to the main camera
    private PlayerControllerAI playerController;

    private void Start()
    {
        // Get the PlayerController component from the player (mainSlime)
        playerController = mainSlime.GetComponent<PlayerControllerAI>();

        // Set up button listeners
        idleBut.onClick.AddListener(delegate { Idle(); });
        walkBut.onClick.AddListener(delegate { ChangeStateTo(SlimeAnimationState.Walk); });
        jumpBut.onClick.AddListener(delegate { LookAtCamera(); ChangeStateTo(SlimeAnimationState.Jump); });
        attackBut.onClick.AddListener(delegate { LookAtCamera(); ChangeStateTo(SlimeAnimationState.Attack); });
        damageBut0.onClick.AddListener(delegate { LookAtCamera(); SetDamageTypeAndChangeState(0); });
        damageBut1.onClick.AddListener(delegate { LookAtCamera(); SetDamageTypeAndChangeState(1); });
        damageBut2.onClick.AddListener(delegate { LookAtCamera(); SetDamageTypeAndChangeState(2); });
    }

    // Function to change to the Idle state
    void Idle()
    {
        LookAtCamera();
        ChangeStateTo(SlimeAnimationState.Idle);
    }

    // Change the player state by updating the state variable in PlayerController
    public void ChangeStateTo(SlimeAnimationState state)
    {
        if (mainSlime == null || playerController == null) return;

        // Avoid unnecessary state changes
        if (state == playerController.currentState) return;

        // Change the player's current animation state
        playerController.currentState = state;
    }

    // Make the character look at the camera
    void LookAtCamera()
    {
        Vector3 lookAtPosition = cam.transform.position;
        lookAtPosition.y = mainSlime.transform.position.y; // Lock the Y axis to avoid tilting up/down
        mainSlime.transform.LookAt(lookAtPosition);
    }

    // Set the damage type and change the state to Damage
    void SetDamageTypeAndChangeState(int damageType)
    {
        if (playerController == null) return;

        // Set the damage type in PlayerController
        playerController.damType = damageType;

        // Change the state to Damage
        ChangeStateTo(SlimeAnimationState.Damage);
    }
}