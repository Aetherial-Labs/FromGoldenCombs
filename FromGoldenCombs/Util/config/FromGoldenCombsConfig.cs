using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Vintagestory.API.Common;

namespace FromGoldenCombs.Util.config
{
    class FromGoldenCombsConfig
    {
        /// <summary>The number of days until a skep is ready to harvest when using 30 day months</summary>
        public float SkepDaysToHarvestIn30DayMonths = 7;
        /// <summary>The number of days until a ceramic hive is ready to harvest when using 30 day months</summary>
        public float ClayPotDaysToHarvestIn30DayMonths = 7;
        /// <summary>The number of days until a Langstroth Hive is ready to harvest when using 30 day months</summary>
        public float LangstrothDaysToHarvestIn30DayMonths = 3.5f;
        /// <summary>Maximum number of langstroth blocks that can be put in a Langstroth Stack</summary>
        public int MaxStackSize = 6;
        /// <summary>The default durability of beeframes</summary>
        public int baseframedurability = 32;
        /// <summary>The minimum number of frames filled per cycle</summary>
        public int minFramePerCycle = 2;
        /// <summary>The maximum number of frames filled per cycle</summary>
        public int maxFramePerCycle = 3;
        /// <summary>Whether to show the time left until new comb/frames are produced</summary>
        public bool showcombpoptime = true;

        /// <summary>Minimum number of honeycombs produced when a honeypot is harvested from a ceramic hive</summary>
        /// <value>The ceramic pot minimum yield.</value>
        public int CeramicPotMinYield { get; set; } = 2;

        /// <summary>Gets or sets the maximum honey produced when a honeypot is harvest from a ceramic hive.</summary>
        /// <value>The ceramic pot maximum yield.</value>
        public int CeramicPotMaxYield { get; set; } = 5;

        /// <summary>Minimum number of frames filled per cycle in a Langstroth Hive</summary>
        /// <value>The frame minimum yield.</value>
        public int FrameMinYield { get; set; } = 2;

        /// <summary>Maximum number of frames filled per cycle in a Langstroth Hive</summary>
        /// <value>The frame maximum yield.</value>
        public int FrameMaxYield { get; set; } = 5;
        /// <summary>Gets or sets the minimum honeycomb yield from a skep.</summary>
        /// <value>The skep minimum yield.</value>
        public int SkepMinYield { get; set; } = 1;
        /// <summary>Gets or sets the maximum honeycomb yield from a skep.</summary>
        /// <value>The skep maximum yield.</value>
        public int SkepMaxYield { get; set; } = 3;

        //private ArrayList HiveSeasons = new();

        public FromGoldenCombsConfig()
        { }

        public static FromGoldenCombsConfig Current { get; set; }


        public static FromGoldenCombsConfig GetDefault()
        {
            FromGoldenCombsConfig defaultConfig = new();

            defaultConfig.SkepDaysToHarvestIn30DayMonths = 7;
            defaultConfig.ClayPotDaysToHarvestIn30DayMonths = 7;
            defaultConfig.LangstrothDaysToHarvestIn30DayMonths = 7f;
            defaultConfig.SkepMinYield = 1;
            defaultConfig.SkepMaxYield = 3;
            defaultConfig.CeramicPotMinYield = 2;
            defaultConfig.CeramicPotMaxYield = 4;
            defaultConfig.minFramePerCycle = 2;
            defaultConfig.maxFramePerCycle = 3;
            defaultConfig.FrameMinYield = 2;
            defaultConfig.FrameMaxYield = 4;
            defaultConfig.MaxStackSize = 6;
            defaultConfig.baseframedurability = 32;
            defaultConfig.minFramePerCycle = 2;
            defaultConfig.maxFramePerCycle = 4;
            defaultConfig.showcombpoptime = true;
            return defaultConfig;
        }

    }
}
