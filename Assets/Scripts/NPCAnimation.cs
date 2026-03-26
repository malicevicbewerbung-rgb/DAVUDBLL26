using UnityEngine;

public class NPCAnimation : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void StartTalking()
    {
        animator.SetBool("isTalking", true);
    }

    public void StopTalking()
    {
        animator.SetBool("isTalking", false);
    }
}