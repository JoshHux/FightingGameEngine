using UnityEngine;
using Spax;

namespace FightingGameEngine
{
    public abstract class RendererBehavior : SpaxBehavior
    {

        protected override void OnStart()
        {
            SpaxManager[] managers = Object.FindObjectsOfType<SpaxManager>();
            if (managers.Length > 0)
            {
                SpaxManager manager = managers[0];
                if (manager != null)
                {
                    manager.PreRender += (() => PreRenderUpdate());
                    manager.RenderUpdate += (() => RenderUpdate());
                    //Debug.Log("starting: " + gameObject.name);
                }
            }
        }
        //remove hooks in delegate
        void OnDestroy()
        {
            var manager = Spax.SpaxManager.Instance;
            if (manager != null)
            {
                manager.PreRender -= (() => PreRenderUpdate());
                manager.RenderUpdate -= (() => RenderUpdate());

            }

        }

        protected abstract void PreRenderUpdate();
        protected abstract void RenderUpdate();
        protected abstract void PostRenderUpdate();
    }
}