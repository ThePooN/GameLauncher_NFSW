﻿using GameLauncher.App.Classes.LauncherCore.FileReadWrite;
using GameLauncher.App.Classes.LauncherCore.Global;
using GameLauncher.App.Classes.LauncherCore.Logger;
using GameLauncher.App.Classes.LauncherCore.Support;
using GameLauncher.App.Classes.LauncherCore.Visuals;
using GameLauncher.App.Classes.SystemPlatform.Unix;
using GameLauncher.App.Classes.SystemPlatform.Windows;
using NetFwTypeLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFirewallHelper;
using WindowsFirewallHelper.Exceptions;
using WindowsFirewallHelper.FirewallRules;

namespace GameLauncher.App
{
    public partial class SecurityCenterScreen : Form
    {
        ///<summary>Cache Old Game Files Path. Just in case the User Does Remove the Old Location</summary>
        public static string CacheOldLocation;
        ///<summary>Disable Button: Firewall Rules API</summary>
        private static bool DisableButtonFRAPI = false;
        ///<summary>Disable Button: Firewall Rules Check</summary>
        private static bool DisableButtonFRC = true;
        ///<summary>Disable Button: Firewall Rules Add All</summary>
        private static bool DisableButtonFRAA = true;
        ///<summary>Disable Button: Firewall Rules Add Launcher</summary>
        private static bool DisableButtonFRAL = true;
        ///<summary>Disable Button: Firewall Rules Add Game</summary>
        private static bool DisableButtonFRAG = true;
        ///<summary>Disable Button: Firewall Rules Remove All</summary>
        private static bool DisableButtonFRRA = true;
        ///<summary>Disable Button: Firewall Rules Remove Launcher</summary>
        private static bool DisableButtonFRRL = true;
        ///<summary>Disable Button: Firewall Rules Remove Game</summary>
        private static bool DisableButtonFRRG = true;

