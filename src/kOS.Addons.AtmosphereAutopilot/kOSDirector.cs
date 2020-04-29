using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtmosphereAutopilot
{
    public sealed class kOSDirector : StateController
    {
        internal kOSDirector(Vessel v)
            : base(v, "kOS Director", 88437289)
        { }

        DirectorController dir_c;
        ProgradeThrustController thrust_c;

        public override void InitializeDependencies(Dictionary<Type, AutopilotModule> modules)
        {
            //imodel = modules[typeof(FlightModel)] as FlightModel;
            dir_c = modules[typeof(DirectorController)] as DirectorController;
            thrust_c = modules[typeof(ProgradeThrustController)] as ProgradeThrustController;
        }

        protected override void OnActivate()
        {
            dir_c.Activate();
            thrust_c.Activate();
            MessageManager.post_status_message("kOS Director enabled");
            target_direction = Vector3.zero;
        }

        protected override void OnDeactivate()
        {
            dir_c.Deactivate();
            thrust_c.Deactivate();
            MessageManager.post_status_message("kOS Director disabled");
        }

        public override void ApplyControl(FlightCtrlState cntrl)
        {
            if (vessel.LandedOrSplashed() || target_direction.sqrMagnitude < 0.9)
                return;

            //
            // follow programmed direction
            //
            dir_c.ApplyControl(cntrl, target_direction, Vector3d.zero);

            if (thrust_c.spd_control_enabled)
                thrust_c.ApplyControl(cntrl, thrust_c.setpoint.mps());
        }

        public Vector3 target_direction;

        protected override void _drawGUI(int id)
        {
        }
    }
}
