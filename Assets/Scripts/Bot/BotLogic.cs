using UnityEngine;

public class BotLogic : MonoBehaviour
{
    private int BehaviorState;

    void Start()
    {
        
    }


    void Update()
    {
        Behavior();
    }



//////////////////////////////////////////////////////////////////////////////////////
// I  N  I  T  I  A  L  I  Z  E  R  S
//////////////////////////////////////////////////////////////////////////////////////







//////////////////////////////////////////////////////////////////////////////////////
// B  E  H  A  V  I  O  R         S  T  A  T  E  S
//////////////////////////////////////////////////////////////////////////////////////

    public int GetBehavior() { return BehaviorState; }
    public void SetBehavior(int b) { BehaviorState = b; }


    private void Behavior()
    {
        if (BehaviorState == 0)      { BehaveOffensive(); }
        else if (BehaviorState == 1) { BehaveDefensive(); }
        else if (BehaviorState == 2) { BehaveRetreat(); }
    }

    private int UpdateBehavior()
    {
        
        return 0;
    }


    private void BehaveOffensive()
    {
        
    }

    private void BehaveDefensive()
    {
        
    }

    private void BehaveRetreat()
    {
        
    }


}
