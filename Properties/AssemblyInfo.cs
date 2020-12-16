
using System.Reflection;
using MelonLoader;

[assembly: AssemblyTitle(DiscordMute.BuildInfo.Description)]
[assembly: AssemblyDescription(DiscordMute.BuildInfo.Description)]
[assembly: AssemblyCompany(DiscordMute.BuildInfo.Company)]
[assembly: AssemblyProduct(DiscordMute.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + DiscordMute.BuildInfo.Author)]
[assembly: AssemblyTrademark(DiscordMute.BuildInfo.Company)]
[assembly: AssemblyVersion(DiscordMute.BuildInfo.Version)]
[assembly: AssemblyFileVersion(DiscordMute.BuildInfo.Version)]
[assembly: MelonInfo(typeof(DiscordMute.DiscordMute), DiscordMute.BuildInfo.Name, DiscordMute.BuildInfo.Version, DiscordMute.BuildInfo.Author, DiscordMute.BuildInfo.DownloadLink)]

[assembly: MelonGame("VRChat", "VRChat")]