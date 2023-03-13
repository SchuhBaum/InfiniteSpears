using System.Collections.Generic;
using Menu.Remix.MixedUI;
using UnityEngine;

using static InfiniteSpears.MainMod;

namespace InfiniteSpears;

public class MainModOptions : OptionInterface
{
    public static MainModOptions instance = new();

    //
    // options
    //

    public static Configurable<int> maxSpearCountSlider = instance.config.Bind("maxSpearCountSlider", defaultValue: 1, new ConfigurableInfo("For values X > 1, the player can simply carry X spears on the back.", new ConfigAcceptableRange<int>(1, 7), "", "Number of BackSpears (1)"));

    public static Configurable<bool> includeYellow = instance.config.Bind("includeYellow", defaultValue: true, new ConfigurableInfo("When disabled, changes will not affect Monk.", null, "", "Monk"));
    public static Configurable<bool> includeWhite = instance.config.Bind("includeWhite", defaultValue: true, new ConfigurableInfo("When disabled, changes will not affect Survivor.", null, "", "Survivor"));
    public static Configurable<bool> includeRed = instance.config.Bind("includeRed", defaultValue: true, new ConfigurableInfo("When disabled, changes will not affect Hunter.", null, "", "Hunter"));

    public static Configurable<bool> includeGourmand = instance.config.Bind("includeGourmand", defaultValue: true, new ConfigurableInfo("When disabled, changes will not affect Gourmand.", null, "", "Gourmand"));
    public static Configurable<bool> includeArtificer = instance.config.Bind("includeArtificer", defaultValue: true, new ConfigurableInfo("When disabled, changes will not affect Artificer.", null, "", "Artificer"));
    public static Configurable<bool> includeRivulet = instance.config.Bind("includeRivulet", defaultValue: true, new ConfigurableInfo("When disabled, changes will not affect Rivulet.", null, "", "Rivulet"));
    public static Configurable<bool> includeSpearmaster = instance.config.Bind("includeSpearmaster", defaultValue: true, new ConfigurableInfo("When disabled, changes will not affect Spearmaster.", null, "", "Spearmaster"));
    public static Configurable<bool> includeSaint = instance.config.Bind("includeSaint", defaultValue: false, new ConfigurableInfo("When disabled, changes will not affect Saint.", null, "", "Saint"));

    public static Configurable<bool> joke_rifle = instance.config.Bind("joke_rifle", defaultValue: false, new ConfigurableInfo("When enabled, you have infinite ammunition for the joke rifle.", null, "", "Joke Rifle"));
    public static Configurable<bool> swallowed_items = instance.config.Bind("swallowed_items", defaultValue: false, new ConfigurableInfo("When enabled, most swallowed item are duplicated when regurgitating unless your hands are full.", null, "", "Swallowed Items"));

    //
    // parameters
    //

    private static readonly float fontHeight = 20f;
    private static readonly float spacing = 20f;

    private readonly int numberOfCheckboxes = 3;
    private readonly float checkBoxSize = 24f;
    private float CheckBoxWithSpacing => checkBoxSize + 0.25f * spacing;

    //
    // variables
    //

    private static Vector2 marginX = new();
    private static Vector2 pos = new();
    private static readonly List<float> boxEndPositions = new();
    private static readonly List<OpLabel> textLabels = new();

    private readonly List<Configurable<bool>> checkBoxConfigurables = new();
    private readonly List<OpLabel> checkBoxesTextLabels = new();

    private readonly List<Configurable<int>> sliderConfigurables = new();
    private readonly List<string> sliderMainTextLabels = new();
    private readonly List<OpLabel> sliderTextLabelsLeft = new();
    private readonly List<OpLabel> sliderTextLabelsRight = new();

    //
    // main
    //

    public MainModOptions() => OnConfigChanged += MainModOptions_OnConfigChanged;

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
        AddTextLabel("Version " + version, FLabelAlignment.Left);
        AddTextLabel("by " + author, FLabelAlignment.Right);
        DrawTextLabels(ref Tabs[0]);

        AddNewLine();

        //
        // Description
        //
        AddBox();
        AddNewLine(1.5f); // add some space for word wrapping and new lines
        AddTextLabel("Description:\n\nYou can either\na) carry one spear on your back, and spawn or despawn spears using it\nOR\nb) carry multiple spears on your back which behave normally.", FLabelAlignment.Left);

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

        AddNewLine(2f);

        AddCheckBox(includeYellow, (string)includeYellow.info.Tags[0]);
        AddCheckBox(includeWhite, (string)includeWhite.info.Tags[0]);
        AddCheckBox(includeRed, (string)includeRed.info.Tags[0]);