        public SecurityCenterScreen()
        {
            InitializeComponent();
            SetVisuals();
            this.Closing += (x, y) =>
            {
                if (DisableButtonFRAPI) { DisableButtonFRAPI = false; }
            };
        }
        /// <summary>
        /// Sets the Color for Buttons
        /// </summary>
        /// <param name="Elements">Button Control Name</param>
        /// <param name="Color">Range 0-3 Sets Colored Button.
        /// <code>"0" Checking Blue</code><code>"1" Success Green</code><code>"2" Warning Orange</code><code>"3" Error Red</code></param>
        /// <param name="EnabledORDisabled">Enables or Disables the Button</param>
        /// <remarks>Range 0-3 Sets Colored Button.
        /// <code>"0" Checking Blue</code><code>"1" Success Green</code><code>"2" Warning Orange</code><code>"3" Error Red</code></remarks>
        private void ButtonsColorSet(Button Elements, int Color, bool EnabledORDisabled)
        {
            switch (Color)
            {
                /* Checking Blue */
                case 0:
                    Elements.ForeColor = Theming.BlueForeColorButton;
                    Elements.BackColor = Theming.BlueBackColorButton;
                    Elements.FlatAppearance.BorderColor = Theming.BlueBorderColorButton;
                    Elements.FlatAppearance.MouseOverBackColor = Theming.BlueMouseOverBackColorButton;
                    Elements.Enabled = EnabledORDisabled;
                    break;
                /* Success Green */
                case 1:
                    Elements.ForeColor = Theming.GreenForeColorButton;
                    Elements.BackColor = Theming.GreenBackColorButton;
                    Elements.FlatAppearance.BorderColor = Theming.GreenBorderColorButton;
                    Elements.FlatAppearance.MouseOverBackColor = Theming.GreenMouseOverBackColorButton;
                    Elements.Enabled = EnabledORDisabled;
                    break;
                /* Warning Orange */
                case 2:
                    Elements.ForeColor = Theming.YellowForeColorButton;
                    Elements.BackColor = Theming.YellowBackColorButton;
                    Elements.FlatAppearance.BorderColor = Theming.YellowBorderColorButton;
                    Elements.FlatAppearance.MouseOverBackColor = Theming.YellowMouseOverBackColorButton;
                    Elements.Enabled = EnabledORDisabled;
                    break;
                /* Error Red */
                case 3:
                    Elements.ForeColor = Theming.RedForeColorButton;
                    Elements.BackColor = Theming.RedBackColorButton;
                    Elements.FlatAppearance.BorderColor = Theming.RedBorderColorButton;
                    Elements.FlatAppearance.MouseOverBackColor = Theming.RedMouseOverBackColorButton;
                    Elements.Enabled = EnabledORDisabled;
                    break;
                /* Unknown Gray */
                default:
                    Elements.ForeColor = Theming.GrayForeColorButton;
                    Elements.BackColor = Theming.GrayBackColorButton;
                    Elements.FlatAppearance.BorderColor = Theming.GrayBorderColorButton;
                    Elements.FlatAppearance.MouseOverBackColor = Theming.GrayMouseOverBackColorButton;
                    Elements.Enabled = EnabledORDisabled;
                    break;
            }
        }
        /// <summary>Checks WMI Query on if Windows Defender is Enabled</summary>
        /// <param name="Query">Query a Specific Collection</param>
        /// <returns><code>True or False</code></returns>
        private static bool GetDefenderStatus(string Query)
        {
            if (!UnixOS.Detected())
            {
                ManagementObjectSearcher ObjectPath = null;
                ManagementObjectCollection ObjectCollection = null;

                try
                {
                    ObjectPath = new ManagementObjectSearcher(Path.Combine("root", "Microsoft", "Windows", "Defender"),
                        "SELECT * FROM MSFT_MpComputerStatus");
                    ObjectCollection = ObjectPath.Get();

                    foreach (ManagementBaseObject SearchBase in ObjectCollection)
                    {
                        if (ObjectCollection != null)
                        {
                            if (bool.TryParse(SearchBase.Properties[Query].Value.ToString(), out bool TrueOrFalse))
                            {
                                return (bool)SearchBase.Properties[Query].Value;
                            }
                        }
                    }
                }
                catch (ManagementException Error)
                {
                    LogToFileAddons.OpenLog("Windows Defender Status [M.E.]", null, Error, null, true);
                }
                catch (COMException Error)
                {
                    LogToFileAddons.OpenLog("Windows Defender Status [C.O.M.]", null, Error, null, true);
                }
                catch (Exception Error)
                {
                    LogToFileAddons.OpenLog("Windows Defender Status", null, Error, null, true);
                }
                finally
                {
                    if (ObjectPath != null) { ObjectPath.Dispose(); }
                    if (ObjectCollection != null) { ObjectCollection.Dispose(); }
                }
            }

            return false;
        }
        /// <summary>Checks Windows Defender on if it's Enabled or Disabled by the User or Third-Party Program</summary>
        /// <remarks>Doesn't checks If the Service is Disabled or a Third-Party Program reports an Incorrect Value</remarks>
        /// <returns><code>True or False</code></returns>
        public static bool Defender()
        {
            if (!UnixOS.Detected())
            {
                return GetDefenderStatus("AntivirusEnabled") && 
                    GetDefenderStatus("AntispywareEnabled") && 
                    GetDefenderStatus("RealTimeProtectionEnabled");
            }

            return false;
        }
        /// <summary>Windows Defender: Checks Defender's Current Exclusion List</summary>
        /// <returns>String-Array of Exclusions</returns>
        private static string[] ExclusionCheck()
        {
            if (!UnixOS.Detected())
            {
                ManagementObjectSearcher ObjectPath = null;
                ManagementObjectCollection ObjectCollection = null;

                try
                {
                    ObjectPath = new ManagementObjectSearcher(Path.Combine("root", "Microsoft", "Windows", "Defender"),
                        "SELECT * FROM MSFT_MpPreference");
                    ObjectCollection = ObjectPath.Get();

                    foreach (ManagementBaseObject SearchBase in ObjectCollection)
                    {
                        if (ObjectCollection != null)
                        {
                            return (string[])SearchBase.Properties["ExclusionPath"].Value;
                        }
                    }
                }
                catch (ManagementException Error)
                {
                    LogToFileAddons.OpenLog("Windows Defender Exclusion Path Check [M.E.]", null, Error, null, true);
                }
                catch (COMException Error)
                {
                    LogToFileAddons.OpenLog("Windows Defender Exclusion Path Check [C.O.M.]", null, Error, null, true);
                }
                catch (Exception Error)
                {
                    LogToFileAddons.OpenLog("Windows Defender Exclusion Path Check", null, Error, null, true);
                }
                finally
                {
                    if (ObjectPath != null) { ObjectPath.Dispose(); }
                    if (ObjectCollection != null) { ObjectCollection.Dispose(); }
                }
            }

            return Array.Empty<string>();
        }
        /// <summary>
        /// Finds a Defender Exclusion in Defenders "Database"
        /// </summary>
        /// <param name="FilePath">Enter file Path</param>
        /// <returns><code>True or False</code></returns>
        private static bool ExclusionExist(string FilePath)
        {
            if (UnixOS.Detected())
            {
                return true;
            }
            else
            {
                if (ExclusionCheck() != null) 
                {
                    return ExclusionCheck().Any(FilePath.Contains);
                    /*
                    foreach (string ExistingPaths in ExclusionCheck())
                    {
                        if (ExistingPaths == FilePath)
                        {
                            return true;
                        }
                    }

                    return false; */
                }
                else { return false; }
            }
        }
        /// <summary>Windows Defender: Add an Exclusion</summary>
        /// <param name="AppName">Enter the name of the Application</param>
        /// <param name="AppPath">Enter the Application Folder</param>
        /// <returns><code>True or False</code></returns>
        private static bool AddApplicationExclusion(string AppName, string AppPath)
        {
            bool Completed = false;
            try
            {
                if (!ExclusionExist(AppPath))
                {
                    /* Remove current Exclusion and Add new location for Exclusion (Game Files Only!) */
                    using (PowerShell AddScript = PowerShell.Create())
                    {
                        AddScript.AddScript($"Add-MpPreference -ExclusionPath \"{Strings.Encode(AppPath)}\"");
                        AddScript.Invoke();
                    }

                    Completed = true;
                    Log.Completed("Windows Defender: ".ToUpper() + "Folder is now Excluded. -> " + AppPath);
                }
                else { Log.Completed("WINDOWS FIREWALL: " + AppName + " Rule is already Added"); Completed = true; }
            }
            catch (COMException Error)
            {
                LogToFileAddons.OpenLog("WINDOWS FIREWALL Add Script [C.O.M.]", null, Error, null, true);
            }
            catch (Exception Error)
            {
                LogToFileAddons.OpenLog("WINDOWS FIREWALL Add Script", null, Error, null, true);
            }

            return Completed;
        }
        /// <summary>Windows Defender: Removes an Exclusion</summary>
        /// <param name="AppName">Enter the name of the Application</param>
        /// <param name="AppPath">Enter the Application Folder</param>
        /// <returns><code>True or False</code></returns>
        private static bool RemoveExclusion(string AppName, string AppPath)
        {
            bool Completed = false;
            try
            {
                if (ExclusionExist(AppPath))
                {
                    /* Remove current Exclusion and Add new location for Exclusion (Game Files Only!) */
                    using (PowerShell RemovalScript = PowerShell.Create())
                    {
                        RemovalScript.AddScript($"Remove-MpPreference -ExclusionPath \"{Strings.Encode(AppPath)}\"");
                        RemovalScript.Invoke();
                    }

                    Completed = true;
                    Log.Completed("Windows Defender: ".ToUpper() + "Folder is no longer Excluded. -> " + AppPath);
                }
                else { Log.Completed("WINDOWS FIREWALL: " + AppName + " Rule is already Removed"); Completed = true; }
            }
            catch (COMException Error)
            {
                LogToFileAddons.OpenLog("WINDOWS FIREWALL Removal Script [C.O.M.]", null, Error, null, true);
            }
            catch (Exception Error)
            {
                LogToFileAddons.OpenLog("WINDOWS FIREWALL Removal Script", null, Error, null, true);
            }

            return Completed;
        }
        /// <summary>
        /// Checks the Firewall API Version Dynamically
        /// </summary>
        /// <returns>Firewall API Version</returns>
        private static FirewallAPIVersion FirewallAPI()
        {
            if (UnixOS.Detected())
            {
                return FirewallAPIVersion.None;
            }
            else
            {
                try { return FirewallManager.Version; }
                catch { return FirewallAPIVersion.None; }
            }
        }
        /// <summary>
        /// Checks the Firewall API Version against Versions that isn't supported
        /// </summary>
        /// <returns><code>True or False</code></returns>
        private static bool FirewallSupported()
        {
            if (UnixOS.Detected() || FirewallAPI() == FirewallAPIVersion.None)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// Checks Windows Firewall on if it's Enabled or Disabled by the User or Third-Party Program
        /// </summary>
        /// <remarks>Checks the Firewall Service at the same time</remarks>
        /// <returns><code>True or False</code></returns>
        private static bool Firewall()
        {
            try
            {
                if (!UnixOS.Detected())
                {
                    if (bool.TryParse(FirewallManager.IsServiceRunning.ToString(), out bool Result) && Result)
                    {
                        Type NetFwMgrType = Type.GetTypeFromProgID("HNetCfg.FwMgr", true);
                        INetFwMgr Mana = (INetFwMgr)Activator.CreateInstance(NetFwMgrType);

                        if (bool.TryParse(Mana.LocalPolicy.CurrentProfile.FirewallEnabled.ToString(), out bool Results))
                        {
                            return Mana.LocalPolicy.CurrentProfile.FirewallEnabled;
                        }
                    }
                }
            }
            catch (COMException Error)
            {
                LogToFileAddons.OpenLog("WINDOWS FIREWALL Check", null, Error, null, true);
            }
            catch (Exception Error)
            {
                LogToFileAddons.OpenLog("WINDOWS FIREWALL Check", null, Error, null, true);
            }

            return false;
        }
        /// <summary>
        /// Used to find a Application Rule on the system by searching Firewall "Database"
        /// </summary>
        /// <param name="Mode">Used to Specifiy how to find a Rule. Enter "Name" or "Path"</param>
        /// <param name="AppName">Used to Specifiy how to find a Rule. Provide the name of Application</param>
        /// <param name="AppPath">Used to Specifiy how to find a Rule. Provide Application Path</param>
        /// <returns>An Array of Rules</returns>
        private static IEnumerable<IFirewallRule> FindRules(string Mode, string AppName, string AppPath)
        {
            try
            {
                if (Firewall() && FirewallSupported() && (FirewallAPI() != FirewallAPIVersion.None))
                {
                    if (FirewallManager.Instance.Rules.Count != 0)
                    {
                        if (Mode == "Name")
                        {
                            return FirewallManager.Instance.Rules.Where(r => 
                            string.Equals(r.Name, AppName, StringComparison.OrdinalIgnoreCase)).ToArray();
                        }
                        else if (Mode == "Path")
                        {
                            return FirewallManager.Instance.Rules.Where(r => 
                            string.Equals(r.ApplicationName, AppPath, StringComparison.OrdinalIgnoreCase)).ToArray();
                        }
                    }
                }
            }
            catch (NotSupportedException Error)
            {
                LogToFileAddons.OpenLog("WINDOWS FIREWALL [Not Supported]", null, Error, null, true);
            }
            catch (COMException Error)
            {
                LogToFileAddons.OpenLog("WINDOWS FIREWALL [COM]", null, Error, null, true);
            }
            catch (Exception Error)
            {
                LogToFileAddons.OpenLog("WINDOWS FIREWALL", null, Error, null, true);
            }

            return Enumerable.Empty<IFirewallRule>();
        }
        /// <summary>
        /// Finds a Firewall Rule and Attempts to Remove it. 
        /// If the Rule List is empty or Encounters an Issue, it will be False.
        /// If the rule list succeeds to remove a Rule, it will be True
        /// </summary>
        /// <param name="Mode"> Used in Find Rules Helper Function. Choose and Type "Name" or "Path"</param>
        /// <param name="AppName">Used in Find Rules Helper Function. Enter name of Application</param>
        /// <param name="AppPath">Used in Find Rules Helper Function. Enter file Path</param>
        /// <param name="F_LogNote">Used to Log which additional Details</param>
        /// <returns><code>True or False</code></returns>
        private static bool RemoveRules(string Mode, string AppName, string AppPath, string F_LogNote)
        {
            try
            {
                var myRule = FindRules(Mode, AppName, AppPath).ToArray();

                if (myRule != null)
                {
                    if (Enumerable.Any(myRule))
                    {
                        int ErrorsRate = 0;

                        foreach (var rule in myRule)
                        {
                            try
                            {
                                FirewallManager.Instance.Rules.Remove(rule);
                                Log.Warning("WINDOWS FIREWALL: Removed " + AppName + " {" + F_LogNote + "} From Firewall!");
                            }
                            catch (Exception Error)
                            {
                                LogToFileAddons.OpenLog("WINDOWS FIREWALL", null, Error, null, true);
                                ErrorsRate++;
                            }
                        }

                        if (ErrorsRate == 0) { return true; }
                    }
                }
            }
            catch { }

            return false;
        }
        /// <summary>
        /// Finds a Firewall Rule in Firewall "Database"
        /// </summary>
        /// <param name="Mode">Used in Find Rules Helper Function. Choose and Type "Name" or "Path"</param>
        /// <param name="Name">Used in Find Rules Helper Function. Enter name of Application</param>
        /// <param name="Path">Used in Find Rules Helper Function. Enter file Path</param>
        /// <returns><code>True or False</code></returns>
        private static bool RuleExist(string Mode, string Name, string Path)
        {
            if (UnixOS.Detected())
            {
                return true;
            }
            else
            {
                if (FindRules(Mode, Name, Path) != null) { return FindRules(Mode, Name, Path).Any(); }
                else { return false; }
            }
        }
        /// <summary>Windows Firewall: Adds an Exclusion</summary>
        /// <param name="AppName">Enter the name of the Application</param>
        /// <param name="AppPath">Enter the Application Path (Must include exe)</param>
        /// <param name="GroupKey">Sets the rule grouping string</param>
        /// <param name="C_Description">Sets the description string of this rule</param>
        /// <param name="C_Direction">Data direction in which this rule applies to</param>
        /// <param name="C_Protocol">Sets the protocol that the rule applies to</param>
        /// <param name="F_LogNote">Notes for Logging</param>
        /// <returns><code>True or False</code></returns>
        private static bool AddApplicationRule(string AppName, string AppPath, string GroupKey, string C_Description,
                            FirewallDirection C_Direction, FirewallProtocol C_Protocol, string F_LogNote)
        {
            bool Completed = false;
            try
            {
                if (FirewallAPI() == FirewallAPIVersion.None)
                {
                    Log.Warning("WINDOWS FIREWALL: API Version not Supported");
                }
                else if (!RuleExist("Path", AppName, AppPath))
                {
                    if (FirewallAPI() != FirewallAPIVersion.FirewallLegacy)
                    {
                        Log.Info("WINDOWS FIREWALL: Supported Firewall [WASRuleWin8]");
                        FirewallWASRuleWin8 Rule = new FirewallWASRuleWin8(AppPath, FirewallAction.Allow, C_Direction,
                            FirewallProfiles.Domain | FirewallProfiles.Private | FirewallProfiles.Public)
                        {
                            ApplicationName = AppPath,
                            Name = AppName,
                            Grouping = GroupKey,
                            Description = C_Description,
                            NetworkInterfaceTypes = NetworkInterfaceTypes.Lan | NetworkInterfaceTypes.RemoteAccess | NetworkInterfaceTypes.Wireless,
                            Protocol = C_Protocol
                        };

                        if (C_Direction == FirewallDirection.Inbound)
                        {
                            Rule.EdgeTraversalOptions = EdgeTraversalAction.Allow;
                        }

                        FirewallManager.Instance.Rules.Add(Rule);
                        Log.Completed("WINDOWS FIREWALL: Finished Adding " + AppName + " to Firewall! {" + F_LogNote + "}");
                        Completed = true;
                    }
                    else
                    {
                        Log.Info("WINDOWS FIREWALL: Supported Firewall [LegacyStandard]");
                        IFirewallRule Rule = FirewallManager.Instance.CreateApplicationRule(
                            FirewallProfiles.Domain | FirewallProfiles.Private | FirewallProfiles.Public,
                            AppName, FirewallAction.Allow, AppPath, C_Protocol);
                        Rule.Direction = C_Direction;

                        FirewallManager.Instance.Rules.Add(Rule);
                        Log.Completed("WINDOWS FIREWALL: Finished Adding " + AppName + " to Firewall! {" + F_LogNote + "}");
                        Completed = true;
                    }
                }
                else { Log.Completed("WINDOWS FIREWALL: " + AppName + " Rule is already Added"); Completed = true; }
            }
            catch (FirewallWASNotSupportedException Error)
            {
                LogToFileAddons.OpenLog("WINDOWS FIREWALL [F.WAS.N.S.E]", null, Error, null, true);
            }
            catch (COMException Error)
            {
                LogToFileAddons.OpenLog("WINDOWS FIREWALL [C.O.M]", null, Error, null, true);
            }
            catch (Exception Error)
            {
                LogToFileAddons.OpenLog("WINDOWS FIREWALL", null, Error, null, true);
            }

            return Completed;
        }
        /// <summary>Function Splitter For Firewall and Defender Checks</summary>
        /// <param name="ModeType">Range 0-5 Sets the Check Status.
        /// <code>"0" Sets Launcher Path</code>
        /// <code>"1" Sets Updater Path</code><code>"2" Sets Current/New Game Path</code>
        /// <code>"3" Sets Old Game Path</code>
        /// <code>"4" Returns the Firewall Status in a form of a Boolean</code>
        /// <code>"5" Returns the Defender Status in a form of a Boolean</code>
        /// </param>
        /// <param name="ModeAPI">Range 0-2 Sets the Function Status. Each Function Returns a Boolean on if it was completed.
        /// <code>"0" Adds Rule</code>
        /// <code>"1" Removes Rule</code>
        /// <code>"2" Checks if Rule Exists</code>
        /// <code>"3" Adds Exclusion</code>
        /// <code>"4" Removes Exclusion</code>
        /// <code>"5" Checks if Exclusion Exists</code>
        /// </param>
        /// <returns><code>True or False</code></returns>
        private static bool DataBase(int ModeType, int ModeAPI)
        {
            string AppName = string.Empty;
            string AppPath = string.Empty;
            string GroupKey = string.Empty;
            string Description = string.Empty;

            switch (ModeType)
            {
                /* Launcher */
                case 0:
                    AppName = "SBRW - Game Launcher";
                    AppPath = Strings.Encode(Path.Combine(Locations.LauncherFolder, Locations.NameLauncher));
                    GroupKey = "Game Launcher for Windows";
                    Description = "Soapbox Race World";
                    break;
                /* Updater */
                case 1:
                    AppName = "SBRW - Game Launcher Updater";
                    AppPath = Strings.Encode(Path.Combine(Locations.LauncherFolder, Locations.NameUpdater));
                    GroupKey = "Game Launcher for Windows";
                    Description = "Soapbox Race World";
                    break;
                /* Current/New Game Files [2] Old Game Files [3]*/
                case 2:
                case 3:
                    AppName = "SBRW - Game";
                    AppPath = ModeType == 3 ? Strings.Encode(Path.Combine(FileSettingsSave.GameInstallation, "nfsw.exe"))
                    : Strings.Encode(Path.Combine(FileSettingsSave.GameInstallation, "nfsw.exe"));
                    GroupKey = "Need for Speed: World";
                    Description = GroupKey;
                    break;
                case 4:
                    return Firewall();
                case 5:
                    return Defender();
                default:
                    return false;
            }

            if (ModeType >= 0 && ModeType <= 3)
            {
                switch (ModeAPI)
                {
                    /* Firewall Rule Add */
                    case 0:
                        if (RuleExist("Path", AppName, AppPath) && !RuleExist("Name", AppName, AppPath))
                        {
                            /* Inbound & Outbound */
                            RemoveRules("Path", "Non-" + AppName, AppPath, "Path Match");
                        }

                        /* Inbound */
                        AddApplicationRule(AppName, AppPath, GroupKey, Description,
                            FirewallDirection.Inbound, FirewallProtocol.Any, "Inbound");
                        /* Outbound */
                        return AddApplicationRule(AppName, AppPath, GroupKey, Description,
                            FirewallDirection.Outbound, FirewallProtocol.Any, "Outbound");
                    /* Firewall Rule Removal */
                    case 1:
                        if (RuleExist("Path", AppName, AppPath) && !RuleExist("Name", AppName, AppPath))
                        {
                            /* Inbound & Outbound */
                            RemoveRules("Path", "Non-" + AppName, AppPath, "Path Match");
                        }

                        if (RuleExist("Path", AppName, AppPath) && RuleExist("Name", AppName, AppPath))
                        {
                            return RemoveRules("Path", AppName, AppPath, "Path Match");
                        }
                        else { return false; }
                    /* Firewall Rule Check (Exists) */
                    case 2:
                        return RuleExist("Path", AppName, AppPath) && RuleExist("Name", AppName, AppPath);
                    /* Defender Exclusion Add */
                    case 3:
                        return AddApplicationExclusion(AppName, AppPath);
                    /* Defender Exclusion Removal */
                    case 4:
                        return RemoveExclusion(AppName, AppPath);
                    /* Defender Exclusion Check (Exists) */
                    case 5:
                        return ExclusionExist(AppPath);
                    default:
                        return false;
                }
            }
            else { return false; }
        }
        /// <summary>
        /// Used to Enable Buttons with only Booleans
        /// </summary>
        /// <param name="ModeType">Range 0-5 Sets the Check Status.
        /// <code>"0" Sets Launcher Path</code>
        /// <code>"1" Sets Updater Path</code>
        /// <code>"2" Sets Current/New Game Path</code>
        /// <code>"3" Sets Old Game Path</code>
        /// <code>"4" Returns the Firewall Status in a form of a Boolean</code>
        /// <code>"5" Returns the Defender Status in a form of a Boolean</code>
        /// </param>
        /// <param name="ModeAPI">Range 0-2 Sets the Function Status. Each Function Returns a Boolean on if it was completed.
        /// <code>"0" Adds Rule</code>
        /// <code>"1" Removes Rule</code>
        /// <code>"2" Checks if Rule Exists</code>
        /// <code>"3" Adds Exclusion</code>
        /// <code>"4" Removes Exclusion</code>
        /// <code>"5" Checks if Exclusion Exists</code>
        /// </param>
        /// <returns><code>True or False</code></returns>
        private static bool ButtonEnabler(int ModeType, int ModeAPI)
        {
            try { return DataBase(ModeType, ModeAPI); }
            catch { return false; }
        }
        ///<summary>Button: Firewall Rules API</summary>
        private void ButtonFirewallRulesAPI_Click(object sender, EventArgs e)
        {
            if (!DisableButtonFRAPI)
            {
                DisableButtonFRAPI = true;

                if (ButtonEnabler(4, 20))
                {
                    ButtonsColorSet(ButtonFirewallRulesCheck, 2, true);
                    DisableButtonFRC = false;
                }
                else { ButtonsColorSet(ButtonFirewallRulesCheck, 3, false); DisableButtonFRC = true; }

                ButtonsColorSet(ButtonFirewallRulesAPI, 1, false);
            }
        }
        ///<summary>Button: Firewall Rules Check</summary>
        private void ButtonFirewallRulesCheck_Click(object sender, EventArgs e)
        {
            if (!DisableButtonFRC)
            {
                if (Firewall())
                {
                    ButtonsColorSet(ButtonFirewallRulesCheck, 0, true);

                    /* Both */
                    if (ButtonEnabler(0, 2) && ButtonEnabler(1, 2) && ButtonEnabler(2, 2))
                    {
                        ButtonsColorSet(ButtonFirewallRulesAddAll, 1, true);
                        DisableButtonFRAA = false;
                        ButtonsColorSet(ButtonFirewallRulesRemoveAll, 2, true);
                        DisableButtonFRRA = false;
                    }
                    else if (!ButtonEnabler(0, 2) && !ButtonEnabler(1, 2) && !ButtonEnabler(2, 2))
                    {
                        ButtonsColorSet(ButtonFirewallRulesAddAll, 2, true);
                        DisableButtonFRAA = false;
                    }
                    else 
                    { 
                        ButtonsColorSet(ButtonFirewallRulesAddAll, 3, false); 
                        DisableButtonFRAA = true;
                        ButtonsColorSet(ButtonFirewallRulesRemoveAll, 3, false);
                        DisableButtonFRRA = true;
                    }
                    /* Launcher */
                    if (ButtonEnabler(0, 2) && ButtonEnabler(1, 2))
                    {
                        ButtonsColorSet(ButtonFirewallRulesAddLauncher, 1, true);
                        DisableButtonFRAL = false;
                        ButtonsColorSet(ButtonFirewallRulesRemoveLauncher, 2, true);
                        DisableButtonFRRL = false;
                    }
                    else if (!ButtonEnabler(0, 2) && !ButtonEnabler(1, 2))
                    {
                        ButtonsColorSet(ButtonFirewallRulesAddLauncher, 2, true);
                        DisableButtonFRAL = false;
                    }
                    else 
                    { 
                        ButtonsColorSet(ButtonFirewallRulesAddLauncher, 3, false); 
                        DisableButtonFRAL = true;
                        ButtonsColorSet(ButtonFirewallRulesRemoveLauncher, 3, false);
                        DisableButtonFRRL = true;
                    }
                    /* Game */
                    if (ButtonEnabler(2, 2))
                    {
                        ButtonsColorSet(ButtonFirewallRulesAddGame, 1, true);
                        DisableButtonFRAG = false;
                        ButtonsColorSet(ButtonFirewallRulesRemoveGame, 2, true);
                        DisableButtonFRRG = false;
                    }
                    else if (!ButtonEnabler(2, 2))
                    {
                        ButtonsColorSet(ButtonFirewallRulesAddGame, 2, true);
                        DisableButtonFRAG = false;
                    }
                    else 
                    { 
                        ButtonsColorSet(ButtonFirewallRulesAddGame, 3, false); 
                        DisableButtonFRAG = true;
                        ButtonsColorSet(ButtonFirewallRulesRemoveGame, 3, false);
                        DisableButtonFRRG = true;
                    }

                    if (Firewall())
                    { ButtonsColorSet(ButtonFirewallRulesCheck, 1, true); }
                    else
                    { ButtonsColorSet(ButtonFirewallRulesCheck, 3, true); }
                }
                else
                { ButtonsColorSet(ButtonFirewallRulesCheck, 3, true); }
            }
        }
        ///<summary>Button: Firewall Rules Add All</summary>
        private void ButtonFirewallRulesAddAll_Click(object sender, EventArgs e)
        {
            if (!DisableButtonFRAA)
            {
                DisableButtonFRAA = true;

                if (ButtonEnabler(4, 20))
                {
                    ButtonsColorSet(ButtonFirewallRulesAddAll, 2, true);

                    /* Launcher & Updater */
                    if (ButtonEnabler(0, 0) && ButtonEnabler(1, 0))
                    {
                        ButtonsColorSet(ButtonFirewallRulesAddLauncher, 1, true);
                        ButtonsColorSet(ButtonFirewallRulesRemoveLauncher, 2, true);
                        DisableButtonFRRL = false;
                        FileSettingsSave.FirewallLauncherStatus = "Excluded";
                    }
                    else
                    { 
                        ButtonsColorSet(ButtonFirewallRulesAddLauncher, 3, false); 
                        FileSettingsSave.FirewallLauncherStatus = "Error"; 
                    }
                    /* Game */
                    if (ButtonEnabler(2, 0))
                    {
                        ButtonsColorSet(ButtonFirewallRulesAddGame, 1, true);
                        ButtonsColorSet(ButtonFirewallRulesRemoveGame, 2, true);
                        DisableButtonFRRG = false;
                        FileSettingsSave.FirewallGameStatus = "Excluded";
                    }
                    else
                    { 
                        ButtonsColorSet(ButtonFirewallRulesAddGame, 3, false);
                        ButtonsColorSet(ButtonFirewallRulesRemoveGame, 3, false);
                        FileSettingsSave.FirewallGameStatus = "Error"; 
                    }

                    FileSettingsSave.SaveSettings();
                    
                    if (Firewall())
                    {
                        ButtonsColorSet(ButtonFirewallRulesAddAll, 1, true);
                        DisableButtonFRRA = ButtonFirewallRulesAddLauncher.Enabled && ButtonFirewallRulesAddGame.Enabled;
                        ButtonsColorSet(ButtonFirewallRulesRemoveAll, 2, DisableButtonFRRA);
                    }
                    else
                    {
                        ButtonsColorSet(ButtonFirewallRulesAddAll, 3, false);
                        ButtonsColorSet(ButtonFirewallRulesRemoveAll, 3, false);
                    }
                }
                else
                {
                    ButtonsColorSet(ButtonFirewallRulesAddAll, 3, false);
                }
            }
        }
        ///<summary>Button: Firewall Rules Add Launcher</summary>
        private void ButtonFirewallRulesAddLauncher_Click(object sender, EventArgs e)
        {
            if (!DisableButtonFRAL)
            {
                DisableButtonFRAL = true;

                if (ButtonEnabler(4, 20))
                {
                    ButtonsColorSet(ButtonFirewallRulesAddLauncher, 2, true);

                    /* Game */
                    if (ButtonEnabler(2, 0))
                    {
                        ButtonsColorSet(ButtonFirewallRulesAddLauncher, 1, true);
                        ButtonsColorSet(ButtonFirewallRulesRemoveLauncher, 2, true);
                        DisableButtonFRRL = false;
                        FileSettingsSave.FirewallGameStatus = "Excluded";
                    }
                    else
                    {
                        ButtonsColorSet(ButtonFirewallRulesAddLauncher, 3, false);
                        ButtonsColorSet(ButtonFirewallRulesRemoveLauncher, 3, false);
                        FileSettingsSave.FirewallGameStatus = "Error"; 
                    }

                    FileSettingsSave.SaveSettings();
                }
                else
                {
                    ButtonsColorSet(ButtonFirewallRulesAddLauncher, 3, false);
                }
            }
        }
        ///<summary>Button: Firewall Rules Add Game</summary>
        private void ButtonFirewallRulesAddGame_Click(object sender, EventArgs e)
        {
            if (!DisableButtonFRAG)
            {
                DisableButtonFRAG = true;

                if (ButtonEnabler(4, 20))
                {
                    ButtonsColorSet(ButtonFirewallRulesAddGame, 2, true);

                    /* Launcher & Updater */
                    if (ButtonEnabler(0, 0) && ButtonEnabler(1, 0))
                    {
                        ButtonsColorSet(ButtonFirewallRulesAddGame, 1, true);
                        ButtonsColorSet(ButtonFirewallRulesRemoveGame, 2, true);
                        DisableButtonFRRG = false;
                        FileSettingsSave.FirewallLauncherStatus = "Excluded";
                    }
                    else
                    { 
                        ButtonsColorSet(ButtonFirewallRulesAddGame, 3, false);
                        ButtonsColorSet(ButtonFirewallRulesRemoveGame, 3, false);
                        FileSettingsSave.FirewallLauncherStatus = "Error"; 
                    }

                    FileSettingsSave.SaveSettings();
                }
                else
                {
                    ButtonsColorSet(ButtonFirewallRulesAddGame, 3, false);
                }
            }
        }
        ///<summary>Button: Firewall Rules Remove All</summary>
        private void ButtonFirewallRulesRemoveAll_Click(object sender, EventArgs e)
        {
            if (!DisableButtonFRRA)
            {
                DisableButtonFRRA = true;

                if (ButtonEnabler(4, 20))
                {
                    ButtonsColorSet(ButtonFirewallRulesRemoveAll, 2, true);

                    /* Launcher & Updater */
                    if (ButtonEnabler(0, 1) && ButtonEnabler(1, 1))
                    {
                        ButtonsColorSet(ButtonFirewallRulesAddLauncher, 2, true);
                        DisableButtonFRAL = true;
                        ButtonsColorSet(ButtonFirewallRulesRemoveLauncher, 1, true);
                        FileSettingsSave.FirewallLauncherStatus = "Removed";
                    }
                    else
                    { 
                        ButtonsColorSet(ButtonFirewallRulesAddLauncher, 3, false);
                        ButtonsColorSet(ButtonFirewallRulesRemoveLauncher, 3, false);
                        FileSettingsSave.FirewallLauncherStatus = "Error"; 
                    }
                    /* Game */
                    if (ButtonEnabler(2, 1))
                    {
                        ButtonsColorSet(ButtonFirewallRulesAddGame, 2, true);
                        DisableButtonFRAG = true;
                        ButtonsColorSet(ButtonFirewallRulesRemoveGame, 1, true);
                        FileSettingsSave.FirewallGameStatus = "Removed";
                    }
                    else
                    { 
                        ButtonsColorSet(ButtonFirewallRulesAddGame, 3, false); 
                        ButtonsColorSet(ButtonFirewallRulesRemoveGame, 3, false); 
                        FileSettingsSave.FirewallGameStatus = "Error"; 
                    }

                    FileSettingsSave.SaveSettings();

                    if (Firewall())
                    {
                        ButtonsColorSet(ButtonFirewallRulesRemoveAll, 1, true);
                        DisableButtonFRAA = ButtonFirewallRulesRemoveLauncher.Enabled && ButtonFirewallRulesRemoveGame.Enabled;
                        ButtonsColorSet(ButtonFirewallRulesAddAll, 2, DisableButtonFRAA);
                    }
                    else
                    {
                        ButtonsColorSet(ButtonFirewallRulesRemoveAll, 3, false);
                        ButtonsColorSet(ButtonFirewallRulesAddAll, 3, false);
                    }
                }
                else
                {
                    ButtonsColorSet(ButtonFirewallRulesRemoveAll, 3, false);
                }
            }
        }
        ///<summary>Button: Firewall Rules Remove Launcher</summary>
        private void ButtonFirewallRulesRemoveLauncher_Click(object sender, EventArgs e)
        {
            if (!DisableButtonFRRL)
            {
                DisableButtonFRRL = true;

                if (ButtonEnabler(4, 20))
                {
                    ButtonsColorSet(ButtonFirewallRulesRemoveLauncher, 2, true);

                    /* Launcher & Updater */
                    if (ButtonEnabler(0, 1) && ButtonEnabler(1, 1))
                    {
                        ButtonsColorSet(ButtonFirewallRulesAddLauncher, 2, true);
                        DisableButtonFRAL = false;
                        ButtonsColorSet(ButtonFirewallRulesRemoveLauncher, 1, true);
                        FileSettingsSave.FirewallLauncherStatus = "Removed";
                    }
                    else
                    {
                        ButtonsColorSet(ButtonFirewallRulesAddLauncher, 3, false);
                        ButtonsColorSet(ButtonFirewallRulesRemoveLauncher, 3, false);
                        FileSettingsSave.FirewallLauncherStatus = "Error";
                    }

                    FileSettingsSave.SaveSettings();
                }
                else
                {
                    ButtonsColorSet(ButtonFirewallRulesRemoveLauncher, 3, false);
                }
            }
        }
        ///<summary>Button: Firewall Rules Remove Game</summary>
        private void ButtonFirewallRulesRemoveGame_Click(object sender, EventArgs e)
        {
            if (!DisableButtonFRRG)
            {
                DisableButtonFRRG = true;

                if (ButtonEnabler(4, 20))
                {
                    ButtonsColorSet(ButtonFirewallRulesRemoveGame, 0, true);

                    /* Game */
                    if (ButtonEnabler(2, 1))
                    {
                        ButtonsColorSet(ButtonFirewallRulesAddGame, 2, true);
                        DisableButtonFRAG = false;
                        ButtonsColorSet(ButtonFirewallRulesRemoveGame, 1, true);
                        FileSettingsSave.FirewallGameStatus = "Removed";
                    }
                    else
                    {
                        ButtonsColorSet(ButtonFirewallRulesAddGame, 3, false);
                        ButtonsColorSet(ButtonFirewallRulesRemoveGame, 3, false);
                        FileSettingsSave.FirewallGameStatus = "Error";
                    }

                    FileSettingsSave.SaveSettings();
                }
                else
                {
                    ButtonsColorSet(ButtonFirewallRulesRemoveGame, 3, false);
                }
            }
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ButtonClose_MouseDown(object sender, EventArgs e)
        {
            ButtonClose.BackgroundImage = Theming.CloseButtonClick;
        }

        private void ButtonClose_MouseEnter(object sender, EventArgs e)
        {
            ButtonClose.BackgroundImage = Theming.CloseButtonHover;
        }

        private void ButtonClose_MouseLeaveANDMouseUp(object sender, EventArgs e)
        {
            ButtonClose.BackgroundImage = Theming.CloseButton;
        }
        ///<summary>Theming, Function, EventHandlers, Etc. Meant to load critial functions before the forms loads</summary>
        private void SetVisuals()
        {
            /*******************************/
            /* Set Initial position & Icon  /
            /*******************************/

            FunctionStatus.CenterParent(this);

            /********************************/
            /* Set Theme Colors & Images     /
            /********************************/

            BackgroundImage = Theming.SecurityCenterScreen;
            TransparencyKey = Theming.SecurityCenterScreenTransparencyKey;
            ButtonClose.BackgroundImage = Theming.CloseButton;

            TextWindowsFirewall.ForeColor = Theming.FivithTextForeColor;

            /*******************************/
            /* Set Colored Buttons          /
            /*******************************/

            ButtonsColorSet(ButtonFirewallRulesAPI, 2, true);
            ButtonsColorSet(ButtonFirewallRulesCheck, 2017, false);
            ButtonsColorSet(ButtonFirewallRulesAddAll, 2017, false);
            ButtonsColorSet(ButtonFirewallRulesAddLauncher, 2017, false);
            ButtonsColorSet(ButtonFirewallRulesAddGame, 2017, false);
            ButtonsColorSet(ButtonFirewallRulesRemoveAll, 2017, false);
            ButtonsColorSet(ButtonFirewallRulesRemoveLauncher, 2017, false);
            ButtonsColorSet(ButtonFirewallRulesRemoveGame, 2017, false);

            /*******************************/
            /* Set Font                     /
            /*******************************/

            FontFamily DejaVuSans = FontWrapper.Instance.GetFontFamily("DejaVuSans.ttf");
            FontFamily DejaVuSansBold = FontWrapper.Instance.GetFontFamily("DejaVuSans-Bold.ttf");

            float MainFontSize = UnixOS.Detected() ? 9f : 9f * 100f / CreateGraphics().DpiY;
            float SecondaryFontSize = UnixOS.Detected() ? 8f : 8f * 100f / CreateGraphics().DpiY;

            Font = new Font(DejaVuSans, SecondaryFontSize, FontStyle.Regular);
            /* Text */
            TextWindowsFirewall.Font = new Font(DejaVuSansBold, MainFontSize, FontStyle.Bold);
            /* Firewall */
            ButtonFirewallRulesAPI.Font = new Font(DejaVuSansBold, SecondaryFontSize, FontStyle.Bold);
            ButtonFirewallRulesCheck.Font = new Font(DejaVuSansBold, SecondaryFontSize, FontStyle.Bold);
            ButtonFirewallRulesAddAll.Font = new Font(DejaVuSansBold, SecondaryFontSize, FontStyle.Bold);
            ButtonFirewallRulesAddLauncher.Font = new Font(DejaVuSansBold, SecondaryFontSize, FontStyle.Bold);
            ButtonFirewallRulesAddGame.Font = new Font(DejaVuSansBold, SecondaryFontSize, FontStyle.Bold);
            ButtonFirewallRulesRemoveAll.Font = new Font(DejaVuSansBold, SecondaryFontSize, FontStyle.Bold);
            ButtonFirewallRulesRemoveLauncher.Font = new Font(DejaVuSansBold, SecondaryFontSize, FontStyle.Bold);
            ButtonFirewallRulesRemoveGame.Font = new Font(DejaVuSansBold, SecondaryFontSize, FontStyle.Bold);

            /*******************************/
            /* Set Event Handlers           /
            /*******************************/

            /* Firewall Checks */
            ButtonFirewallRulesAPI.Click += new EventHandler(ButtonFirewallRulesAPI_Click);
            ButtonFirewallRulesCheck.Click += new EventHandler(ButtonFirewallRulesCheck_Click);
            /* Firewall Add */
            ButtonFirewallRulesAddAll.Click += new EventHandler(ButtonFirewallRulesAddAll_Click);
            ButtonFirewallRulesAddLauncher.Click += new EventHandler(ButtonFirewallRulesAddLauncher_Click);
            ButtonFirewallRulesAddGame.Click += new EventHandler(ButtonFirewallRulesAddGame_Click);
            /* Firewall Remove */
            ButtonFirewallRulesRemoveAll.Click += new EventHandler(ButtonFirewallRulesRemoveAll_Click);
            ButtonFirewallRulesRemoveLauncher.Click += new EventHandler(ButtonFirewallRulesRemoveLauncher_Click);
            ButtonFirewallRulesRemoveGame.Click += new EventHandler(ButtonFirewallRulesRemoveGame_Click);
            /* Close */
            ButtonClose.MouseEnter += new EventHandler(ButtonClose_MouseEnter);
            ButtonClose.MouseLeave += new EventHandler(ButtonClose_MouseLeaveANDMouseUp);
            ButtonClose.MouseUp += new MouseEventHandler(ButtonClose_MouseLeaveANDMouseUp);
            ButtonClose.MouseDown += new MouseEventHandler(ButtonClose_MouseDown);
            ButtonClose.Click += new EventHandler(ButtonClose_Click);
        }
    }
}
