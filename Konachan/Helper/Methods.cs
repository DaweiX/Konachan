using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Konachan.Helper
{
    class Methods
    {
        /// <summary>
        /// 转换Linux时间戳为标准时间
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string LinuxToData(string str)
        {
            try
            {
                long sec = long.Parse(str);
                DateTimeOffset start = DateTimeOffset.FromUnixTimeSeconds(sec);
                return start.DateTime.ToLocalTime().ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取存储文件夹
        /// </summary>
        public async static Task<StorageFolder> GetMyFolderAsync()
        {
            StorageFolder folder = null;
            if (!SettingHelper.ContainsKey("_path"))
            {
                SettingHelper.SetValue("_path", string.Empty);
                folder = await KnownFolders.PicturesLibrary.CreateFolderAsync("Konachan", CreationCollisionOption.OpenIfExists);
            }
            else
            {
                string path = SettingHelper.GetValue("_path").ToString();
                if (!string.IsNullOrEmpty(path))
                {
                    try
                    {
                        folder = await StorageFolder.GetFolderFromPathAsync(path);
                    }
                    catch
                    {
                        folder = await KnownFolders.PicturesLibrary.CreateFolderAsync("Konachan", CreationCollisionOption.OpenIfExists);
                    }
                }
                else
                {
                    folder = await KnownFolders.PicturesLibrary.CreateFolderAsync("Konachan", CreationCollisionOption.OpenIfExists);
                }
            }
            return folder;
        }
    }
}
