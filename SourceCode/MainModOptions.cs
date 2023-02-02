using System.Collections.Generic;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace InfiniteSpears
{
    public class MainModOptions : OptionInterface
    {
        public static MainModOptions instance = new();

        //
        // options
        //

        public static Configurable<int> maxSpearCountSlider = instance.config.Bind("maxSpearCountSlider", defaultValue: 1, new ConfigurableInfo("For values X > 1, the player can simply carry X spears on the back.", new ConfigAcceptableRange<int>(1, 7), "", "Number of BackSpears (1)"));

        //
        // parameters
        //

        private static readonly float fontHeight = 20f;
        private static readonly float spacing = 20f;

        //
        // variables
        //

        private static Vector2 marginX = new();
        private static Vector2 pos = new();
        private static readonly List<float> boxEndPositions = new();
        private static readonly List<OpLabel> textLabels = new();

        private readonly List<Configurable<int>> sliderConfigurables = new();
        private readonly List<string> sliderMainTextLabels = new();
        private readonly List<OpLabel> sliderTextLabelsLeft = new();
        private readonly List<OpLabel> sliderTextLabelsRight = new();

        //
        // main
        //

        public MainModOptions()
        {
            // ambiguity error // why? TODO
            // OnConfigChanged += MainModOptions_OnConfigChanged;
        }

        //
        // public
        //

        public override void Initialize()
        {
            base.Initialize();
            Tabs = new OpTab[1];
            Tabs[0] = new OpTab(this, "Options");
            InitializeMarginAndPos();

            //
            // Title
            //
            AddNewLine();
            AddTextLabel("Infinite Spears Mod", bigText: true);
            DrawTextLabels(ref Tabs[0]);

            //
            // Subtitle
            //
            AddNewLine(0.5f);
            AddTextLabel("Version " + MainMod.version, FLabelAlignment.Left);
            AddTextLabel("by SchuhBaum", FLabelAlignment.Right);
            DrawTextLabels(ref Tabs[0]);

            AddNewLine();

            //
            // Description
            //
            AddBox();
            AddNewLine(1.5f); // add some space for word wrapping and new lines
            AddTextLabel("Description:\n\nYou can either\na) carry one backspear, and spawn and despawn spears using it\nOR\nb) carry multiple backspears which behave normally.", FLabelAlignment.Left);

            DrawTextLabels(ref Tabs[0]);
            AddNewLine(1.5f);
            DrawBox(ref Tabs[0]);

            AddNewLine();

            //
            // content
            //

            AddBox();
            AddSlider(maxSpearCountSlider, (string)maxSpearCountSlider.info.Tags[0], "1 (infinite)", "7");
            DrawSliders(ref Tabs[0]);
            DrawBox(ref Tabs[0]);
        }

        public void MainModOptions_OnConfigChanged()
        {
            Debug.Log("InfiniteSpears: maxSpearCount " + MainMod.Option_MaxSpearCount);
        }

        //
        // private
        //

        private static void InitializeMarginAndPos()
        {
            marginX = new Vector2(50f, 550f);
            pos = new Vector2(50f, 600f);
        }

        private static void AddNewLine(float spacingModifier = 1f)
        {
            pos.x = marginX.x; // left margin
            pos.y -= spacingModifier * spacing;
        }

        private static void AddBox()
        {
            marginX += new Vector2(spacing, -spacing);
            boxEndPositions.Add(pos.y);
            AddNewLine();
        }

        private static void DrawBox(ref OpTab tab)
        {
            marginX += new Vector2(-spacing, spacing);
            AddNewLine();

            float boxWidth = marginX.y - marginX.x;
            int lastIndex = boxEndPositions.Count - 1;
            tab.AddItems(new OpRect(pos, new Vector2(boxWidth, boxEndPositions[lastIndex] - pos.y)));
            boxEndPositions.RemoveAt(lastIndex);
        }

        private void AddSlider(Configurable<int> configurable, string text, string sliderTextLeft = "", string sliderTextRight = "")
        {
            sliderConfigurables.Add(configurable);
            sliderMainTextLabels.Add(text);
            sliderTextLabelsLeft.Add(new OpLabel(new Vector2(), new Vector2(), sliderTextLeft, alignment: FLabelAlignment.Right)); // set pos and size when drawing
            sliderTextLabelsRight.Add(new OpLabel(new Vector2(), new Vector2(), sliderTextRight, alignment: FLabelAlignment.Left));
        }

        private void DrawSliders(ref OpTab tab)
        {
            if (sliderConfigurables.Count != sliderMainTextLabels.Count) return;
            if (sliderConfigurables.Count != sliderTextLabelsLeft.Count) return;
            if (sliderConfigurables.Count != sliderTextLabelsRight.Count) return;

            float width = marginX.y - marginX.x;
            float sliderCenter = marginX.x + 0.5f * width;
            float sliderLabelSizeX = 0.2f * width;
            float sliderSizeX = width - 2f * sliderLabelSizeX - spacing;

            for (int sliderIndex = 0; sliderIndex < sliderConfigurables.Count; ++sliderIndex)
            {
                AddNewLine(2f);

                OpLabel opLabel = sliderTextLabelsLeft[sliderIndex];
                opLabel.pos = new Vector2(marginX.x, pos.y + 5f);
                opLabel.size = new Vector2(sliderLabelSizeX, fontHeight);
                tab.AddItems(opLabel);

                Configurable<int> configurable = sliderConfigurables[sliderIndex];
                OpSlider slider = new(configurable, new Vector2(sliderCenter - 0.5f * sliderSizeX, pos.y), (int)sliderSizeX)
                {
                    size = new Vector2(sliderSizeX, fontHeight),
                    description = configurable.info?.description ?? ""
                };
                tab.AddItems(slider);

                opLabel = sliderTextLabelsRight[sliderIndex];
                opLabel.pos = new Vector2(sliderCenter + 0.5f * sliderSizeX + 0.5f * spacing, pos.y + 5f);
                opLabel.size = new Vector2(sliderLabelSizeX, fontHeight);
                tab.AddItems(opLabel);

                AddTextLabel(sliderMainTextLabels[sliderIndex]);
                DrawTextLabels(ref tab);

                if (sliderIndex < sliderConfigurables.Count - 1)
                {
                    AddNewLine();
                }
            }

            sliderConfigurables.Clear();
            sliderMainTextLabels.Clear();
            sliderTextLabelsLeft.Clear();
            sliderTextLabelsRight.Clear();
        }

        private static void AddTextLabel(string text, FLabelAlignment alignment = FLabelAlignment.Center, bool bigText = false)
        {
            float textHeight = (bigText ? 2f : 1f) * fontHeight;
            if (textLabels.Count == 0)
            {
                pos.y -= textHeight;
            }

            OpLabel textLabel = new(new Vector2(), new Vector2(20f, textHeight), text, alignment, bigText) // minimal size.x = 20f
            {
                autoWrap = true
            };
            textLabels.Add(textLabel);
        }

        private static void DrawTextLabels(ref OpTab tab)
        {
            if (textLabels.Count == 0)
            {
                return;
            }

            float width = (marginX.y - marginX.x) / textLabels.Count;
            foreach (OpLabel textLabel in textLabels)
            {
                textLabel.pos = pos;
                textLabel.size += new Vector2(width - 20f, 0.0f);
                tab.AddItems(textLabel);
                pos.x += width;
            }

            pos.x = marginX.x;
            textLabels.Clear();
        }
    }
}