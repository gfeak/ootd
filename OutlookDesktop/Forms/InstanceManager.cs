﻿using System;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Windows.Forms;
using Microsoft.Win32;
using OutlookDesktop.Properties;
using BitFactory.Logging;

namespace OutlookDesktop.Forms
{
    public partial class InstanceManager : Form
    {
        private MainForm[] _mainFormInstances;

        public InstanceManager()
        {
            InitializeComponent();

            if (GlobalPreferences.IsFirstRun)
            {
                trayIcon.ShowBalloonTip(2000, Resources.OotdRunning, Resources.RightClickToConfigure, ToolTipIcon.Info);

                ConfigLogger.Instance.LogDebug("First Run");
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                // Turn on WS_EX_TOOLWINDOW style bit to hide window from alt-tab
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80;
                return cp;
            }
        }

        public void LoadInstances()
        {
            ConfigLogger.Instance.LogDebug("Loading app settings from registry");

            // Each subkey in our main registry key represents an instance. 
            // Read each subkey and load the instance.
            using (RegistryKey appReg = Registry.CurrentUser.CreateSubKey("Software\\" + Application.CompanyName + "\\" + Application.ProductName))
            {
                ConfigLogger.Instance.LogDebug("Settings Found.");
                if (appReg != null)
                    if (appReg.SubKeyCount > 1)
                    {
                        ConfigLogger.Instance.LogDebug("Multiple instances to load");

                        // There are multiple instances defined, so we build the context menu strip dynamically.
                        trayIcon.ContextMenuStrip = new ContextMenuStrip();
                        trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Add Instance", null, AddInstanceMenu_Click));
                        trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());

                        _mainFormInstances = new MainForm[appReg.SubKeyCount];

                        var instanceSubmenu = new ToolStripMenuItem[appReg.SubKeyCount];
                        var count = 0;

                        // each instance will get it's own submenu in the main context menu.
                        foreach (string instanceName in appReg.GetSubKeyNames())
                        {
                            ConfigLogger.Instance.LogDebug(String.Format("Instanciating up instance {0}", instanceName));
                            _mainFormInstances[count] = new MainForm(instanceName);

                            // hook up the instance removed/renamed event handlers so that we can
                            // remove/rename the appropriate menu item from the context menu.
                            _mainFormInstances[count].InstanceRemoved += InstanceRemovedEventHandler;
                            _mainFormInstances[count].InstanceRenamed += InstanceRenamedEventHandler;

                            // create the submenu for the instance
                            instanceSubmenu[count] = new ToolStripMenuItem(instanceName, null, null, instanceName);
                            trayIcon.ContextMenuStrip.Items.Add(instanceSubmenu[count]);

                            // add the name of the instance to the top of the submenu so it's clear which
                            // instance the submenu belongs to.
                            _mainFormInstances[count].TrayMenu.Items.Insert(0, new ToolStripMenuItem(instanceName));
                            _mainFormInstances[count].TrayMenu.Items[0].BackColor = Color.Gainsboro;

                            // the submenu items are set to the contenxt menu defined in the form's instance.
                            instanceSubmenu[count].DropDown = _mainFormInstances[count].TrayMenu;

                            _mainFormInstances[count].TrayMenu.Items["ExitMenu"].Visible = false;

                            // finally, show the form.
                            ConfigLogger.Instance.LogDebug(string.Format("Showing Instance {0}", instanceName));
                            _mainFormInstances[count].Show();
                            UnsafeNativeMethods.SendWindowToBack(_mainFormInstances[count]);
                            count++;
                        }

                        // add the rest of the necessary menu items to the main context menu.
                        trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
                        trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.StartWithWindows, null, StartWithWindowsMenu_Click, "StartWithWindows"));
                        trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
                        trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.HideAll, null, HideShowAllMenu_Click,"HideShowMenu"));
                        trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.DisableEditing, null, DisableEnableEditingMenu_Click,"DisableEnableEditingMenu"));
                        trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.About, null, AboutMenu_Click,"AboutMenu"));
                        trayIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem(Resources.Exit, null, ExitMenu_Click, "ExitMenu"));
                    }
                    else
                    {
                        // this is a first run, or there is only 1 instance defined.
                        var instanceName = appReg.SubKeyCount == 1 ? appReg.GetSubKeyNames()[0] : "Default Instance";

                        // create our instance and set the context menu to one defined in the form instance.
                        _mainFormInstances = new MainForm[1];
                        _mainFormInstances[0] = new MainForm(instanceName);
                        trayIcon.ContextMenuStrip = _mainFormInstances[0].TrayMenu;

                        // remove unnecessary menu items
                        trayIcon.ContextMenuStrip.Items["RemoveInstanceMenu"].Visible = false;
                        trayIcon.ContextMenuStrip.Items["RenameInstanceMenu"].Visible = false;

                        // add global menu items that don't apply to the instance.
                        trayIcon.ContextMenuStrip.Items.Insert(0, new ToolStripMenuItem(Resources.AddInstance, null, AddInstanceMenu_Click));
                        trayIcon.ContextMenuStrip.Items.Insert(1, new ToolStripSeparator());
                        trayIcon.ContextMenuStrip.Items.Insert(12, new ToolStripMenuItem(Resources.StartWithWindows, null, StartWithWindowsMenu_Click, "StartWithWindows"));
                        trayIcon.ContextMenuStrip.Items.Insert(18, new ToolStripMenuItem(Resources.About, null, AboutMenu_Click, "AboutMenu"));

                        // finally, show the form.
                        _mainFormInstances[0].Show();
                        UnsafeNativeMethods.SendWindowToBack(_mainFormInstances[0]);
                    }
            }

            var startWithWindowsMenu = (ToolStripMenuItem)trayIcon.ContextMenuStrip.Items["StartWithWindows"];

            startWithWindowsMenu.Checked = GlobalPreferences.StartWithWindows;
        }

        private void ChangeTrayIconDate()
        {
            // get new instance of the resource manager.  This will allow us to look up a resource by name.
            var resourceManager = new ResourceManager("OutlookDesktop.Properties.Resources", typeof(Resources).Assembly);

            DateTime today = DateTime.Now;

            // find the icon for the today's day of the month and replace the tray icon with it.
            trayIcon.Icon = (Icon)resourceManager.GetObject("_" + today.Date.Day, CultureInfo.CurrentCulture);
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            // update day of the month in the tray
            ChangeTrayIconDate();
        }

        private void ExitMenu_Click(object sender, EventArgs e)
        {
            foreach (MainForm form in _mainFormInstances)
            {
                form.Dispose();
            }
            Application.Exit();
        }

        private void AddInstanceMenu_Click(object sender, EventArgs e)
        {
            var result = InputBox.Show(this, "", Resources.NewInstanceName, String.Empty, InputBox_Validating);
            if (result.Ok)
            {
                var mainForm = new MainForm(result.Text);
                mainForm.Dispose();
                LoadInstances();
            }
        }

        private static void InputBox_Validating(object sender, InputBoxValidatingEventArgs e)
        {
            if (String.IsNullOrEmpty(e.Text.Trim()))
            {
                e.Cancel = true;
                e.Message = "Required";
            }
        }

        private static void AboutMenu_Click(object sender, EventArgs e)
        {
            var aboutForm = new AboutBox();
            aboutForm.ShowDialog();
        }

        private void StartWithWindowsMenu_Click(object sender, EventArgs e)
        {
            var startWithWindowsMenu = (ToolStripMenuItem)trayIcon.ContextMenuStrip.Items["StartWithWindows"];
            if (startWithWindowsMenu.Checked)
            {
                GlobalPreferences.StartWithWindows = false;
                startWithWindowsMenu.Checked = false;
            }
            else
            {
                GlobalPreferences.StartWithWindows = true;
                startWithWindowsMenu.Checked = true;
            }
        }

        private void HideShowAllMenu_Click(object sender, EventArgs e)
        {
            ShowHideAllInstances();
        }

        private void ShowHideAllInstances()
        {
            if (trayIcon.ContextMenuStrip.Items["HideShowMenu"].Text == Resources.HideAll)
            {
                foreach (MainForm form in _mainFormInstances)
                {
                    form.Visible = false;
                    form.TrayMenu.Items["HideShowMenu"].Text = Resources.Show;
                }
                trayIcon.ContextMenuStrip.Items["HideShowMenu"].Text = Resources.ShowAll;
            }
            else if (trayIcon.ContextMenuStrip.Items["HideShowMenu"].Text == Resources.ShowAll)
            {
                foreach (MainForm form in _mainFormInstances)
                {
                    form.Visible = true;
                    form.TrayMenu.Items["HideShowMenu"].Text = Resources.Hide;
                }
                trayIcon.ContextMenuStrip.Items["HideShowMenu"].Text = Resources.HideAll;
            }
        }

        private void DisableEnableEditingMenu_Click(object sender, EventArgs e)
        {
            DisableEnableAllInstances();
        }

        private void DisableEnableAllInstances()
        {
            if (trayIcon.ContextMenuStrip.Items["DisableEnableEditingMenu"].Text == Resources.DisableEditing)
            {
                foreach (MainForm form in _mainFormInstances)
                {
                    form.Enabled = false;
                    form.TrayMenu.Items["DisableEnableEditingMenu"].Text = Resources.EnableEditing;
                }
                trayIcon.ContextMenuStrip.Items["DisableEnableEditingMenu"].Text = Resources.EnableEditing;
            }
            else if (trayIcon.ContextMenuStrip.Items["DisableEnableEditingMenu"].Text == Resources.EnableEditing)
            {
                foreach (MainForm form in _mainFormInstances)
                {
                    form.Enabled = true;
                    form.TrayMenu.Items["DisableEnableEditingMenu"].Text = Resources.DisableEditing;
                }
                trayIcon.ContextMenuStrip.Items["DisableEnableEditingMenu"].Text = Resources.DisableEditing;
            }
        }

        private void InstanceRemovedEventHandler(Object sender, InstanceRemovedEventArgs e)
        {
            // remove the menu item for the removed instance.
            trayIcon.ContextMenuStrip.Items.RemoveByKey(e.InstanceName);

            // if we only have one instance left, reload everything so that the context
            // menu only shows the one instances' menu items.
            LoadInstances();
        }

        private void InstanceRenamedEventHandler(Object sender, InstanceRenamedEventArgs e)
        {
            // remove the menu item for the removed instance.
            trayIcon.ContextMenuStrip.Items[e.OldInstanceName].Text = e.NewInstanceName;
            trayIcon.ContextMenuStrip.Items[e.OldInstanceName].Name = e.NewInstanceName;
        }

        private void TrayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                ShowHideAllInstances();
        }
    }
}