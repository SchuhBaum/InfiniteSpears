using Menu.Remix.MixedUI;
using System.Collections.Generic;
using UnityEngine;
using static InfiniteSpears.MainMod;
using static InfiniteSpears.ProcessManagerMod;

namespace InfiniteSpears;

public class MainModOptions : OptionInterface {
    public static MainModOptions main_mod_options = new();

    //
    // options
    //

    public static Configurable<bool> joke_rifle = main_mod_options.config.Bind("joke_rifle", defaultValue: false, new ConfigurableInfo("When enabled, you have infinite ammunition for the joke rifle.", null, "", "Joke Rifle"));
    public static Configurable<bool> swallowed_items = main_mod_options.config.Bind("swallowed_items", defaultValue: false, new ConfigurableInfo("When enabled, most swallowed item are duplicated when regurgitating unless your hands are full.", null, "", "Swallowed Items"));

    public static Configurable<int> max_spear_count_slider_yellow = main_mod_options.config.Bind("max_spear_count_slider_yellow", defaultValue: -1, new ConfigurableInfo("For values X > 0, the player can simply carry X spears on the back.", new ConfigAcceptableRange<int>(-1, 7), "", "Number of BackSpears for Monk (-1)"));
    public static Configurable<int> max_spear_count_slider_white = main_mod_options.config.Bind("max_spear_count_slider_white", defaultValue: -1, new ConfigurableInfo("For values X > 0, the player can simply carry X spears on the back.", new ConfigAcceptableRange<int>(-1, 7), "", "Number of BackSpears for Survivor (-1)"));
    public static Configurable<int> max_spear_count_slider_red = main_mod_options.config.Bind("max_spear_count_slider_red", defaultValue: -1, new ConfigurableInfo("For values X > 0, the player can simply carry X spears on the back.", new ConfigAcceptableRange<int>(-1, 7), "", "Number of BackSpears for Hunter (-1)"));

    public static Configurable<int> max_spear_count_slider_gourmand = main_mod_options.config.Bind("max_spear_count_slider_gourmand", defaultValue: -1, new ConfigurableInfo("For values X > 0, the player can simply carry X spears on the back.", new ConfigAcceptableRange<int>(-1, 7), "", "Number of BackSpears for Gourmand (-1)"));
    public static Configurable<int> max_spear_count_slider_artificer = main_mod_options.config.Bind("max_spear_count_slider_artificer", defaultValue: -1, new ConfigurableInfo("For values X > 0, the player can simply carry X spears on the back.", new ConfigAcceptableRange<int>(-1, 7), "", "Number of BackSpears for Artificer (-1)"));
    public static Configurable<int> max_spear_count_slider_rivulet = main_mod_options.config.Bind("max_spear_count_slider_rivulet", defaultValue: -1, new ConfigurableInfo("For values X > 0, the player can simply carry X spears on the back.", new ConfigAcceptableRange<int>(-1, 7), "", "Number of BackSpears for Rivulet (-1)"));
    public static Configurable<int> max_spear_count_slider_spearmaster = main_mod_options.config.Bind("max_spear_count_slider_spearmaster", defaultValue: -1, new ConfigurableInfo("For values X > 0, the player can simply carry X spears on the back.", new ConfigAcceptableRange<int>(-1, 7), "", "Number of BackSpears for Spearmaster (-1)"));
    public static Configurable<int> max_spear_count_slider_saint = main_mod_options.config.Bind("max_spear_count_slider_saint", defaultValue: 0, new ConfigurableInfo("For values X > 0, the player can simply carry X spears on the back.", new ConfigAcceptableRange<int>(-1, 7), "", "Number of BackSpears for Saint (0)"));

    public static Configurable<int> max_spear_count_slider_sofanthiel = main_mod_options.config.Bind("max_spear_count_slider_sofanthiel", defaultValue: 0, new ConfigurableInfo("For values X > 0, the player can simply carry X spears on the back.", new ConfigAcceptableRange<int>(-1, 7), "", "Number of BackSpears for Inv (0)"));
    public static Configurable<int> max_spear_count_slider_custom_slugcats = main_mod_options.config.Bind("max_spear_count_slider_custom_slugcats", defaultValue: 0, new ConfigurableInfo("For values X > 0, the player can simply carry X spears on the back.", new ConfigAcceptableRange<int>(-1, 7), "", "Number of BackSpears for Custom Slugcats (0)"));

    //
    // parameters
    //

    private static readonly float _font_height = 20f;
    private static readonly float _spacing = 20f;

    private readonly int _number_of_checkboxes = 3;
    private readonly float _checkbox_size = 24f;
    private float Checkbox_With_Spacing => _checkbox_size + 0.25f * _spacing;

