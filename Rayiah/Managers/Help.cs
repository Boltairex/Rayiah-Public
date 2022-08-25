using Discord;
using System.Collections.Generic;
using System;
using System.Reflection;
using Rayiah.Objects.Attributes;
using System.Linq;
using Discord.Commands;
using Rayiah.Objects.Abstracts;

namespace Rayiah.Managers
{
    /// <summary>
    /// Singleton generujący instrukcje do komend.
    /// </summary>
    class Help : ManagerBase
    {
        public static Help Instance { get; private set; } = new Help();

        public static Help CreateInstance() => Instance;

        Dictionary<string, EmbedBuilder[]> helpEmbeds;

        Help()
        {
            helpEmbeds = new Dictionary<string, EmbedBuilder[]>();
            var helpCommand = new List<EmbedFieldBuilder>();

            Type t = typeof(HelpAttribute);
            foreach (Type type in Assembly.GetAssembly(t).GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.GetCustomAttribute(t) != null))
            {
                var attr = type.GetCustomAttribute<HelpAttribute>();
                List<EmbedBuilder> embeds = new List<EmbedBuilder>() { new EmbedBuilder().WithColor(attr.Color).WithDescription('_' + attr.Description + '_') };
                int embedSize = 0;
                int currEmbedIndex = 0;

                foreach (MethodInfo method in type.GetMethods())
                {
                    // Pre-Definiowanie
                    var commandName = method.GetCustomAttribute<CommandAttribute>();
                    var commandDesc = method.GetCustomAttribute<InfoAttribute>() ?? new InfoAttribute("None");
                    var commandAliasses = method.GetCustomAttribute<AliasAttribute>() ?? new AliasAttribute("None");
                    var arguments = method.GetParameters();
                    int sizeOfField;

                    if (commandDesc.Desc == "None") continue; // Skipowanie komend bez opisów.

                    // Generacja argumentów
                    string args = "Argumenty: ";
                    foreach (System.Reflection.ParameterInfo par in arguments)
                        args += '[' + par.ParameterType.Name + "] ";

                    // Generacja Aliasów
                    string aliasses = "";
                    if (commandAliasses.Aliases[0] != "None")
                        for (int x = 0; x < commandAliasses.Aliases.Length; x++)
                            aliasses += " " + commandAliasses.Aliases[x].ToUpper();

                    // Operacje
                    string embedName = commandDesc.Protected ? $"*{commandName.Text}" : $"{commandName.Text}";

                    embedName += commandAliasses.Aliases[0] != "None" ? " | " + aliasses : "";

                    string embedField = "";
                    if (arguments.Length != 0)
                        embedField += args + "\n";

                    embedField += commandDesc.Desc;
                    sizeOfField = embedName.Length + embedField.Length;

                    if (sizeOfField + embedSize >= 5800) {
                        currEmbedIndex++;
                        embedSize = 0;
                        embeds.Add(new EmbedBuilder().WithColor(attr.Color).WithDescription('_' + attr.Description + '_'));
                    }

                    embedSize += sizeOfField;
                    embeds[currEmbedIndex].AddField(embedName, embedField);
                }
/*
                // Pusty folder
                if (embeds[0].Fields.Count == 0)
                    Console.WriteLine("Skipping " + attr.Name + ", empty container.");*/

                // Gdy wszystko się powiedzie, załadowanie modułu do indexu 'Help'
                EmbedFieldBuilder helpBuilder = new EmbedFieldBuilder()
                    .WithName(attr.Name)
                    .WithValue(attr.Description.Split('\n')[0])
                    .WithIsInline(false);
                helpCommand.Add(helpBuilder);

                helpEmbeds.Add(attr.Name, embeds.ToArray());
            }

            // Generacja 'Help'
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(Color.Orange)
                .WithTitle("Spis bazy komend")
                .WithDescription("*Wszystkie generowane są automatycznie, więc jest szansa że coś nie zadziała... Ale mała.*")
                .WithFields(fields: helpCommand);

            helpEmbeds.Add("help", new EmbedBuilder[1] { embed });
        }


        public (bool, EmbedBuilder[]) GetEmbedBuilder(string name)
        {
            name = name.ToLower();
            if (helpEmbeds.ContainsKey(name) && helpEmbeds[name].First().Fields.Count != 0)
                return (true, helpEmbeds[name]);
            else
                return (false, null);
        }
    }
}
