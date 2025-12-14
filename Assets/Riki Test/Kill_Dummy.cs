using UnityEngine;

public class DummyTrigger : MonoBehaviour
{
    private Animator animator;
    private bool hitState = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            hitState = !hitState;
            animator.SetBool("Hit_Dummy", hitState);

            Debug.Log("Hit_Dummy = " + hitState);
        }
    }
}