    //
    // variables
    //

    private static Vector2 _margin_x = new();
    private static Vector2 _position = new();
    private static readonly List<float> _box_end_position = new();
    private static readonly List<OpLabel> _text_labels = new();

    private readonly List<Configurable<bool>> _checkbox_configurables = new();
    private readonly List<OpLabel> _checkboxes_text_labels = new();

    private readonly List<Configurable<int>> _slider_configurables = new();
    private readonly List<string> _slider_main_text_labels = new();
    private readonly List<OpLabel> _slider_text_labels_left = new();
    private readonly List<OpLabel> _slider_text_labels_right = new();

    //
    // main
    //

    private MainModOptions() {
        On.OptionInterface._SaveConfigFile -= Save_Config_File;
        On.OptionInterface._SaveConfigFile += Save_Config_File;
    }

    private void Save_Config_File(On.OptionInterface.orig__SaveConfigFile orig, OptionInterface option_interface) {
        // the event OnConfigChange is triggered too often;
        // it is triggered when you click on the mod name in the
        // remix menu;
        // initializing the hooks takes like half a second;
        // I don't want to do that too often;

        orig(option_interface);
        if (option_interface != main_mod_options) return;
        Debug.Log("InfiniteSpears: Save_Config_File.");
        Initialize_Option_Specific_Hooks();
    }

    //
    // public
    //

    public override void Initialize() {
        base.Initialize();
        int number_of_tabs = 3;
        Tabs = new OpTab[number_of_tabs];

        //
        // General A
        //

        int tab_index = Mathf.Min(0, number_of_tabs);
        Tabs[tab_index] = new OpTab(this, "General A");
        InitializeMarginAndPos();

        //
        // Title
        //
        AddNewLine();
        AddTextLabel("Infinite Spears Mod", has_big_text: true);
        DrawTextLabels(ref Tabs[tab_index]);

        //
        // Subtitle
        //
        AddNewLine(0.5f);
        AddTextLabel("Version " + version, FLabelAlignment.Left);
        AddTextLabel("by " + author, FLabelAlignment.Right);
        DrawTextLabels(ref Tabs[tab_index]);

        AddNewLine();

        //
        // Description
        //
        AddBox();
        AddNewLine(1.5f); // add some space for word wrapping and new lines
        AddTextLabel("Description:\n\nYou can either\na) carry one spear on your back, and spawn or despawn spears using it\nOR\nb) carry multiple spears on your back which behave normally.", FLabelAlignment.Left);

        DrawTextLabels(ref Tabs[tab_index]);
        AddNewLine(1.5f);
        DrawBox(ref Tabs[tab_index]);

        AddNewLine();

        //
        // content
        //
        AddBox();
        AddSlider(max_spear_count_slider_yellow, (string)max_spear_count_slider_yellow.info.Tags[0], "-1 (infinite)", "7");
        AddSlider(max_spear_count_slider_white, (string)max_spear_count_slider_white.info.Tags[0], "-1 (infinite)", "7");
        AddSlider(max_spear_count_slider_red, (string)max_spear_count_slider_red.info.Tags[0], "-1 (infinite)", "7");
        DrawSliders(ref Tabs[tab_index]);

        AddNewLine(2f);

        AddCheckBox(joke_rifle, (string)joke_rifle.info.Tags[0]);
        AddCheckBox(swallowed_items, (string)swallowed_items.info.Tags[0]);
        DrawCheckBoxes(ref Tabs[tab_index]);

        DrawBox(ref Tabs[tab_index]);

        //
        // General B
        //
        tab_index = Mathf.Min(1, number_of_tabs);
        Tabs[tab_index] = new OpTab(this, "General B");
        InitializeMarginAndPos();

        //
        // Title
        //
        AddNewLine();
        AddTextLabel("Infinite Spears Mod", has_big_text: true);
        DrawTextLabels(ref Tabs[tab_index]);

        //
        // Subtitle
        //
        AddNewLine(0.5f);
        AddTextLabel("Version " + version, FLabelAlignment.Left);
        AddTextLabel("by " + author, FLabelAlignment.Right);
        DrawTextLabels(ref Tabs[tab_index]);

        AddNewLine();

        //
        // content
        //
        AddBox();

        AddSlider(max_spear_count_slider_gourmand, (string)max_spear_count_slider_gourmand.info.Tags[0], "-1 (infinite)", "7");
        AddSlider(max_spear_count_slider_artificer, (string)max_spear_count_slider_artificer.info.Tags[0], "-1 (infinite)", "7");
        AddSlider(max_spear_count_slider_rivulet, (string)max_spear_count_slider_rivulet.info.Tags[0], "-1 (infinite)", "7");
        AddSlider(max_spear_count_slider_spearmaster, (string)max_spear_count_slider_spearmaster.info.Tags[0], "-1 (infinite)", "7");
        AddSlider(max_spear_count_slider_saint, (string)max_spear_count_slider_saint.info.Tags[0], "-1 (infinite)", "7");
        DrawSliders(ref Tabs[tab_index]);

        DrawBox(ref Tabs[tab_index]);

        //
        // General C
        //
        tab_index = Mathf.Min(2, number_of_tabs);
        Tabs[tab_index] = new OpTab(this, "General C");
        InitializeMarginAndPos();

        //
        // Title
        //
        AddNewLine();
        AddTextLabel("Infinite Spears Mod", has_big_text: true);
        DrawTextLabels(ref Tabs[tab_index]);

        //
        // Subtitle
        //
        AddNewLine(0.5f);
        AddTextLabel("Version " + version, FLabelAlignment.Left);
        AddTextLabel("by " + author, FLabelAlignment.Right);
        DrawTextLabels(ref Tabs[tab_index]);

        AddNewLine();

        //
        // content
        //

        AddBox();

        AddSlider(max_spear_count_slider_sofanthiel, (string)max_spear_count_slider_sofanthiel.info.Tags[0], "-1 (infinite)", "7");
        AddSlider(max_spear_count_slider_custom_slugcats, (string)max_spear_count_slider_custom_slugcats.info.Tags[0], "-1 (infinite)", "7");
        DrawSliders(ref Tabs[tab_index]);

        DrawBox(ref Tabs[tab_index]);
    }

