using OptionalUI;
using UnityEngine;

namespace InfiniteSpears
{
    public class Options : OptionInterface
    {
        public Options() : base(MainMod.instance)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            Tabs = new OpTab[1];
            Tabs[0] = new OpTab("Options");


            // Labels //
            float fontHeight = 20f;
            float boxWidth = 500f;
            float spacing = 20f;
            float left_margin = 0.5f * (600f - boxWidth);
            float position_x;
            float position_y;
            float sliderHeight = 40f;
            float sliderSize_x = 200f;
            float center = 300f;

            // Title
            position_x = left_margin;
            position_y = 600f - spacing - 2f * fontHeight;
            Tabs[0].AddItems(new OpLabel(new Vector2(position_x, position_y), new Vector2(boxWidth, 2f * fontHeight), "Infinite Spears Mod", bigText: true));

            // Subtitle
            position_y += -0.5f * spacing - fontHeight;
            Tabs[0].AddItems(new OpLabel(new Vector2(position_x, position_y), new Vector2(0.5f * boxWidth, fontHeight), "Version " + MainMod.instance?.Version, alignment: FLabelAlignment.Left));
            Tabs[0].AddItems(new OpLabel(new Vector2(position_x + 0.5f * boxWidth, position_y), new Vector2(0.5f * boxWidth, fontHeight), "by SchuhBaum", alignment: FLabelAlignment.Right));


            // Options box content //
            position_x += spacing;

            // maxSpearCount Slider
            float boxEndPosition = position_y - 2f * spacing;

            position_y = boxEndPosition - spacing - fontHeight;
            Tabs[0].AddItems(new OpLabel(new Vector2(position_x, position_y), new Vector2(boxWidth - 2f * spacing, fontHeight), "Number of BackSpears"));

            position_y += -0.5f * spacing - sliderHeight;
            Tabs[0].AddItems(new OpLabel(new Vector2(position_x, position_y + 5f), new Vector2(center - 0.5f * sliderSize_x - 0.5f * spacing - position_x, fontHeight), "1", alignment: FLabelAlignment.Right));
            OpSlider zoomSlider = new(new Vector2(center - 0.5f * sliderSize_x, position_y), "maxSpearCountSlider", new RWCustom.IntVector2(1, 7), 200, defaultValue: 1)
            {
                size = new Vector2(sliderSize_x, fontHeight),
                description = "The default value is 1 (infinity). For the values X > 1, the player can simply carry X spears on the back."
            };
            Tabs[0].AddItems(zoomSlider);
            Tabs[0].AddItems(new OpLabel(new Vector2(center + 0.5f * sliderSize_x + 0.5f * spacing, position_y + 5f), new Vector2(600f - position_x, fontHeight), "7", alignment: FLabelAlignment.Left));

            // draw box around
            position_x = left_margin;
            position_y += -spacing;
            Tabs[0].AddItems(new OpRect(new Vector2(position_x, position_y), new Vector2(boxWidth, boxEndPosition - position_y)));
        }

        public override void Update(float dt)
        {
            base.Update(dt);
        }

        public override void ConfigOnChange()
        {
            base.ConfigOnChange();
            SpearOnBackMod.maxSpearCount = int.Parse(config["maxSpearCountSlider"]);
            Debug.Log("InfiniteSpears: maxSpearCount " + SpearOnBackMod.maxSpearCount);
        }
    }
}