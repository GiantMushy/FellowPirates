using UnityEngine;
using System.Collections;



public class AttackFlowController : MonoBehaviour
{
    public GameObject TimingBarCanvas;
    public GameObject DefendPrefab;

    private TimingBar timingBar;

    private void Awake()
    {
        if (TimingBarCanvas != null)
        {
            timingBar = TimingBarCanvas.GetComponentInChildren<TimingBar>(true);
            TimingBarCanvas.SetActive(false);
        }
        else
        {
            Debug.LogError("AttackFlowController: TimingBarCanvas is NOT assigned in Inspector!");
        }

        if (DefendPrefab != null)
        {
            DefendPrefab.SetActive(false);
        }

    }

    public void StartAttack()
    {
        if (timingBar == null)
        {
            return;
        }

        if (DefendPrefab != null)
            DefendPrefab.SetActive(false);

        TimingBarCanvas.SetActive(true);
        timingBar.StartTiming(this);
    }

    public void StartDefend()
    {
        StartCoroutine(StartDefendDelayed());
    }

    private IEnumerator StartDefendDelayed()
    {
        yield return new WaitForSeconds(2f);

        if (TimingBarCanvas != null)
            TimingBarCanvas.SetActive(false);

        if (DefendPrefab != null)
            DefendPrefab.SetActive(true);
    }
}
