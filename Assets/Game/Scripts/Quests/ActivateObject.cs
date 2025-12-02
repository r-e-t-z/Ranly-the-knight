using UnityEngine;

public class ActivateObject : MonoBehaviour
{
    public GameObject objectToActivate;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            objectToActivate.SetActive(true);
        }
    }
}
