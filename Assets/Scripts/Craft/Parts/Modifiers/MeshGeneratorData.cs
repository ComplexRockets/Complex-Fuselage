namespace Assets.Scripts.Craft.Parts.Modifiers {
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    using System;
    using ModApi.Craft.Parts.Attributes;
    using ModApi.Craft.Parts;
    using ModApi.Design.PartProperties;
    using UnityEngine;

    [Serializable]
    [DesignerPartModifier ("MeshGenerator")]
    [PartModifierTypeId ("MoFuselageMod.MeshGenerator")]
    public class MeshGeneratorData : PartModifierData<MeshGeneratorScript> {
        [SerializeField]
        [DesignerPropertySlider (Label = "width", Order = 1, MinValue = 1, MaxValue = 100, Tooltip = "Change the percentage of fuel capacity", NumberOfSteps = 101)]
        public float width = 1f;
        [SerializeField]
        [DesignerPropertySlider (Label = "depth", Order = 2, MinValue = 1, MaxValue = 100, Tooltip = "Change the percentage of fuel capacity", NumberOfSteps = 101)]
        public float depth = 1f;
        [SerializeField]
        [DesignerPropertySlider (Label = "length", Order = 3, MinValue = 1, MaxValue = 100, Tooltip = "Change the percentage of fuel capacity", NumberOfSteps = 101)]
        public float length = 1;
        [SerializeField]
        [DesignerPropertySlider (Label = "curvature", Order = 4, MinValue = -1, MaxValue = 1, Tooltip = "Change the percentage of fuel capacity", NumberOfSteps = 101)]
        public float curvature = 0.5f;
        [SerializeField]
        [DesignerPropertySlider (Label = "distribution", Order = 5, MinValue = 0, MaxValue = 100, Tooltip = "Change the percentage of fuel capacity", NumberOfSteps = 101)]
        public float distribution = 0.5f;
        [SerializeField]
        [DesignerPropertySlider (Label = "twist", Order = 7, MinValue = 1, MaxValue = 100, Tooltip = "Change the percentage of fuel capacity", NumberOfSteps = 101)]
        public float twist = 0;

        protected override void OnDesignerInitialization (IDesignerPartPropertiesModifierInterface d) {
            d.OnPropertyChanged (() => width, (newVal, OldVal) => Script.UpdateMesh ());
            d.OnPropertyChanged (() => depth, (newVal, OldVal) => Script.UpdateMesh ());
            d.OnPropertyChanged (() => length, (newVal, OldVal) => Script.UpdateMesh ());
            d.OnPropertyChanged (() => curvature, (newVal, OldVal) => Script.UpdateMesh ());
            d.OnPropertyChanged (() => distribution, (newVal, OldVal) => Script.UpdateMesh ());
            d.OnPropertyChanged (() => twist, (newVal, OldVal) => Script.UpdateMesh ());
        }
    }
}