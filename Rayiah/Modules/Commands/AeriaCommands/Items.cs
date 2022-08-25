using Discord.Commands;
using Rayiah.AeriaStories;
using Rayiah.Management;
using Rayiah.AeriaStories.Objects;
using static Rayiah.Tools.RayiahUtilites;
using System.Threading.Tasks;
using Rayiah.Tools;

namespace Rayiah.Modules.AeriaCommands
{
    public class Items : ModuleBase<SocketCommandContext>
    {
        [Command(">CreateItem")]//CreateItem "" "" "" 0
        public async Task CreateItem(string name, string type, string ID, int power, bool overwrite = false)
        {
            if (Container.BannedCharsFilter(name))
                await CreateMessage(this,$"Użyto niedozwolonych znaków: \n`{Container.getBannedCharactersInString}`",5000);
            else
            {
                EquipmentInfo eq = new EquipmentInfo()
                {
                    IID = ID,
                    power = power,
                    desc = "brak"
                };

                if (RayiahUtilites.CalculateSimilarity(type, "Helmet") > 0.90)
                {
                    eq.IID = $"H{eq.IID}";
                    if (IIDReader.isIDTaken(eq.IID)) { await CreateMessage(this, "ID jest przydzielone do istniejącego przedmiotu."); return; }
                    ObjectsLoader.SaveConfig(eq, name, EqType.Helmet, overwrite);
                }
                else if (RayiahUtilites.CalculateSimilarity(type, "Chestplate") > 0.90)
                {
                    eq.IID = $"C{eq.IID}";
                    if (IIDReader.isIDTaken(eq.IID)) { await CreateMessage(this, "ID jest przydzielone do istniejącego przedmiotu."); return; }
                    ObjectsLoader.SaveConfig(eq, name, EqType.Chestplate, overwrite);
                }
                else if (RayiahUtilites.CalculateSimilarity(type, "Leggins") > 0.90)
                {
                    eq.IID = $"L{eq.IID}";
                    if (IIDReader.isIDTaken(eq.IID)) { await CreateMessage(this, "ID jest przydzielone do istniejącego przedmiotu."); return; }
                    ObjectsLoader.SaveConfig(eq, name, EqType.Leggins, overwrite);
                }
                else if (RayiahUtilites.CalculateSimilarity(type, "Boots") > 0.90)
                { 
                    eq.IID = $"B{eq.IID}";
                    if (IIDReader.isIDTaken(eq.IID)) { await CreateMessage(this, "ID jest przydzielone do istniejącego przedmiotu."); return; }
                    ObjectsLoader.SaveConfig(eq, name, EqType.Boots, overwrite); 
                }
                else if (RayiahUtilites.CalculateSimilarity(type, "Weapon") > 0.90)
                { 
                    eq.IID = $"W{eq.IID}";
                    if (IIDReader.isIDTaken(eq.IID)) { await CreateMessage(this, "ID jest przydzielone do istniejącego przedmiotu."); return; }
                    ObjectsLoader.SaveConfig(eq, name, EqType.Weapon, overwrite); 
                }
                else if (RayiahUtilites.CalculateSimilarity(type, "SideWeapon") > 0.90)
                { 
                    eq.IID = $"V{eq.IID}";
                    if (IIDReader.isIDTaken(eq.IID)) { await CreateMessage(this, "ID jest przydzielone do istniejącego przedmiotu."); return; }
                    ObjectsLoader.SaveConfig(eq, name, EqType.SideWeapon, overwrite); 
                }

                await CreateMessage(this, "Tak");
            }
        }
    }
}