        AddCheckBox(includeGourmand, (string)includeGourmand.info.Tags[0]);
        AddCheckBox(includeArtificer, (string)includeArtificer.info.Tags[0]);
        AddCheckBox(includeRivulet, (string)includeRivulet.info.Tags[0]);
        AddCheckBox(includeSpearmaster, (string)includeSpearmaster.info.Tags[0]);
        AddCheckBox(includeSaint, (string)includeSaint.info.Tags[0]);

        DrawCheckBoxes(ref Tabs[0]);

        AddNewLine(1.5f);

        AddCheckBox(joke_rifle, (string)joke_rifle.info.Tags[0]);
        AddCheckBox(swallowed_items, (string)swallowed_items.info.Tags[0]);
        DrawCheckBoxes(ref Tabs[0]);

        DrawBox(ref Tabs[0]);
    }

    public void MainModOptions_OnConfigChanged()
    {
        Debug.Log("InfiniteSpears: Option_MaxSpearCount " + Option_MaxSpearCount);

        Debug.Log("InfiniteSpears: Option_Yellow " + Option_Yellow);
        Debug.Log("InfiniteSpears: Option_White " + Option_White);
        Debug.Log("InfiniteSpears: Option_Red " + Option_Red);

        Debug.Log("InfiniteSpears: Option_Gourmand " + Option_Gourmand);
        Debug.Log("InfiniteSpears: Option_Artificer " + Option_Artificer);
        Debug.Log("InfiniteSpears: Option_Rivulet " + Option_Rivulet);
        Debug.Log("InfiniteSpears: Option_Spearmaster " + Option_Spearmaster);
        Debug.Log("InfiniteSpears: Option_Saint " + Option_Saint);

        Debug.Log("InfiniteSpears: Option_JokeRifle " + Option_JokeRifle);
        Debug.Log("InfiniteSpears: Option_SwallowedItems " + Option_SwallowedItems);
    }

    //
    // private
    //

    private void InitializeMarginAndPos()
    {
        marginX = new Vector2(50f, 550f);
        pos = new Vector2(50f, 600f);
    }

    private void AddNewLine(float spacingModifier = 1f)
    {
        pos.x = marginX.x; // left margin
        pos.y -= spacingModifier * spacing;
    }

    private void AddBox()
    {
        marginX += new Vector2(spacing, -spacing);
        boxEndPositions.Add(pos.y);
        AddNewLine();
    }

    private void DrawBox(ref OpTab tab)
    {
        marginX += new Vector2(-spacing, spacing);
        AddNewLine();

        float boxWidth = marginX.y - marginX.x;
        int lastIndex = boxEndPositions.Count - 1;
        tab.AddItems(new OpRect(pos, new Vector2(boxWidth, boxEndPositions[lastIndex] - pos.y)));
        boxEndPositions.RemoveAt(lastIndex);
    }

    private void AddCheckBox(Configurable<bool> configurable, string text)
    {
        checkBoxConfigurables.Add(configurable);
        checkBoxesTextLabels.Add(new OpLabel(new Vector2(), new Vector2(), text, FLabelAlignment.Left));
    }

    private void DrawCheckBoxes(ref OpTab tab) // changes pos.y but not pos.x
    {
        if (checkBoxConfigurables.Count != checkBoxesTextLabels.Count) return;

        float width = marginX.y - marginX.x;
        float elementWidth = (width - (numberOfCheckboxes - 1) * 0.5f * spacing) / numberOfCheckboxes;
        pos.y -= checkBoxSize;
        float _posX = pos.x;

        for (int checkBoxIndex = 0; checkBoxIndex < checkBoxConfigurables.Count; ++checkBoxIndex)
        {
            Configurable<bool> configurable = checkBoxConfigurables[checkBoxIndex];
            OpCheckBox checkBox = new(configurable, new Vector2(_posX, pos.y))
            {
                description = configurable.info?.description ?? ""
            };
            tab.AddItems(checkBox);
            _posX += CheckBoxWithSpacing;

            OpLabel checkBoxLabel = checkBoxesTextLabels[checkBoxIndex];
            checkBoxLabel.pos = new Vector2(_posX, pos.y + 2f);
            checkBoxLabel.size = new Vector2(elementWidth - CheckBoxWithSpacing, fontHeight);
            tab.AddItems(checkBoxLabel);

            if (checkBoxIndex < checkBoxConfigurables.Count - 1)
            {
                if ((checkBoxIndex + 1) % numberOfCheckboxes == 0)
                {
                    AddNewLine();
                    pos.y -= checkBoxSize;
                    _posX = pos.x;
                }
                else
                {
                    _posX += elementWidth - CheckBoxWithSpacing + 0.5f * spacing;
                }
            }
        }

        checkBoxConfigurables.Clear();
        checkBoxesTextLabels.Clear();
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

    private void AddTextLabel(string text, FLabelAlignment alignment = FLabelAlignment.Center, bool bigText = false)
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

    private void DrawTextLabels(ref OpTab tab)
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