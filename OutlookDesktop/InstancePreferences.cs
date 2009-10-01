using System;
using System.Windows.Forms;
using Microsoft.Win32;
using OutlookDesktop.Properties;
using System.Globalization;

namespace OutlookDesktop
{
    public class InstancePreferences
    {
        public const double DefaultOpacity = 0.5;
        public const int DefaultLeftPosition = 100;
        public const int DefaultTopPosition = 100;
        public const int DefaultHeight = 500;
        public const int DefaultWidth = 700;

        private readonly RegistryKey _appReg;

        public InstancePreferences(String instanceName)
        {
            try
            {
                _appReg = Registry.CurrentUser.CreateSubKey("Software\\" + Application.CompanyName + "\\" + Application.ProductName + "\\" + instanceName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Resources.ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        ~InstancePreferences()
        {
            _appReg.Close();
        }

        /// <summary>
        /// Main Window Opacity.
        /// </summary>
        public double Opacity
        {
            get
            {
                double opacity = DefaultOpacity;

                if (double.TryParse((string)_appReg.GetValue("Opacity", opacity.ToString("G", CultureInfo.CurrentCulture)), out opacity))
                    return opacity;
                
                return DefaultOpacity;
            }
            set
            {
                _appReg.SetValue("Opacity", value);
            }
        }

        /// <summary>
        /// Main Window Left.
        /// </summary>
        public int Left
        {
            get
            {
                return (int)_appReg.GetValue("Left", DefaultLeftPosition);
            }
            set
            {
                _appReg.SetValue("Left", value);
            }
        }

        /// <summary>
        /// Main Window Top.
        /// </summary>
        public int Top
        {
            get
            {
                return (int)_appReg.GetValue("Top", DefaultTopPosition);
            }
            set
            {
                _appReg.SetValue("Top", value);
            }
        }

        /// <summary>
        /// Main Window Width.
        /// </summary>
        public int Width
        {
            get
            {
                return (int)_appReg.GetValue("Width", DefaultWidth);
            }
            set
            {
                _appReg.SetValue("Width", value);
            }
        }

        /// <summary>
        /// Main Window Height.
        /// </summary>
        public int Height
        {
            get
            {
                return (int)_appReg.GetValue("Height", DefaultHeight);
            }
            set
            {
                _appReg.SetValue("Height", value);
            }
        }

        public string OutlookFolderName
        {
            get
            {
                return (string)_appReg.GetValue("CurrentViewType", "Calendar");
            }
            set
            {
                _appReg.SetValue("CurrentViewType", value);
            }
        }

        public string OutlookFolderView
        {
            get
            {
                return (string)_appReg.GetValue("OutlookView", "Day/Week/Month");
            }
            set
            {
                _appReg.SetValue("OutlookView", value);
            }
        }


        public string OutlookFolderEntryId
        {
            get
            {
                return (string)_appReg.GetValue("FolderEntryId", "");
            }
            set
            {
                _appReg.SetValue("FolderEntryId", value);
            }
        }


        public string OutlookFolderStoreId
        {
            get
            {
                return (string)_appReg.GetValue("FolderStoreId", "");
            }
            set
            {
                _appReg.SetValue("FolderStoreId", value);
            }
        }
    
    }
}
