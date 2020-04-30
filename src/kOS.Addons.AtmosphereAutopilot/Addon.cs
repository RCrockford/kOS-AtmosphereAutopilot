using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Safe.Utilities;
using kOS.Safe.Exceptions;
using kOS.Suffixed;
using UnityEngine;
using AtmosphereAutopilot;
using System;

namespace kOS.AddOns.AtmosphereAutopilotAddon
{
    [kOSAddon("AA")]
    [KOSNomenclature("AtmosphereAutopilotAddon")]
    public class Addon : Suffixed.Addon
    {
        private AtmosphereAutopilot.AtmosphereAutopilot aa;

        private TopModuleManager masterAP = null;
        private StandardFlyByWire fbwAP = null;
        private kOSDirector dcAP = null;
        private CruiseController ccAP = null;
        private ProgradeThrustController speedAP = null;

        private float SpeedSetPoint = -1f;
        private float HeadingSetPoint = -1f;
        private float AltitudeSetPoint = -1f;
        private float VertSpeedSetPoint = float.NaN;
        private Vector3 DirectionSetPoint = Vector3.zero;
        private bool PseudoFLC = true;

        public Addon(SharedObjects shared) : base(shared)
        {
            AddSuffix(new string[] { "FBW", "FLYBYWIRE" }, new SetSuffix<BooleanValue>(() => fbwAP != null, SetFBWActive));
            AddSuffix("DIRECTOR", new SetSuffix<BooleanValue>(() => dcAP != null, SetDirectorActive));
            AddSuffix("CRUISE", new SetSuffix<BooleanValue>(() => ccAP != null, SetCruiseActive));
            AddSuffix("SPEEDCONTROL", new SetSuffix<BooleanValue>(() => speedAP != null && speedAP.spd_control_enabled, SetThrustActive));
            AddSuffix("PSEUDOFLC", new SetSuffix<BooleanValue>(() => PseudoFLC, SetPseudoFLC));
            AddSuffix("SPEED", new SetSuffix<ScalarValue>(() => SpeedSetPoint, SetSpeedSetPoint));
            AddSuffix("HEADING", new SetSuffix<ScalarValue>(() => HeadingSetPoint, SetHeadingSetPoint));
            AddSuffix("ALTITUDE", new SetSuffix<ScalarValue>(() => AltitudeSetPoint, SetAltitudeSetPoint));
            AddSuffix("VERTSPEED", new SetSuffix<ScalarValue>(() => VertSpeedSetPoint, SetVertSpeedSetPoint));
            AddSuffix("DIRECTION", new SetSuffix<Vector>(() => new Vector(DirectionSetPoint), SetDirectionSetPoint));

            aa = AtmosphereAutopilot.AtmosphereAutopilot.Instance;
        }

        public override BooleanValue Available()
        {
            return AtmosphereAutopilot.AtmosphereAutopilot.Instance != null;
        }

        private void GetMasterAP()
        {
            var apModules = aa.getVesselModules(shared.Vessel);

            if (apModules.ContainsKey(typeof(TopModuleManager)))
                masterAP = apModules[typeof(TopModuleManager)] as TopModuleManager;
            else
                throw new KOSException("Unable to get master autopilot from AtmosphereAutopilot.");
        }
        private void ResetSpeedControl()
        {
            if (speedAP != null && speedAP.spd_control_enabled)
            {
                speedAP.setpoint = new SpeedSetpoint(SpeedType.MetersPerSecond, SpeedSetPoint, shared.Vessel);
            }
        }

