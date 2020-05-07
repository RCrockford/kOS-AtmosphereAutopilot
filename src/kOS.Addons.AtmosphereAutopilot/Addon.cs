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
        #region AAData
        private AtmosphereAutopilot.AtmosphereAutopilot aa;

        private TopModuleManager masterAP = null;
        private StandardFlyByWire fbwAP = null;
        private kOSDirector dcAP = null;
        private CruiseController ccAP = null;
        private ProgradeThrustController speedAP = null;

        private bool CoordTurn = false;
        private bool RocketMode = false;

        private bool PseudoFLC = true;
        private double FLCMargin = double.NaN;
        private double MaxClimbAngle = double.NaN;
        private float HeadingSetPoint = -1f;
        private float AltitudeSetPoint = -1f;
        private float VertSpeedSetPoint = float.NaN;
        private GeoCoordinates WaypointSetPoint = null;

        private Vector3 DirectionSetPoint = Vector3.zero;

        private float SpeedSetPoint = -1f;
        #endregion

        public Addon(SharedObjects shared) : base(shared)
        {
            // Craft settings
            AddSuffix("MODERATEAOA", new SetSuffix<BooleanValue>(GetModerateAoA, SetModerateAoA));
            AddSuffix("MAXAOA", new SetSuffix<ScalarValue>(GetMaxAoA, SetMaxAoA));
            AddSuffix("MODERATEG", new SetSuffix<BooleanValue>(GetModerateG, SetModerateG));
            AddSuffix("MAXG", new SetSuffix<ScalarValue>(GetMaxG, SetMaxG));
            AddSuffix("MODERATESIDESLIP", new SetSuffix<BooleanValue>(GetModerateSideSlip, SetModerateSideSlip));
            AddSuffix("MAXSIDESLIP", new SetSuffix<ScalarValue>(GetMaxSideSlip, SetMaxSideSlip));
            AddSuffix("MODERATESIDEG", new SetSuffix<BooleanValue>(GetModerateSideG, SetModerateSideG));
            AddSuffix("MAXSIDEG", new SetSuffix<ScalarValue>(GetMaxSideG, SetMaxSideG));

            AddSuffix("PITCHRATELIMIT", new SetSuffix<ScalarValue>(GetPitchRateLimit, SetPitchRateLimit));
            AddSuffix("YAWRATELIMIT", new SetSuffix<ScalarValue>(GetYawRateLimit, SetYawRateLimit));
            AddSuffix("ROLLRATELIMIT", new SetSuffix<ScalarValue>(GetRollRateLimit, SetRollRateLimit));

            AddSuffix("WINGLEVELER", new SetSuffix<BooleanValue>(GetWingLeveler, SetWingLeveler));

            // FBW
            AddSuffix(new string[] { "FBW", "FLYBYWIRE" }, new SetSuffix<BooleanValue>(() => fbwAP != null, SetFBWActive));
            AddSuffix("COORDTURN", new SetSuffix<BooleanValue>(() => CoordTurn, SetCoordTurn));
            AddSuffix("ROCKETMODE", new SetSuffix<BooleanValue>(() => RocketMode, SetRocketMode));

            // Director
            AddSuffix("DIRECTOR", new SetSuffix<BooleanValue>(() => dcAP != null, SetDirectorActive));
            AddSuffix("DIRECTION", new SetSuffix<Vector>(() => new Vector(DirectionSetPoint), SetDirectionSetPoint));
            AddSuffix("DIRECTORSTRENGTH", new SetSuffix<ScalarValue>(GetDirectorStrength, SetDirectorStrength));

            // Cruise
            AddSuffix("CRUISE", new SetSuffix<BooleanValue>(() => ccAP != null, SetCruiseActive));
            AddSuffix("PSEUDOFLC", new SetSuffix<BooleanValue>(() => PseudoFLC, SetPseudoFLC));
            AddSuffix("HEADING", new SetSuffix<ScalarValue>(() => HeadingSetPoint, SetHeadingSetPoint));
            AddSuffix("ALTITUDE", new SetSuffix<ScalarValue>(() => AltitudeSetPoint, SetAltitudeSetPoint));
            AddSuffix("VERTSPEED", new SetSuffix<ScalarValue>(() => VertSpeedSetPoint, SetVertSpeedSetPoint));
            AddSuffix("FLCMARGIN", new SetSuffix<ScalarValue>(() => FLCMargin, SetFLCMargin));
            AddSuffix("MAXCLIMBANGLE", new SetSuffix<ScalarValue>(() => MaxClimbAngle, SetMaxClimbAngle));
            AddSuffix("WAYPOINT", new SetSuffix<GeoCoordinates>(() => WaypointSetPoint ?? new GeoCoordinates(shared, 0f, 0f), SetWaypointSetPoint));

            // Speed control
            AddSuffix("SPEEDCONTROL", new SetSuffix<BooleanValue>(() => speedAP != null && speedAP.spd_control_enabled, SetThrustActive));
            AddSuffix("SPEED", new SetSuffix<ScalarValue>(() => SpeedSetPoint, SetSpeedSetPoint));

            aa = AtmosphereAutopilot.AtmosphereAutopilot.Instance;
        }

        public override BooleanValue Available()
        {
            return AtmosphereAutopilot.AtmosphereAutopilot.Instance != null;
        }
        private AutopilotModule GetAPModule(Type type)
        {
            var apModules = aa.getVesselModules(shared.Vessel);

            if (apModules.ContainsKey(type))
                return apModules[type];
            else
                throw new KOSException("Unable to get autopilot from AtmosphereAutopilot.");
        }
        private void GetMasterAP()
        {
            masterAP = GetAPModule(typeof(TopModuleManager)) as TopModuleManager;
        }
        #region Craft settings
        private BooleanValue GetModerateAoA()
        {
            PitchAngularVelocityController pvc = GetAPModule(typeof(PitchAngularVelocityController)) as PitchAngularVelocityController;
            return pvc.moderate_aoa;
        }
        private void SetModerateAoA(BooleanValue value)
        {
            PitchAngularVelocityController pvc = GetAPModule(typeof(PitchAngularVelocityController)) as PitchAngularVelocityController;
            pvc.moderate_aoa = value.Value;
        }
        private ScalarValue GetMaxAoA()
        {
            PitchAngularVelocityController pvc = GetAPModule(typeof(PitchAngularVelocityController)) as PitchAngularVelocityController;
            return pvc.max_aoa;
        }
        private void SetMaxAoA(ScalarValue value)
        {
            PitchAngularVelocityController pvc = GetAPModule(typeof(PitchAngularVelocityController)) as PitchAngularVelocityController;
            pvc.max_aoa = value;
        }
        private BooleanValue GetModerateG()
        {
            PitchAngularVelocityController pvc = GetAPModule(typeof(PitchAngularVelocityController)) as PitchAngularVelocityController;
            return pvc.moderate_g;
        }
        private void SetModerateG(BooleanValue value)
        {
            PitchAngularVelocityController pvc = GetAPModule(typeof(PitchAngularVelocityController)) as PitchAngularVelocityController;
            pvc.moderate_g = value.Value;
        }
        private ScalarValue GetMaxG()
        {
            PitchAngularVelocityController pvc = GetAPModule(typeof(PitchAngularVelocityController)) as PitchAngularVelocityController;
            return pvc.max_g_force;
        }
        private void SetMaxG(ScalarValue value)
        {
            PitchAngularVelocityController pvc = GetAPModule(typeof(PitchAngularVelocityController)) as PitchAngularVelocityController;
            pvc.max_g_force = value;
        }
        private BooleanValue GetModerateSideSlip()
        {
            YawAngularVelocityController yvc = GetAPModule(typeof(YawAngularVelocityController)) as YawAngularVelocityController;
            return yvc.moderate_aoa;
        }
        private void SetModerateSideSlip(BooleanValue value)
        {
            YawAngularVelocityController yvc = GetAPModule(typeof(YawAngularVelocityController)) as YawAngularVelocityController;
            yvc.moderate_aoa = value.Value;
        }
        private ScalarValue GetMaxSideSlip()
        {
            YawAngularVelocityController yvc = GetAPModule(typeof(YawAngularVelocityController)) as YawAngularVelocityController;
            return yvc.max_aoa;
        }
        private void SetMaxSideSlip(ScalarValue value)
        {
            YawAngularVelocityController yvc = GetAPModule(typeof(YawAngularVelocityController)) as YawAngularVelocityController;
            yvc.max_aoa = value;
        }
        private BooleanValue GetModerateSideG()
        {
            YawAngularVelocityController yvc = GetAPModule(typeof(YawAngularVelocityController)) as YawAngularVelocityController;
            return yvc.moderate_g;
        }
        private void SetModerateSideG(BooleanValue value)
        {
            YawAngularVelocityController yvc = GetAPModule(typeof(YawAngularVelocityController)) as YawAngularVelocityController;
            yvc.moderate_g = value.Value;
        }
        private ScalarValue GetMaxSideG()
        {
            YawAngularVelocityController yvc = GetAPModule(typeof(YawAngularVelocityController)) as YawAngularVelocityController;
            return yvc.max_g_force;
        }
        private void SetMaxSideG(ScalarValue value)
        {
            YawAngularVelocityController yvc = GetAPModule(typeof(YawAngularVelocityController)) as YawAngularVelocityController;
            yvc.max_g_force = value;
        }
        private ScalarValue GetPitchRateLimit()
        {
            PitchAngularVelocityController pvc = GetAPModule(typeof(PitchAngularVelocityController)) as PitchAngularVelocityController;
            return pvc.max_v_construction;
        }
        private void SetPitchRateLimit(ScalarValue value)
        {
            PitchAngularVelocityController pvc = GetAPModule(typeof(PitchAngularVelocityController)) as PitchAngularVelocityController;
            pvc.max_v_construction = value;
        }
        private ScalarValue GetYawRateLimit()
        {
            YawAngularVelocityController yvc = GetAPModule(typeof(YawAngularVelocityController)) as YawAngularVelocityController;
            return yvc.max_v_construction;
        }
        private void SetYawRateLimit(ScalarValue value)
        {
            YawAngularVelocityController yvc = GetAPModule(typeof(YawAngularVelocityController)) as YawAngularVelocityController;
            yvc.max_v_construction = value;
        }
        private ScalarValue GetRollRateLimit()
        {
            RollAngularVelocityController rvc = GetAPModule(typeof(RollAngularVelocityController)) as RollAngularVelocityController;
            return rvc.max_v_construction;
        }
        private void SetRollRateLimit(ScalarValue value)
        {
            RollAngularVelocityController rvc = GetAPModule(typeof(RollAngularVelocityController)) as RollAngularVelocityController;
            rvc.max_v_construction = value;
        }
        private BooleanValue GetWingLeveler()
        {
            RollAngularVelocityController rvc = GetAPModule(typeof(RollAngularVelocityController)) as RollAngularVelocityController;
            return rvc.wing_leveler;
        }
        private void SetWingLeveler(BooleanValue value)
        {
            RollAngularVelocityController rvc = GetAPModule(typeof(RollAngularVelocityController)) as RollAngularVelocityController;
            rvc.wing_leveler = value.Value;
        }
        #endregion
        #region FBW
        private void SetFBWActive(BooleanValue value)
        {
            if (value)
            {
                GetMasterAP();
                fbwAP = masterAP.activateAutopilot(typeof(StandardFlyByWire)) as StandardFlyByWire;
                dcAP = null;
                ccAP = null;
                ResetSpeedControl();
                fbwAP.coord_turn = CoordTurn;
                fbwAP.rocket_mode = RocketMode;
            }
            else
            {
                if (fbwAP != null)
                    masterAP.Active = false;
                fbwAP = null;
            }
        }
        private void SetCoordTurn(BooleanValue value)
        {
            CoordTurn = value;
            if (fbwAP != null)
            {
                fbwAP.coord_turn = CoordTurn;
            }
        }
        private void SetRocketMode(BooleanValue value)
        {
            RocketMode = value;
            if (fbwAP != null)
            {
                fbwAP.rocket_mode = RocketMode;
            }
        }
        #endregion
        #region Director
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
        private void SetDirectionSetPoint(Vector value)
        {
            DirectionSetPoint = value.ToVector3().normalized;
            if (dcAP != null)
            {
                dcAP.target_direction = DirectionSetPoint;
            }
        }
        private ScalarValue GetDirectorStrength()
        {
            DirectorController dc = GetAPModule(typeof(DirectorController)) as DirectorController;
            return dc.strength;
        }
        private void SetDirectorStrength(ScalarValue value)
        {
            DirectorController dc = GetAPModule(typeof(DirectorController)) as DirectorController;
            dc.strength = value;
        }
        #endregion
        #region Cruise
        private void SetCruiseMode()
        {
            if (WaypointSetPoint != null)
            {
                ccAP.current_mode = CruiseController.CruiseMode.Waypoint;
                ccAP.desired_latitude.Value = (float)WaypointSetPoint.Latitude;
                ccAP.desired_longitude.Value = (float)WaypointSetPoint.Longitude;
            }
            else if (HeadingSetPoint >= 0f)
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

                    if (double.IsNaN(FLCMargin))
                        FLCMargin = ccAP.flc_margin;
                    else
                        ccAP.flc_margin = FLCMargin;
                    if (double.IsNaN(MaxClimbAngle))
                        MaxClimbAngle = ccAP.max_climb_angle;
                    else
                        ccAP.max_climb_angle = MaxClimbAngle;

                    SetCruiseMode();

                    if (AltitudeSetPoint >= 0f)
                    {
                        ccAP.height_mode = CruiseController.HeightMode.Altitude;
                        ccAP.desired_altitude.Value = AltitudeSetPoint;
                    }
                    else
                    {
                        ccAP.height_mode = CruiseController.HeightMode.VerticalSpeed;
                        if (float.IsNaN(VertSpeedSetPoint))
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
        private void SetPseudoFLC(BooleanValue value)
        {
            PseudoFLC = value;
            if (ccAP != null)
            {
                ccAP.pseudo_flc = PseudoFLC;
            }
        }
        private void SetHeadingSetPoint(ScalarValue value)
        {
            HeadingSetPoint = value;
            if (ccAP != null)
            {
                WaypointSetPoint = null;
                SetCruiseMode();
            }
        }
        private void SetWaypointSetPoint(GeoCoordinates value)
        {
            WaypointSetPoint = value;
            if (ccAP != null)
            {
                HeadingSetPoint = -1f;
                SetCruiseMode();
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
        private void SetFLCMargin(ScalarValue value)
        {
            FLCMargin = value;
            if (ccAP != null)
                ccAP.flc_margin = FLCMargin;
        }
        private void SetMaxClimbAngle(ScalarValue value)
        {
            MaxClimbAngle = value;
            if (ccAP != null)
                ccAP.max_climb_angle = MaxClimbAngle;
        }
        #endregion
        #region Speed control
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
        private void ResetSpeedControl()
        {
            if (speedAP != null && speedAP.spd_control_enabled)
            {
                speedAP.setpoint = new SpeedSetpoint(SpeedType.MetersPerSecond, SpeedSetPoint, shared.Vessel);
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
        #endregion
    }
}
