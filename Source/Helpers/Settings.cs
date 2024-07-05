using HugsLib;
using HugsLib.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace XenoRitual.Helpers
{
    internal class Settings:ModBase
    {
        public override string ModIdentifier
        {
            get { return "XenoRitual"; }
        }
        private SettingHandle<bool> _IsSicknessEnabled;

        private SettingHandle<int> _ResAmmount;
        private SettingHandle<int> _WorkAmmount;
        public override void DefsLoaded()
        {
            _IsSicknessEnabled = Settings.GetHandle<bool>(
                "IsSicknessEnabled",
                "Enable gene reconstruction sickness",
                "Enable gene reconstruction sickness on ritualist",
                true);
            _ResAmmount = Settings.GetHandle<int>(
                "ResAmmount",
                "Resourrce ammount",
                "Ammount of resource to start ritual",
                75);
            _WorkAmmount = Settings.GetHandle<int>(
                "WorkAmmount",
                "Work ammount",
                "Ammount of work to complete ritual",
                300);
            StaticModVariables.isSicknessEnabled = _IsSicknessEnabled.Value;
            StaticModVariables.ResourceAmmount = _ResAmmount.Value;
            StaticModVariables.WorkAmmount = _WorkAmmount.Value;
        }

        public override void SettingsChanged()
        {
            base.SettingsChanged();
            StaticModVariables.isSicknessEnabled = _IsSicknessEnabled.Value;
            StaticModVariables.ResourceAmmount = _ResAmmount.Value;
            StaticModVariables.WorkAmmount = _WorkAmmount.Value;
        }

    }
}
