using UnityEngine;
using Spax;
namespace FightingGameEngine.Gameplay
{
    public abstract class GameplayBehavior : SpaxBehavior
    {

        protected override void OnStart()
        {
            SpaxManager[] managers = Object.FindObjectsOfType<SpaxManager>();
            if (managers.Length > 0)
            {
                SpaxManager manager = managers[0];
                if (manager != null)
                {
                    manager.InputUpdate += (() => InputUpdate());
                    manager.StateUpdate += (() => StateUpdate());
                    manager.StateCleanUpdate += (() => StateCleanUpdate());
                    manager.PreUpdate += (() => PreUpdate());
                    manager.SpaxUpdate += (() => SpaxUpdate());
                    manager.PostPhysUpdate += (() => PostPhysUpdate());
                    manager.HitQueryUpdate += (() => HitboxQueryUpdate());
                    manager.HurtQueryUpdate += (() => HurtboxQueryUpdate());
                    manager.PostUpdate += (() => PostUpdate());
                    // Debug.Log("adding to the thing");
                }
            }
        }

        //remove hooks in delegate
        void OnDestroy()
        {
            var manager = Spax.SpaxManager.Instance;
            if (manager != null)
            {
                manager.InputUpdate -= (() => InputUpdate());
                manager.StateUpdate -= (() => StateUpdate());
                manager.StateCleanUpdate -= (() => StateCleanUpdate());
                manager.PreUpdate -= (() => PreUpdate());
                manager.SpaxUpdate -= (() => SpaxUpdate());
                manager.PostPhysUpdate -= (() => PostPhysUpdate());
                manager.HitQueryUpdate -= (() => HitboxQueryUpdate());
                manager.HurtQueryUpdate -= (() => HurtboxQueryUpdate());
                manager.PostUpdate -= (() => PostUpdate());

            }

        }


        protected abstract void InputUpdate();
        //update state related things, like transitions and frames
        protected abstract void StateUpdate();
        //get it? It's a pun!...
        //this naming is gonna bite me later, I just know it...
        protected abstract void StateCleanUpdate();
        protected abstract void PreUpdate();
        protected abstract void SpaxUpdate();
        protected abstract void PostPhysUpdate();
        protected abstract void HitboxQueryUpdate();
        protected abstract void HurtboxQueryUpdate();
        protected abstract void PostUpdate();

    }
}