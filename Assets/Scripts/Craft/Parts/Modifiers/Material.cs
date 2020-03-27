namespace Assets.Scripts.Craft.Parts.Modifiers {
    using System.Collections;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    using System;
    using Assets.Scripts.Design;
    using ModApi.Craft.Parts.Attributes;
    using ModApi.Craft.Parts;
    using ModApi.Design.PartProperties;
    using UnityEngine;

    class Material{
        public static object[,] shellMaterial = new object[3,4] {
            {"Al 2219", 2840f, 0.9f, 0.009f}, // price and thickness arbitrary
            {"Al-Li 2195", 2630f, 1.2f, 0.0075f}, // price of normal aluminum from LME 3M Contracts
            {"Steel", 7750f, 0.72f, 0.01f}, // steel from WikiPedia, thickness arbitrary
        };
    }
}