using FightingGameEngine.Gameplay;
namespace FightingGameEngine.Data
{
    [System.Serializable]
    public struct HitboxState
    {
        public HitboxData CurrentData;
        public TimerInfo TimerInfo;
        //player ID of entity we are colliding with
        public Arr16<Arr2<int>> WasColliding;

        public HitboxState(HitboxTrigger box)
        {
            this.CurrentData = box.GetHitboxData();
            this.TimerInfo = new TimerInfo(box.GetTimer());
            this.WasColliding = new Arr16<Arr2<int>>();

            if (box != null)
            {
                var wasList = box.GetWasCol();
                //loop to assign the collision information
                int i = 0;
                int len = wasList.Count;

                while (i < 16)
                {
                    //first item is the ID, second item is the index of the hit or hurtbox
                    Arr2<int> toAdd = new Arr2<int>();

                    //set default values
                    toAdd._0 = -1;
                    toAdd._1 = -1;

                    //if we aren't out of bounds
                    //only changes value of toAdd if we have collision information
                    if (i < len)
                    {
                        var hold = wasList[i];

                        //is the box we're colliding with a hit or hurtbox?
                        //the index of hitboxes will be multiplied by -1 and will thus be negative
                        int hitHurt = (hold is HitboxTrigger) ? -1 : 1;
                        toAdd.SetValue(0, hold.Owner.Status.PlayerID);
                        toAdd.SetValue(1, hold.GetTriggerIndex() * hitHurt);

                        //UnityEngine.Debug.Log(i + " :: " + hold.Owner.Status.PlayerID + " | " + (hold.GetTriggerIndex() * hitHurt) + " ||| " + toAdd._0 + " | " + toAdd._1);
                    }

                    this.WasColliding.SetValue(i, toAdd);
                    if (i == 0 && box.Owner.Status.PlayerID == 0) { UnityEngine.Debug.Log(i + " :: " + this.WasColliding._0._0 + " | " + this.WasColliding._0._1 + " |||| " + toAdd._0 + " | " + toAdd._1); }
                    i++;
                }

                //UnityEngine.Debug.Log(i + " :: " + this.WasColliding._0._0 + " | " + this.WasColliding._0._1); 

            }
            else
            {
                Arr2<int> toAdd = new Arr2<int>();

                //set default values
                toAdd._0 = -1;
                toAdd._1 = -1;

                this.WasColliding.SetValue(0, toAdd);
                this.WasColliding.SetValue(1, toAdd);
                this.WasColliding.SetValue(2, toAdd);
                this.WasColliding.SetValue(3, toAdd);
                this.WasColliding.SetValue(4, toAdd);
                this.WasColliding.SetValue(5, toAdd);
                this.WasColliding.SetValue(6, toAdd);
                this.WasColliding.SetValue(7, toAdd);
                this.WasColliding.SetValue(8, toAdd);
                this.WasColliding.SetValue(9, toAdd);
                this.WasColliding.SetValue(10, toAdd);
                this.WasColliding.SetValue(11, toAdd);
                this.WasColliding.SetValue(12, toAdd);
                this.WasColliding.SetValue(13, toAdd);
                this.WasColliding.SetValue(14, toAdd);
                this.WasColliding.SetValue(15, toAdd);

            }
        }
    }
}