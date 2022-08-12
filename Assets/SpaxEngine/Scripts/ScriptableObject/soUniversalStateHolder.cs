using System.Collections.Generic;
using UnityEngine;
using FightingGameEngine.Enum;
namespace FightingGameEngine.Data
{

    [CreateAssetMenu(fileName = "UniversalStates", menuName = "UniversalStates", order = 1)]
    public class soUniversalStateHolder : ScriptableObject
    {
        [SerializeField] private List<TransitionData> _stateList;

        //check the transition to specific state
        public TransitionData CheckTransition(int targetStateIndex, TransitionFlags curFlags, CancelConditions curCan, ResourceData curResources, InputItem[] playerInputs, int facingDir)
        {
            TransitionData ret = null;
            TransitionData potenTrans = this._stateList[targetStateIndex];
            //Debug.Log("checking movelist");
            /*int i = 0;
            int len = this._stateList.Length;
            while (i < len)
            {
                var hold = this._stateList[i];
                bool check = hold.CheckTransition(curFlags, curCan, curResources, playerInputs);


                //if (i == 1)
                //{
                //Debug.Log(i + " " + checkCancels + " " + checkFlags + " " + checkResources + " " + checkInputs);
                //}

                if (check)
                {
                    ret = hold;
                    break;
                }

                i++;
            }
*/
            bool gotTransition = potenTrans.CheckTransition(curFlags, curCan, curResources, playerInputs, facingDir);

            if (gotTransition)
            {
                ret = potenTrans;
            }

            return ret;
        }

        public soStateData GetStateData(int ind)
        {
            var ret = this._stateList[ind].TargetState;

            return ret;
        }

    }
}