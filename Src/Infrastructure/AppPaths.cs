using Neurocommenting;

namespace Neurocommenting.Infrastructure;

public static class AppPaths
{
    public static string Root => Path.GetFullPath("../../../", AppContext.BaseDirectory);
    
    // Folders
    public static string DataFolder => Path.Combine(Root, "Data");
    public static string LogsFolder => Path.Combine(DataFolder, "Logs");
    public static string ConfigFolder => Path.Combine(DataFolder, "Config");
    public static string AccountsFolder => Path.Combine(DataFolder, "Accounts");
    public static string GroupFolder (string groupName) => Path.Combine(AccountsFolder, groupName);
    public static string GroupFolder (Proxy proxy) 
        => Path.Combine(AccountsFolder, $"Group({proxy.Ip}_{proxy.Port})");
    public static string AccountFolder (string groupName, string accountId) 
        => Path.Combine(GroupFolder(groupName), $"Account({accountId})");

    // Files
    public static string AccountSettingsFile(string pathToAccount) => Path.Combine(pathToAccount, "Settings.json");
    public static string AccountSessionFile(string pathToAccount) => Path.Combine(pathToAccount, "Account.session");
    public static string LogFile(DateTime date) => Path.Combine(LogsFolder, $"{date.ToString("d")}.txt");
    public static string ChannelsFile => Path.Combine(ConfigFolder, "Channels.txt" );
    public static string PromptFile => Path.Combine(ConfigFolder, "Prompt.txt");
    public static string AppSettingsFile => Path.Combine(ConfigFolder, "Settings.json");
    public static string GroupProxyFile(string groupName) => Path.Combine(GroupFolder(groupName), "Proxy.txt");
    public static string GroupProxyFile (Proxy proxy) => Path.Combine(GroupFolder(proxy), "Proxy.txt");
}