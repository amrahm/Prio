using System;

namespace MainConfig {
    public interface IMainConfigService {
        void ShowConfigWindow();
        void CloseConfigWindow();
        public event Action ApplySettingsPress;
        public void InvokeApplySettings();
    }
}