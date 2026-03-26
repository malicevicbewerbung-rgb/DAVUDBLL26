using UnityEngine;

public class ArgumentInteraction : MonoBehaviour
{
    public ArgumentNPC ownerNPC;
    public NPCArgumentController argumentController;
    public float interactDistance = 4f;

    private Transform player;

    void Start()
    {
        if (ownerNPC == null)
            ownerNPC = GetComponent<ArgumentNPC>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    void Update()
    {
        if (player == null || ownerNPC == null || argumentController == null)
            return;

        float dist = Vector3.Distance(player.position, transform.position);
        if (dist > interactDistance)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!argumentController.playerInterrupted)
            {
                ownerNPC.SayLine("Wir reden gerade. Misch dich nicht ein.", 2.5f);
                return;
            }

            if (argumentController.rewardGiven)
            {
                ownerNPC.SayLine("Es ist vorbei...", 2f);
                return;
            }

            if (ownerNPC.tellsTruth)
                ownerNPC.SayLine("Ich sage die Wahrheit. Der andere belügt dich.", 3f);
            else
                ownerNPC.SayLine("Er ist der Lügner. Glaub mir lieber.", 3f);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            argumentController.AccuseNPC(ownerNPC);
        }
    }
}