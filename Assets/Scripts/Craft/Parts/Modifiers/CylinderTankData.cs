namespace Assets.Scripts.Craft.Parts.Modifiers {
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    using System;
    using Assets.Scripts.Design;
    using ModApi.Craft.Parts.Attributes;
    using ModApi.Craft.Parts;
    using ModApi.Design.PartProperties;
    using UnityEngine;

    [Serializable]
    [DesignerPartModifier ("Tank Dimensions")]
    [PartModifierTypeId ("MoFuselageMod.CylinderTank")]
    public class CylinderTankData : PartModifierData<CylinderTankScript> {

        public override float Mass => (Volume - Capacity + sVolume) * density * 0.01f; // Shell Volume(m3) * Shell Density(kg/m3) * massScale
        public override int Price => 100 * Mathf.CeilToInt (Mass * pricePerKg); // It only considers Material Cost. It would be good if we can consondier Conversion Cost
        private float density, pricePerKg, shellThickness;

        public float CylinderHeight {
            get {
                if (float.TryParse (cylinderHeight, out float height) && height < 0f) return 0f;
                return height;
            }
        }

        public float TankDiameter {
            get {
                if (float.TryParse (tankDiameter, out float diameter) && diameter < 2f * shellThickness) return 2f * shellThickness;
                return diameter;
            }
        }

        public float FwdBulkheadHeight {
            get {
                float.TryParse (fwdBulkheadHeight, out float fwdBH);
                float domeThickness = shellThickness / 2f;

                if (fwdBH < domeThickness && fwdBH >= 0f) return domeThickness;
                else if (fwdBH < 0f && fwdBH > -domeThickness) return -domeThickness;
                else if (fwdBH < -domeThickness && fwdBH < AftBulkheadHeight) {
                    if (fwdBH < shellThickness - CylinderHeight - AftBulkheadHeight)
                        return shellThickness - CylinderHeight - AftBulkheadHeight;
                    return fwdBH;
                } else return fwdBH;
            }
        }

        public float AftBulkheadHeight {
            get {
                float.TryParse (aftBulkheadHeight, out float aftBH);
                float domeThickness = shellThickness / 2f;

                if (aftBH < domeThickness && aftBH >= 0f) return domeThickness;
                else if (aftBH < 0f && aftBH > -domeThickness) return -domeThickness;
                else if (aftBH < -domeThickness && aftBH < FwdBulkheadHeight) {
                    if (aftBH < shellThickness - CylinderHeight - FwdBulkheadHeight)
                        return shellThickness - CylinderHeight - FwdBulkheadHeight;
                    return aftBH;
                } else return aftBH;
            }
        }

        public float FwdSkirtHeight {
            get {
                if (float.TryParse (fwdSkirtHeight, out float height) && height < 0) return 0f;
                else if (height > FwdBulkheadHeight) return FwdBulkheadHeight;
                return height;
            }
        }

        public float AftSkirtHeight {
            get {
                if (float.TryParse (aftSkirtHeight, out float height) && height < 0) return 0f;
                else if (height > AftBulkheadHeight) return AftBulkheadHeight;
                return height;
            }
        }

        public float FuelPercentage {
            get {
                return fuelPercentage / 100f;
            }
        }

        private IDesignerPartPropertiesModifierInterface _designerPartProperties;
        private List<string> materialShellList = new List<string> ();

        [SerializeField]
        [DesignerPropertySpinner ("Al 2219", "Al-Li 2195", "Steel", Label = "Material    ", Order = 1, Tooltip = "Change the material of the tank shell")]
        private string materialShell = "Al-Li 2195";

        [SerializeField]
        [DesignerPropertyTextInput (Label = "Cylinder    ", Order = 2, Tooltip = "Change the length of the Tank Cylinder")]
        private string cylinderHeight = "1";

        [SerializeField]
        [DesignerPropertyTextInput (Label = "Diameter    ", Order = 3, Tooltip = "Change the diameter of the Tank")]
        private string tankDiameter = "1";

        [SerializeField]
        [DesignerPropertyTextInput (Label = "Top Dome    ", Order = 4, Tooltip = "Change the length of the Top Dome")]
        private string fwdBulkheadHeight = "0.5";

        [SerializeField]
        [DesignerPropertyTextInput (Label = "Bottom Dome", Order = 5, Tooltip = "Change the length of the Bottom Dome")]
        private string aftBulkheadHeight = "0.5";

        [SerializeField]
        [DesignerPropertyTextInput (Label = "Top Skirt   ", Order = 6, Tooltip = "Change the length of the Top Skirt")]
        private string fwdSkirtHeight = "0.25";

        [SerializeField]
        [DesignerPropertyTextInput (Label = "Bottom Skirt", Order = 7, Tooltip = "Change the length of the Bottom Skirt")]
        private string aftSkirtHeight = "0.25";

        [SerializeField]
        [DesignerPropertyToggleButton (Label = "Auto Resize", Order = 8, Tooltip = "Indicates if the tank will adapt to other parts size")]
        private bool autoResize = true;

        [SerializeField]
        [DesignerPropertySlider (Label = "Fuel Percentage", Order = 9, MinValue = 0, MaxValue = 100, Tooltip = "Change the percentage of fuel capacity", NumberOfSteps = 101)]
        private float fuelPercentage = 100f;

        protected override void OnDesignerInitialization (IDesignerPartPropertiesModifierInterface d) {
            d.OnAnyPropertyChanged (() => Updates ());
            d.OnValueLabelRequested (() => fuelPercentage, (float x) => GetDesignerFuelLabel ());
        }

        public float Volume => CalculateVolume (0f); // in m^3
        public float Capacity => CalculateVolume (shellThickness); // in m^3, so you have to convert it to L
        public float sVolume => SkirtVolume (shellThickness / 2f, FwdSkirtHeight, AftSkirtHeight, TankDiameter);

        private float CalculateVolume (float shellThickness) {
            float domeThickness = shellThickness / 2f;
            return CylinderTankVolume (FwdBulkheadHeight - domeThickness, CylinderHeight, AftBulkheadHeight - domeThickness, TankDiameter - shellThickness);
        }

        private float SkirtVolume (float shellThickness, float fsH, float asH, float tD) {
            float tRsOut = tD * tD / 4f;
            tD -= shellThickness;
            float tRsIn = tD * tD / 4f;
            float sH = fsH + asH;
            return (tRsOut - tRsIn) * Mathf.PI * sH;
        }

        private static float CylinderTankVolume (float feH, float cH, float aeH, float tD) {
            float tRs = tD * tD / 4f;
            float FwdDome = 2 / 3f * tRs * feH * Mathf.PI;
            float Cylinder = tRs * Mathf.PI * cH;
            float AftDome = 2 / 3f * tRs * aeH * Mathf.PI;
            return FwdDome + Cylinder + AftDome;
        }

        public void OnDesignerCraftStructureChanged () {
            _designerPartProperties?.GetProperty (() => fuelPercentage)?.RefreshUI ();
        }

        private void Updates () {
            Symmetry.SynchronizePartModifiers (Script.PartScript);
            materialShellListMaking ();
            UpdateValue ();
            Script.UpdateTank ();
            Script.PartScript.CraftScript.RaiseDesignerCraftStructureChangedEvent ();
        }

        public void materialShellListMaking () {
            materialShellList.Clear ();

            for (int i = 0; i < Material.shellMaterial.GetLength (0); i++) {
                string shellMaterial = Material.shellMaterial[i, 0].ToString ();
                materialShellList.Add (shellMaterial);
            }

            density = Convert.ToSingle (Material.shellMaterial[materialShellList.IndexOf (materialShell), 1]);
            pricePerKg = Convert.ToSingle (Material.shellMaterial[materialShellList.IndexOf (materialShell), 2]);
            shellThickness = Convert.ToSingle (Material.shellMaterial[materialShellList.IndexOf (materialShell), 3]);
        }

        private void UpdateValue () {
            cylinderHeight = CylinderHeight.ToString ();
            tankDiameter = TankDiameter.ToString ();
            fwdBulkheadHeight = FwdBulkheadHeight.ToString ();
            aftBulkheadHeight = AftBulkheadHeight.ToString ();
            fwdSkirtHeight = FwdSkirtHeight.ToString ();
            aftSkirtHeight = AftSkirtHeight.ToString ();
        }
        private string GetDesignerFuelLabel () {
            FuelTankData modifier = base.Part.GetModifier<FuelTankData> ();
            if (modifier != null) {
                return FuelTankScript.GetAmountLabel (modifier.Script);
            }
            return string.Empty;
        }

        private void OnFuelPercentageChanged () {
            Symmetry.SynchronizePartModifiers (base.Script.PartScript);
            Script.UpdateFuel ();
            Script.PartScript.CraftScript.RaiseDesignerCraftStructureChangedEvent ();
        }

        private void OnFuelTypeChanged (FuelTankData fuelTank) {
            Symmetry.SynchronizePartModifiers (base.Script.PartScript);
            Script.UpdateFuel ();
            Script.PartScript.CraftScript.RaiseDesignerCraftStructureChangedEvent ();
        }
        public void OnConnectedToPart (PartConnectedEventData e) {
            AttachPoint attachPoint = e.ThisAttachPoint;
            AttachPoint TargetAttachPoint = e.TargetAttachPoint;
            
            if (autoResize) {
                if (TargetAttachPoint.Radius > 0 && (attachPoint.Tag == "STop" || attachPoint.Tag == "SBottom")) {
                    tankDiameter = (TargetAttachPoint.Radius * 2).ToString ();
                }
            }
            Updates ();
        }
    }
}