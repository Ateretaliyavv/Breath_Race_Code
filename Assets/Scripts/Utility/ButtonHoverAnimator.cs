using UnityEngine;

public class ButtonHoverAnimator : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void OnHoverEnter()
    {
        animator.SetBool("isHover", true);
    }

    public void OnHoverExit()
    {
        animator.SetBool("isHover", false);
    }
}