        private void SetFBWActive(BooleanValue value)
        {
            if (value)
            {
                GetMasterAP();
                fbwAP = masterAP.activateAutopilot(typeof(StandardFlyByWire)) as StandardFlyByWire;
                dcAP = null;
                ccAP = null;
                ResetSpeedControl();
            }
            else
            {
                if (fbwAP != null)
                    masterAP.Active = false;
                fbwAP = null;
            }
        }
        private void SetDirectorActive(BooleanValue value)
        {
            if (value)
            {
                GetMasterAP();
                fbwAP = null;
                dcAP = masterAP.activateAutopilot(typeof(kOSDirector)) as kOSDirector;
                ccAP = null;

                if (DirectionSetPoint.sqrMagnitude < 0.9)
                    DirectionSetPoint = shared.Vessel.ReferenceTransform.forward;

                dcAP.target_direction = DirectionSetPoint;
                ResetSpeedControl();
            }
            else
            {
                if (dcAP != null)
                    masterAP.Active = false;
                dcAP = null;
            }
        }
        private void SetCruiseActive(BooleanValue value)
        {
            if (value)
            {
                if (ccAP == null)
                {
                    GetMasterAP();
                    fbwAP = null;
                    dcAP = null;
                    ccAP = masterAP.activateAutopilot(typeof(CruiseController)) as CruiseController;
                    CruiseController.use_keys = false;

                    if (HeadingSetPoint >= 0f)
                    {
                        ccAP.current_mode = CruiseController.CruiseMode.CourseHold;
                        ccAP.desired_course.Value = HeadingSetPoint;
                    }

                    if (AltitudeSetPoint >= 0f)
                    {
                        ccAP.height_mode = CruiseController.HeightMode.Altitude;
                        ccAP.desired_altitude.Value = AltitudeSetPoint;
                    }
                    else
                    {
                        ccAP.height_mode = CruiseController.HeightMode.VerticalSpeed;
                        if (Single.IsNaN(VertSpeedSetPoint))
                            ccAP.desired_vertspeed.Value = 0f;
                        else
                            ccAP.desired_vertspeed.Value = VertSpeedSetPoint;
                    }
                    ccAP.vertical_control = true;
                    ccAP.pseudo_flc = PseudoFLC;
                    ResetSpeedControl();
                }
            }
            else
            {
                if (ccAP != null)
                    masterAP.Active = false;
                ccAP = null;
            }
        }
        private void SetThrustActive(BooleanValue value)
        {
            if (speedAP == null)
            {
                speedAP = aa.getVesselModules(shared.Vessel)[typeof(ProgradeThrustController)] as ProgradeThrustController;
            }
            speedAP.spd_control_enabled = value;
            if (speedAP.spd_control_enabled)
            {
                if (SpeedSetPoint >= 0f)
                    speedAP.setpoint = new SpeedSetpoint(SpeedType.MetersPerSecond, SpeedSetPoint, shared.Vessel);
                else
                    SpeedSetPoint = speedAP.setpoint.mps();
            }
        }
        private void SetPseudoFLC(BooleanValue value)
        {
            PseudoFLC = value;
            if (ccAP != null)
            {
                ccAP.pseudo_flc = PseudoFLC;
            }
        }
        private void SetSpeedSetPoint(ScalarValue value)
        {
            SpeedSetPoint = value;
            if (speedAP != null)
            {
                speedAP.setpoint = new SpeedSetpoint(SpeedType.MetersPerSecond, Mathf.Max(SpeedSetPoint, 0f), shared.Vessel);
            }
        }
        private void SetHeadingSetPoint(ScalarValue value)
        {
            HeadingSetPoint = value;
            if (ccAP != null)
            {
                if (HeadingSetPoint >= 0f)
                {
                    ccAP.current_mode = CruiseController.CruiseMode.CourseHold;
                    ccAP.desired_course.Value = HeadingSetPoint;
                }
                else
                {
                    ccAP.current_mode = CruiseController.CruiseMode.LevelFlight;
                    ccAP.circle_axis = Vector3d.Cross(shared.Vessel.srf_velocity, shared.Vessel.GetWorldPos3D() - shared.Vessel.mainBody.position).normalized;
                }
            }
        }
        private void SetAltitudeSetPoint(ScalarValue value)
        {
            AltitudeSetPoint = value;
            if (ccAP != null)
            {
                if (AltitudeSetPoint >= 0f)
                {
                    ccAP.height_mode = CruiseController.HeightMode.Altitude;
                    ccAP.desired_altitude.Value = AltitudeSetPoint;
                }
                else
                {
                    ccAP.height_mode = CruiseController.HeightMode.VerticalSpeed;
                    ccAP.desired_vertspeed.Value = 0f;
                }
            }
        }
        private void SetVertSpeedSetPoint(ScalarValue value)
        {
            VertSpeedSetPoint = value;
            if (ccAP != null)
            {
                ccAP.height_mode = CruiseController.HeightMode.VerticalSpeed;
                ccAP.desired_vertspeed.Value = VertSpeedSetPoint;
            }
        }
        private void SetDirectionSetPoint(Vector value)
        {
            DirectionSetPoint = value.ToVector3().normalized;
            if (dcAP != null)
            {
                dcAP.target_direction = DirectionSetPoint;
            }
        }
    }
}
