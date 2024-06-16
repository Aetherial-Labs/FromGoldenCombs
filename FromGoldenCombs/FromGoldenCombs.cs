using Vintagestory.API.Common;
using FromGoldenCombs.Blocks;
using FromGoldenCombs.BlockEntities;
using FromGoldenCombs.config;
using FromGoldenCombs.Blocks.Langstroth;
using FromGoldenCombs.Items;
using VFromGoldenCombs.Blocks.Langstroth;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace FromGoldenCombs
{
    class FromGoldenCombs : ModSystem
    {
        enum EnumHivePopSize
        {
            Poor = 0,
            Decent = 1,
            Large = 2
        }

        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return true;
        }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            //BlockEntities
            api.RegisterBlockEntityClass("fgcbeehive", typeof(FGCBeehive));
            api.RegisterBlockEntityClass("beceramichive", typeof(BECeramicBroodPot));
            api.RegisterBlockEntityClass("belangstrothsuper", typeof(BELangstrothSuper));
            api.RegisterBlockEntityClass("belangstrothstack", typeof(BELangstrothStack));
            api.RegisterBlockEntityClass("beframerack", typeof(BEFrameRack));

            //Blocks
            api.RegisterBlockClass("ceramicbroodpot", typeof(CeramicBroodPot));
            api.RegisterBlockClass("hivetop", typeof(ClayHiveTop));
            api.RegisterBlockClass("rawceramichive", typeof(RawBroodPot));
            api.RegisterBlockClass("langstrothsuper", typeof(LangstrothSuper));
            api.RegisterBlockClass("langstrothbrood", typeof(LangstrothBrood));
            api.RegisterBlockClass("langstrothbase", typeof(LangstrothBase));
            api.RegisterBlockClass("langstrothstack", typeof(LangstrothStack));
            api.RegisterBlockClass("framerack", typeof(FrameRack));

            //Items
            api.RegisterItemClass("langstrothpartcore", typeof(LangstrothPartCore));


            try
            {
                var Config = api.LoadModConfig<FromGoldenCombsConfig>("fromgoldencombs.json");
                if (Config != null)
                {
                    api.Logger.Notification(Lang.Get("modconfigload"));
                    FromGoldenCombsConfig.Current = Config;
                }
                else
                {
                    api.Logger.Notification(Lang.Get("nomodconfig"));
                    FromGoldenCombsConfig.Current = FromGoldenCombsConfig.GetDefault();
                }
            }
            catch
            {
                FromGoldenCombsConfig.Current = FromGoldenCombsConfig.GetDefault();
                api.Logger.Error(Lang.Get("defaultloaded"));
            }
            finally
            {
                if (FromGoldenCombsConfig.Current.SkepDaysToHarvestIn30DayMonths <= 0)
                    FromGoldenCombsConfig.Current.SkepDaysToHarvestIn30DayMonths = 7;
                if (FromGoldenCombsConfig.Current.ClayPotDaysToHarvestIn30DayMonths <= 0)
                    FromGoldenCombsConfig.Current.ClayPotDaysToHarvestIn30DayMonths = 7;
                if (FromGoldenCombsConfig.Current.LangstrothDaysToHarvestIn30DayMonths <= 0)
                    FromGoldenCombsConfig.Current.LangstrothDaysToHarvestIn30DayMonths = 1.2f;
                api.StoreModConfig(FromGoldenCombsConfig.Current, "fromgoldencombs.json");
            }
        }
    }
}
