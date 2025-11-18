using UnityEngine;
using System.Collections;

public class ForestExitTrigger : MonoBehaviour
{
    [Header("Точка возврата")]
    public Transform returnPoint;

    [Header("Диалоги")]
    public TextAsset dialogue1;
    public TextAsset dialogue2;
    public TextAsset dialogue3;

    [Header("Настройки персонажей")]
    public string leftCharacterName = "";
    public Sprite leftCharacterPortrait;
    public string rightCharacterName = "";
    public Sprite rightCharacterPortrait;

    [Header("Движение")]
    public float moveSpeed = 3f;

    private int exitAttempts = 0;
    private PlayerMovement playerController;
    private SpriteRenderer playerSprite;


    void Start()
    {
        playerController = FindObjectOfType<PlayerMovement>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerSprite = player.GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && exitAttempts <3)
        {
            exitAttempts++;
            StartCoroutine(ReturnPlayer(other.gameObject));
            
        }
    }

    IEnumerator ReturnPlayer(GameObject player)
    {
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        yield return new WaitForSeconds(0.3f);
        while(Vector3.Distance(player.transform.position, returnPoint.position)> 0.1f)
        {
            Vector3 direction = (returnPoint.position - player.transform.position).normalized;
            player.transform.position += direction * moveSpeed * Time.deltaTime;

            UpdatePlayerSprite(direction);

            yield return null;
        }

        if(playerController != null)
        {
            playerController.enabled = true;
        }

        StartAnDialogue();
        
    }

    void UpdatePlayerSprite(Vector3 direction)
    {
        if(Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if(direction.x > 0)
            {
                playerSprite.sprite = playerController.rightsprite;
            }
            else
            {
                playerSprite.sprite = playerController.leftsprite;

            }
        }
        else
        {
            if (direction.y > 0)
            {
                playerSprite.sprite = playerController.backsprite;
            }
            else
            {
                playerSprite.sprite = playerController.frontsprite;

            }
        }
    }

    void StartAnDialogue()
    {
        TextAsset dialogueToPlay = GetDialogue();

        DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager != null && dialogueToPlay != null)
        {
            dialogueManager.StartDialogue(
                inkJSON: dialogueToPlay,
                leftCharName: leftCharacterName,
                leftCharPortrait: leftCharacterPortrait,
                rightCharName: rightCharacterName,
                rightCharPortrait: rightCharacterPortrait
            );
        }
    }

    TextAsset GetDialogue()
    {
        if (exitAttempts == 1 && dialogue1 != null)
            return dialogue1;
        else if (exitAttempts == 2 && dialogue2 != null)
            return dialogue2;
        else if (dialogue3 != null)
            return dialogue3;
        else
            return dialogue1;
    }
}