    public void Log_All_Options() {
        Debug.Log("InfiniteSpears: Option_Max_Spear_Count_Yellow " + Option_Max_Spear_Count_Yellow);
        Debug.Log("InfiniteSpears: Option_Max_Spear_Count_White " + Option_Max_Spear_Count_White);
        Debug.Log("InfiniteSpears: Option_Max_Spear_Count_Red " + Option_Max_Spear_Count_Red);

        Debug.Log("InfiniteSpears: Option_JokeRifle " + Option_JokeRifle);
        Debug.Log("InfiniteSpears: Option_SwallowedItems " + Option_SwallowedItems);

        Debug.Log("InfiniteSpears: Option_Max_Spear_Count_Gourmand " + Option_Max_Spear_Count_Gourmand);
        Debug.Log("InfiniteSpears: Option_Max_Spear_Count_Artificer " + Option_Max_Spear_Count_Artificer);
        Debug.Log("InfiniteSpears: Option_Max_Spear_Count_Rivulet " + Option_Max_Spear_Count_Rivulet);
        Debug.Log("InfiniteSpears: Option_Max_Spear_Count_Spearmaster " + Option_Max_Spear_Count_Spearmaster);
        Debug.Log("InfiniteSpears: Option_Max_Spear_Count_Saint " + Option_Max_Spear_Count_Saint);

        Debug.Log("InfiniteSpears: Option_Max_Spear_Count_Sofanthiel " + Option_Max_Spear_Count_Sofanthiel);
        Debug.Log("InfiniteSpears: Option_Max_Spear_Count_Custom_Slugcats " + Option_Max_Spear_Count_Custom_Slugcats);
    }

    //
    // private
    //

    private void InitializeMarginAndPos() {
        _margin_x = new Vector2(50f, 550f);
        _position = new Vector2(50f, 600f);
    }

    private void AddNewLine(float spacing_modifier = 1f) {
        _position.x = _margin_x.x; // left margin
        _position.y -= spacing_modifier * _spacing;
    }

    private void AddBox() {
        _margin_x += new Vector2(_spacing, -_spacing);
        _box_end_position.Add(_position.y);
        AddNewLine();
    }

    private void DrawBox(ref OpTab tab) {
        _margin_x += new Vector2(-_spacing, _spacing);
        AddNewLine();

        float box_width = _margin_x.y - _margin_x.x;
        int last_index = _box_end_position.Count - 1;
        tab.AddItems(new OpRect(_position, new Vector2(box_width, _box_end_position[last_index] - _position.y)));
        _box_end_position.RemoveAt(last_index);
    }

    private void AddCheckBox(Configurable<bool> configurable, string text) {
        _checkbox_configurables.Add(configurable);
        _checkboxes_text_labels.Add(new OpLabel(new Vector2(), new Vector2(), text, FLabelAlignment.Left));
    }

