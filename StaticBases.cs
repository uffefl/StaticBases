using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace StaticBases
{


    public class ModuleStaticBase : PartModule
    {

        public override void OnStart(PartModule.StartState state)
        {
            part.OnJustAboutToBeDestroyed += JustAbout;
            base.OnStart(state);
        }

        void JustAbout()
        {
            Debug.Log("vessel just about to be destroyed");
            part.vessel.situation = Vessel.Situations.LANDED;
        }

        [KSPField(isPersistant=true, guiName = "Should be static?", guiActive = true)]
        bool isStatic = false;

        [KSPField(guiName = "Vessel Situation", isPersistant = false, guiActive = true)]
        string situation = "";

        [KSPField(guiName = "Landed for", isPersistant = false, guiActive = true)]
        string landedFor = "";

        double notLandedTime = 0;

        void Apply()
        {
            if (part == null || part.vessel == null) return; // editor mode
            situation = part.vessel.situation.ToString();
            var now = Planetarium.GetUniversalTime();
            if (part.vessel.situation != Vessel.Situations.LANDED || part.vessel.srf_velocity.sqrMagnitude>0.01) notLandedTime = now;
            var available = true;
            Events["ToggleStatic"].guiActive = available;
            Events["ToggleStatic"].guiName = isStatic ? "Make Dynamic" : "Make Static";
            landedFor = notLandedTime == 0 ? "always" : (now - notLandedTime).ToString("0.00 s");
            var shouldBeKinematic = isStatic;// && part.vessel != FlightGlobals.ActiveVessel;
            if (shouldBeKinematic) part.vessel.situation = Vessel.Situations.LANDED;
            foreach (var p in part.vessel.parts)
            {
                var rb = p.Rigidbody;
                if (rb == null) continue;
                if (rb.isKinematic != shouldBeKinematic)
                {
                    Debug.Log("Making " + p.vessel.vesselName + ":" + p.name + " " + (shouldBeKinematic ? "static" : "dynamic") + " now situation " + p.vessel.situation);
                    rb.isKinematic = shouldBeKinematic;
                }
            }
        }

        void FixedUpdate()
        {
            Apply();
        }

        [KSPEvent(guiName = "Make Static", guiActive = false)]
        //[KSPAction("Make Static")]
        public void MakeStatic()
        {
            isStatic = true;
            Apply();
        }

        [KSPEvent(guiName = "Make Dynamic", guiActive = false)]
        //[KSPAction("Make Dynamic")]
        public void MakeDynamic()
        {
            isStatic = false;
            Apply();
        }

        [KSPEvent(guiName = "Toggle Static", guiActive = false)]
        public void ToggleStatic()
        {
            isStatic = !isStatic;
            Apply();
        }

    }
}
