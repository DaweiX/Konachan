using Windows.Storage;

namespace Konachan.Helper
{
    class SettingHelper
    {
        public enum DType
        {
            PC,
            Mobile,
            Unknown
        }
        static ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
        /// <summary>
        /// 获取指定键的值
        /// </summary>
        /// <param name="key">键名称</param>
        /// <returns></returns>
        public static object GetValue(string key)
        {
            if (container.Values[key] != null)
            {
                return container.Values[key];
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 设置指定键的值
        /// </summary>
        /// <param name="key">键名称</param>
        /// <param name="value">值</param>
        public static void SetValue(string key, object value)
        {
            container.Values[key] = value;
        }
        /// <summary>
        /// 指示应用容器内是否存在某键
        /// </summary>
        /// <param name="key">键名称</param>
        /// <returns></returns>
        public static bool ContainsKey(string key)
        {
            if (container.Values[key] != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 获取设备类型
        /// </summary>
        /// <returns></returns>
        public static DType DeviceType
        {
            get
            {
                string type = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;
                switch (type)
                {
                    case "Windows.Desktop": return DType.PC;
                    case "Windows.Mobile": return DType.Mobile;
                    default: return DType.Unknown;
                }
            }        
        }
    }
}