    private void DrawCheckBoxes(ref OpTab tab) // changes pos.y but not pos.x
    {
        if (_checkbox_configurables.Count != _checkboxes_text_labels.Count) return;

        float width = _margin_x.y - _margin_x.x;
        float element_width = (width - (_number_of_checkboxes - 1) * 0.5f * _spacing) / _number_of_checkboxes;
        _position.y -= _checkbox_size;
        float position_x = _position.x;

        for (int checkbox_index = 0; checkbox_index < _checkbox_configurables.Count; ++checkbox_index) {
            Configurable<bool> configurable = _checkbox_configurables[checkbox_index];
            OpCheckBox checkbox = new(configurable, new Vector2(position_x, _position.y)) {
                description = configurable.info?.description ?? ""
            };
            tab.AddItems(checkbox);
            position_x += Checkbox_With_Spacing;

            OpLabel checkbox_label = _checkboxes_text_labels[checkbox_index];
            checkbox_label.pos = new Vector2(position_x, _position.y + 2f);
            checkbox_label.size = new Vector2(element_width - Checkbox_With_Spacing, _font_height);
            tab.AddItems(checkbox_label);

            if (checkbox_index < _checkbox_configurables.Count - 1) {
                if ((checkbox_index + 1) % _number_of_checkboxes == 0) {
                    AddNewLine();
                    _position.y -= _checkbox_size;
                    position_x = _position.x;
                } else {
                    position_x += element_width - Checkbox_With_Spacing + 0.5f * _spacing;
                }
            }
        }

        _checkbox_configurables.Clear();
        _checkboxes_text_labels.Clear();
    }

    private void AddSlider(Configurable<int> configurable, string text, string slider_text_left = "", string slider_text_right = "") {
        _slider_configurables.Add(configurable);
        _slider_main_text_labels.Add(text);
        _slider_text_labels_left.Add(new OpLabel(new Vector2(), new Vector2(), slider_text_left, alignment: FLabelAlignment.Right)); // set pos and size when drawing
        _slider_text_labels_right.Add(new OpLabel(new Vector2(), new Vector2(), slider_text_right, alignment: FLabelAlignment.Left));
    }

    private void DrawSliders(ref OpTab tab) {
        if (_slider_configurables.Count != _slider_main_text_labels.Count) return;
        if (_slider_configurables.Count != _slider_text_labels_left.Count) return;
        if (_slider_configurables.Count != _slider_text_labels_right.Count) return;

        float width = _margin_x.y - _margin_x.x;
        float slider_center = _margin_x.x + 0.5f * width;
        float slider_label_size_x = 0.2f * width;
        float slider_size_x = width - 2f * slider_label_size_x - _spacing;

        for (int slider_index = 0; slider_index < _slider_configurables.Count; ++slider_index) {
            AddNewLine(2f);

            OpLabel op_label = _slider_text_labels_left[slider_index];
            op_label.pos = new Vector2(_margin_x.x, _position.y + 5f);
            op_label.size = new Vector2(slider_label_size_x, _font_height);
            tab.AddItems(op_label);

            Configurable<int> configurable = _slider_configurables[slider_index];
            OpSlider slider = new(configurable, new Vector2(slider_center - 0.5f * slider_size_x, _position.y), (int)slider_size_x) {
                size = new Vector2(slider_size_x, _font_height),
                description = configurable.info?.description ?? ""
            };
            tab.AddItems(slider);

            op_label = _slider_text_labels_right[slider_index];
            op_label.pos = new Vector2(slider_center + 0.5f * slider_size_x + 0.5f * _spacing, _position.y + 5f);
            op_label.size = new Vector2(slider_label_size_x, _font_height);
            tab.AddItems(op_label);

            AddTextLabel(_slider_main_text_labels[slider_index]);
            DrawTextLabels(ref tab);

            if (slider_index < _slider_configurables.Count - 1) {
                AddNewLine();
            }
        }

        _slider_configurables.Clear();
        _slider_main_text_labels.Clear();
        _slider_text_labels_left.Clear();
        _slider_text_labels_right.Clear();
    }

    private void AddTextLabel(string text, FLabelAlignment alignment = FLabelAlignment.Center, bool has_big_text = false) {
        float text_height = (has_big_text ? 2f : 1f) * _font_height;
        if (_text_labels.Count == 0) {
            _position.y -= text_height;
        }

        OpLabel text_label = new(new Vector2(), new Vector2(20f, text_height), text, alignment, has_big_text) // minimal size.x = 20f
        {
            autoWrap = true
        };
        _text_labels.Add(text_label);
    }

    private void DrawTextLabels(ref OpTab tab) {
        if (_text_labels.Count == 0) {
            return;
        }

        float width = (_margin_x.y - _margin_x.x) / _text_labels.Count;
        foreach (OpLabel text_label in _text_labels) {
            text_label.pos = _position;
            text_label.size += new Vector2(width - 20f, 0.0f);
            tab.AddItems(text_label);
            _position.x += width;
        }

        _position.x = _margin_x.x;
        _text_labels.Clear();
    }
}
