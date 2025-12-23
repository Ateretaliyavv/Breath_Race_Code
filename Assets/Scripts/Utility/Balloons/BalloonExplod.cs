using System.Collections;
using UnityEngine;

/*
 * Script that handel the balloon expolosion by animator and go to surprise scene
 */
[RequireComponent(typeof(Animator))]
public class BalloonExplod : MonoBehaviour
{
    // Reference to the object that is allowed to trigger the balloon explosion
    [Header("Explosion Settings")]
    [SerializeField] private GameObject explodingObject;

    // Scale multiplier applied only when the balloon explodes to make the explosion visually larger
    [Header("Explosion Scale Effect")]
    [SerializeField] private float explosionScaleMultiplier = 1.5f;

    [Header("Surprise scene")]
    [SerializeField] private string sceneToLoad;

    [Header("Scene Transition Delay")]
    [SerializeField] private float sceneLoadDelay = 0.5f;

    private Animator animator;
    private bool hasExploded = false;

    private void Awake()
    {
        // Initialize references and ensure the balloon starts unexploaded
        animator = GetComponent<Animator>();
        animator.SetBool("IsExploded", false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore further collisions after the balloon has already exploded
        if (hasExploded)
            return;

        // React only to the specifically assigned exploding object
        if (other.gameObject != explodingObject)
            return;

        Explode();
    }

    private void Explode()
    {
        // Handles the full explosion sequence of the balloon
        hasExploded = true;
        transform.localScale *= explosionScaleMultiplier;
        animator.SetBool("IsExploded", true);
        // Start delayed actions
        StartCoroutine(SceneTransitionAfterDelay());
    }

    private IEnumerator SceneTransitionAfterDelay()
    {
        // Wait additional time before loading the next scene
        yield return new WaitForSeconds(sceneLoadDelay);
        SceneNavigator.LoadScene(sceneToLoad, markAsNextLevel: false);
    }
}
