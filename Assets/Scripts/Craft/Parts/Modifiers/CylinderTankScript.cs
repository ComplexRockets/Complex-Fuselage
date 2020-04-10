namespace Assets.Scripts.Craft.Parts.Modifiers {
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Assets.Scripts.Design;
    using ModApi.Common;
    using ModApi.Craft.Parts;
    using ModApi.Craft;
    using ModApi.Design;
    using ModApi.GameLoop.Interfaces;
    using UnityEngine;

    public class CylinderTankScript : PartModifierScript<CylinderTankData> {
        private float cylinderHeight = 1f, fwdBulkheadHeight = 0.5f, aftBulkheadHeight = 0.5f, tankDiameter = 1f, fwdSkirtHeight = 0.25f, aftSkirtHeight = 0.25f;
        private float fwdBulkheadDiameter, aftBulkheadDiameter, oldCh, oldFdh, oldAdh, oldFsh, oldAsh;
        private bool LengthBlocked, FwdDomeOccupied, AftDomeOccupied, FwdSkirtOccupied, AftSkirtOccupied, initializing;
        private GameObject Tank, Bottom, Middle, Top, FwdSkirt, AftSkirt;
        private Vector3 temp;
        protected override void OnInitialized () {
            initializing = true;
            base.OnInitialized ();

            Tank = transform.Find ("Tank").gameObject;
            Bottom = Tank.transform.Find ("Bottom").gameObject;
            Middle = Tank.transform.Find ("Middle").gameObject;
            Top = Tank.transform.Find ("Top").gameObject;
            FwdSkirt = Tank.transform.Find ("FwdSkirt").gameObject;
            AftSkirt = Tank.transform.Find ("AftSkirt").gameObject;

            Data.materialShellListMaking ();
            UpdateTank ();
            initializing = false;
        }

        public void UpdateTank () {
            UpdateAttachPoints ();
            OffsetUpdate (Data.FwdBulkheadHeight / fwdBulkheadHeight, Data.AftBulkheadHeight / aftBulkheadHeight, Data.FwdSkirtHeight / fwdSkirtHeight, Data.AftSkirtHeight / aftSkirtHeight, Data.CylinderHeight / cylinderHeight);
            CylinderHeightUpdated (Data.CylinderHeight / cylinderHeight);
            FwdBulkheadHeightUpdated (Data.FwdBulkheadHeight / fwdBulkheadHeight);
            AftBulkheadHeightUpdated (Data.AftBulkheadHeight / aftBulkheadHeight);
            TankDiameterUpdated (Data.TankDiameter / tankDiameter);
            FwdSkirtHeightUpdated (Data.FwdSkirtHeight / fwdSkirtHeight);
            AftSkirtHeightUpdated (Data.AftSkirtHeight / aftSkirtHeight);
            FuelPercentageUpdated ();
        }

        public void UpdateFuel () {
            if (Game.InDesignerScene) {
                FuelTankScript modifier = base.PartScript.GetModifier<FuelTankScript> ();
                if (modifier != null) {
                    float fuelPercentage = Data.FuelPercentage;
                    modifier.Data.CalculateInitialFuel (Data.Capacity * 1000f, fuelPercentage);
                }
            }
        }

        private void UpdateAttachPoints () {
            Vector3 fwdSkirtYOffset = new Vector3 ();
            Vector3 aftSkirtYOffset = new Vector3 ();
            Vector3 fwdBulkheadYOffset = new Vector3 ();
            Vector3 aftBulkheadYOffset = new Vector3 ();
            Vector3 xOffset = new Vector3 ();

            if (Game.InDesignerScene) {
                if (!LengthBlocked || initializing) {
                    fwdSkirtYOffset.y = Data.CylinderHeight / 2 + Data.FwdSkirtHeight;
                    fwdBulkheadYOffset.y = Data.CylinderHeight / 2;
                    if (fwdBulkheadHeight > 0) fwdBulkheadYOffset.y += Data.FwdBulkheadHeight;

                    aftSkirtYOffset.y = -Data.CylinderHeight / 2 - Data.AftSkirtHeight;
                    aftBulkheadYOffset.y = -Data.CylinderHeight / 2;
                    if (aftBulkheadHeight > 0) aftBulkheadYOffset.y -= Data.AftBulkheadHeight;

                    xOffset.z = Data.TankDiameter / 2f;
                }
                foreach (AttachPoint attachPoint in base.PartScript.Data.AttachPoints) {
                    if (attachPoint.Tag == "STop") {
                        attachPoint.Radius = Data.TankDiameter / 2;
                        attachPoint.AttachPointScript.transform.localPosition = fwdSkirtYOffset;
                        if (attachPoint.PartConnections.Count == 0) FwdSkirtOccupied = false;
                        else FwdSkirtOccupied = true;

                    } else if (attachPoint.Tag == "SBottom") {
                        attachPoint.Radius = Data.TankDiameter / 2;
                        attachPoint.AttachPointScript.transform.localPosition = aftSkirtYOffset;
                        if (attachPoint.PartConnections.Count == 0) AftSkirtOccupied = false;
                        else AftSkirtOccupied = true;

                    } else if (attachPoint.Tag == "ETop") {
                        attachPoint.AttachPointScript.transform.localPosition = fwdBulkheadYOffset;
                        if (attachPoint.PartConnections.Count == 0) FwdDomeOccupied = false;
                        else FwdDomeOccupied = true;

                    } else if (attachPoint.Tag == "EBottom") {
                        attachPoint.AttachPointScript.transform.localPosition = aftBulkheadYOffset;
                        if (attachPoint.PartConnections.Count == 0) AftDomeOccupied = false;
                        else AftDomeOccupied = true;

                    } else if (attachPoint.Tag == "Front") {
                        attachPoint.AttachPointScript.transform.localPosition = xOffset;
                    }
                }
            }
        }

        public void CylinderHeightUpdated (float num) {
            if (!LengthBlocked || initializing) {
                temp = Middle.transform.localScale;
                temp.z = num;
                Middle.transform.localScale = temp;

                temp = Top.transform.localPosition;
                temp.z = num / 2f;
                Top.transform.localPosition = temp;
                FwdSkirt.transform.localPosition = temp;

                temp = Bottom.transform.localPosition;
                temp.z = -num / 2f;
                Bottom.transform.localPosition = temp;
                AftSkirt.transform.localPosition = temp;

                UpdateFuel ();
            }
        }
        public void FwdBulkheadHeightUpdated (float num) {
            if (!LengthBlocked || initializing) {

                temp = Top.transform.localScale;
                temp.z = num;
                Top.transform.localScale = temp;

                FwdBulkheadDiameterUpdate (Data.FwdSkirtHeight / fwdSkirtHeight, num);
                UpdateFuel ();
            }
        }
        public void AftBulkheadHeightUpdated (float num) {
            if (!LengthBlocked || initializing) {

                temp = Bottom.transform.localScale;
                temp.z = num;
                Bottom.transform.localScale = temp;

                AftBulkheadDiameterUpdate (Data.AftSkirtHeight / aftSkirtHeight, num);
                UpdateFuel ();
            }
        }
        public void TankDiameterUpdated (float num) {
            temp = Tank.transform.localScale;
            temp.y = num;
            temp.x = num;
            Tank.transform.localScale = temp;

            UpdateFuel ();
        }
        public void FwdSkirtHeightUpdated (float num) {
            if (!FwdSkirtOccupied || initializing) {
                temp = FwdSkirt.transform.localScale;
                temp.z = num;
                FwdSkirt.transform.localScale = temp;

                FwdBulkheadDiameterUpdate (num, Data.FwdBulkheadHeight / fwdBulkheadHeight);
                UpdateFuel ();
            }
        }
        public void AftSkirtHeightUpdated (float num) {
            if (!AftSkirtOccupied || initializing) {
                temp = AftSkirt.transform.localScale;
                temp.z = num;
                AftSkirt.transform.localScale = temp;

                AftBulkheadDiameterUpdate (num, Data.AftBulkheadHeight / aftBulkheadHeight);
                UpdateFuel ();
            }
        }

        public void AftBulkheadDiameterUpdate (float asHM, float aeHM) {
            if (asHM <= 0.005f) { if (aeHM >= 0f) { aftBulkheadDiameter = 1 / 0.975f; } else { aftBulkheadDiameter = 0.975f; } } else { aftBulkheadDiameter = 1f; }
            temp = Bottom.transform.localScale;
            temp.x = aftBulkheadDiameter;
            temp.y = aftBulkheadDiameter;
            Bottom.transform.localScale = temp;
        }

        public void FuelPercentageUpdated () {
            UpdateFuel ();
        }

        public void FwdBulkheadDiameterUpdate (float fsHM, float feHM) {
            if (fsHM <= 0.005f) { if (feHM >= 0f) { fwdBulkheadDiameter = 1 / 0.975f; } else { fwdBulkheadDiameter = 0.975f; } } else { fwdBulkheadDiameter = 1f; }

            temp = Top.transform.localScale;
            temp.x = fwdBulkheadDiameter;
            temp.y = fwdBulkheadDiameter;
            Top.transform.localScale = temp;
        }

        public void OffsetUpdate (float fdh, float adh, float fsh, float ash, float ch) {
            float offset = 0;
            LengthBlocked = false;

            if ((FwdDomeOccupied || FwdSkirtOccupied) && (AftDomeOccupied || AftSkirtOccupied)) LengthBlocked = true;
            else if (FwdDomeOccupied) offset = -((ch / 2 + fdh) - (oldCh / 2 + oldFdh));
            else if (AftDomeOccupied) offset = (ch / 2 + adh) - (oldCh / 2 + oldAdh);
            else if (FwdSkirtOccupied) offset = -((ch / 2 + fsh) - (oldCh / 2 + oldFsh));
            else if (AftSkirtOccupied) offset = (ch / 2 + ash) - (oldCh / 2 + oldAsh);

            temp = transform.localPosition;
            temp.y += offset;
            transform.localPosition = temp;

            oldFdh = fdh;
            oldAdh = adh;
            oldFsh = fsh;
            oldAsh = ash;
            oldCh = ch;
        }
        public override void OnCraftStructureChanged (ICraftScript craftScript) {
            base.OnCraftStructureChanged (craftScript);
            if (Game.InDesignerScene) {
                Data.OnDesignerCraftStructureChanged ();
            }
        }
        public override void OnConnectedToPart (PartConnectedEventData e) {
            Data.OnConnectedToPart (e);
        }

        public override void OnSymmetry (SymmetryMode mode, IPartScript originalPart, bool created) {
            UpdateTank ();
        }
    }
}