using UnityEngine;

public class AttackFlowController : MonoBehaviour
{
    [Header("Assigned in Inspector")]
    public GameObject TimingBarCanvas;  // drag TimingBarCanvas here
    public GameObject DefendPrefab;     // drag Defend object here

    private TimingBar timingBar;

    private void Awake()
    {
        if (TimingBarCanvas != null)
        {
            // find the TimingBar component inside the canvas (even if the object is inactive)
            timingBar = TimingBarCanvas.GetComponentInChildren<TimingBar>(true);
            TimingBarCanvas.SetActive(false);   // hide at start
        }
        else
        {
            Debug.LogError("AttackFlowController: TimingBarCanvas is NOT assigned in Inspector!");
        }

        if (DefendPrefab != null)
        {
            DefendPrefab.SetActive(false);      // hide at start
        }
        else
        {
            Debug.LogError("AttackFlowController: DefendPrefab is NOT assigned in Inspector!");
        }
    }

    public void StartAttack()
    {
        if (timingBar == null)
        {
            Debug.LogError("AttackFlowController: timingBar is null (TimingBar component not found on TimingBarCanvas).");
            return;
        }

        if (DefendPrefab != null)
            DefendPrefab.SetActive(false);

        TimingBarCanvas.SetActive(true);
        timingBar.StartTiming(this);   // <<< this is what sets 'flow' in TimingBar
    }

    public void StartDefend()
    {
        if (TimingBarCanvas != null)
            TimingBarCanvas.SetActive(false);

        if (DefendPrefab != null)
            DefendPrefab.SetActive(true);
    }
}
