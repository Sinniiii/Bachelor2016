using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Data.Linq;
using System.Windows;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Globalization;
using System.Threading;

namespace ProductBrowser
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            SetImportCatalog();
            SetLanguage();
        }

        private void SetImportCatalog()
        {
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                DirectoryCatalog catalog = new DirectoryCatalog(path);
                CompositionContainer container = new CompositionContainer(catalog);
                container.ComposeParts(BaseModel.Instance.ImportCatalog);

            }
            catch (Exception ex)
            {

            }
        }

        private void SetLanguage()
        {
            try
            {
                ABBDataClassesDataContext dc = new ABBDataClassesDataContext();
                Table<Setting> settings;
                settings = dc.GetTable<Setting>();
                Setting configData = settings.Where(x => x.SettingName.Equals("Current")).FirstOrDefault();
                string cultureCode;
                if (configData.Language.ToLower().Equals("english"))
                    cultureCode = "en";
                else
                    cultureCode = "nb-NO";
                CultureInfo cultureInfo = new CultureInfo(cultureCode);
                Thread.CurrentThread.CurrentCulture = cultureInfo;
                Thread.CurrentThread.CurrentUICulture = cultureInfo;
                var dictionary = (from d in BaseModel.Instance.ImportCatalog.ResourceDictionaryList
                                  where d.Metadata.ContainsKey("Culture")
                                  && d.Metadata["Culture"].ToString().Equals(cultureCode)
                                  select d).FirstOrDefault();
                if (dictionary != null && dictionary.Value != null)
                {
                    this.Resources.MergedDictionaries.Add(dictionary.Value);
                }
            }
            catch (Exception ex)
            {
            }
        }       
    }
}