using UnityEngine;
using System.Collections;

public class PoisonEffect : MonoBehaviour
{
    private int damagePerTick = 3;
    private float duration = 5f;
    private float tickRate = 1f;

    private float nextTickTime = 0f;
    private float endTime = 0f;
    private Coroutine poisonCoroutine;

    public void ApplyPoison(int damage, float dur, float tick)
    {
        damagePerTick = damage;
        duration = dur;
        tickRate = tick;

        endTime = Time.time + duration;
        nextTickTime = Time.time + tickRate;

        if (poisonCoroutine == null)
            poisonCoroutine = StartCoroutine(PoisonTick());
    }

    private IEnumerator PoisonTick()
    {
        while (Time.time < endTime)
        {
            if (Time.time >= nextTickTime)
            {
                PlayerController player = GetComponent<PlayerController>();
                if (player != null)
                {
                    player.Changehealth(-damagePerTick);
                }
                nextTickTime = Time.time + tickRate;
            }
            yield return null;
        }

        Destroy(this); // Remove component when poison ends
    }
}